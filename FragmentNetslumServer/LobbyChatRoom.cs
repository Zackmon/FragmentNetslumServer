using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using FragmentNetslumServer.Entities;
using FragmentNetslumServer.Services;

namespace FragmentNetslumServer
{

    /// <summary>
    /// Represents a lobby where users are talking in
    /// </summary>
    public sealed class LobbyChatRoom
    {

        private const int MAXIMUM_MESSAGE_LENGTH = 30;

        private readonly string name;
        private readonly ushort id;
        private readonly ushort type;
        private readonly ConcurrentDictionary<int, GameClientAsync> clients;


        public string Name => name;
        public ushort ID => id;
        public ushort Type => type;

        public ReadOnlyCollection<GameClientAsync> Clients => new List<GameClientAsync>(this.clients.Values).AsReadOnly();


        /// <summary>
        /// Creates a new Lobby Room
        /// </summary>
        /// <param name="desc">The name / description of the lobby</param>
        /// <param name="id">The ID of the lobby</param>
        /// <param name="t">The type of lobby</param>
        public LobbyChatRoom(
            string desc,
            ushort id,
            ushort t)
        {
            this.name = desc;
            this.id = id;
            this.type = t;
            this.clients = new ConcurrentDictionary<int, GameClientAsync>();
        }



        /// <summary>
        /// Sends all the currently logged in clients information to the newly joined client
        /// </summary>
        /// <param name="client">The client that is joining</param>
        /// <returns>A promise to submit all information to the client</returns>
        public async Task ClientJoinedLobbyAsync(GameClientAsync client)
        {
            // Tell the incoming client who all is in the lobby
            foreach (var existing in this.clients)
            {
                await client.SendDataPacket(OpCodes.OPCODE_DATA_LOBBY_STATUS_UPDATE, existing.Value.last_status ?? new byte[0]);
            }
            this.clients.AddOrUpdate(client.ClientIndex, client, (index, existing) => client);
        }
        
        /// <summary>
        /// Sends all the currently logged in clients information to the newly joined client (Aggregate Response for packet handler)
        /// </summary>
        /// <param name="client">The client that is joining</param>
        /// <returns>A promise to submit all information to the client</returns>
        public IEnumerable<ResponseContent> ClientJoinedLobbyPacketHandler(GameClientAsync client, RequestContent request)
        {
            List<ResponseContent> responseContents = new List<ResponseContent>();
            // Tell the incoming client who all is in the lobby
            foreach (var existing in this.clients)
            {
                responseContents.Add(request.CreateResponse(OpCodes.OPCODE_DATA_LOBBY_STATUS_UPDATE, existing.Value.last_status ?? new byte[0]));
            }
            this.clients.AddOrUpdate(client.ClientIndex, client, (index, existing) => client);
            return responseContents;
        }

        /// <summary>
        /// Updates all clients in the lobby room with the incoming client's new status
        /// </summary>
        /// <param name="data">The data to submit</param>
        /// <param name="clientIndex">The client the data is originating from</param>
        public async Task UpdateLobbyStatusAsync(byte[] data, int clientIndex)
        {
            try
            {
                var m = new MemoryStream();
                await m.WriteAsync(BitConverter.GetBytes(((ushort)FindClientOffsetByClientIndex(clientIndex)).Swap()), 0, 2);
                await m.WriteAsync(BitConverter.GetBytes(((ushort)(data.Length)).Swap()), 0, 2);
                await m.WriteAsync(data, 0, data.Length);
                byte[] buff = m.ToArray();

                foreach (var client in clients.Values)
                {
                    if (client.ClientIndex != clientIndex)
                    {
                        await client.SendDataPacket(OpCodes.OPCODE_DATA_LOBBY_STATUS_UPDATE, buff);
                    }
                    else
                    {
                        client.last_status = buff;
                    }
                }
            }
            catch (Exception e)
            {
                // TODO: Support ILogger in LobbyChatRoom
                Console.WriteLine(e);
            }
        }

        /// <summary>
        /// Sends a public message to the lobby
        /// </summary>
        /// <param name="data">The data payload to send</param>
        /// <param name="clientIndex">The index of the client that sent the payload</param>
        /// <returns>A promse to send the message to all individuals in the lobby</returns>
        public async Task SendPublicMessageAsync(byte[] data, int clientIndex)
        {
            try
            {
                int id = FindClientOffsetByClientIndex(clientIndex);
                byte[] temp = new byte[data.Length];
                data.CopyTo(temp, 0);
                foreach (var clientKvp in clients)
                {
                    var index = clientKvp.Key;
                    var client = clientKvp.Value;

                    if (index != clientIndex)
                    {
                        temp[0] = (byte)(id >> 8);
                        temp[1] = (byte)(id & 0xff);
                        await client.SendDataPacket(0x7862, temp);
                    }
                    else
                    {
                        temp[0] = 0xff;
                        temp[1] = 0xff;
                        await client.SendDataPacket(0x7862, temp);
                    }
                }
            }
            catch (Exception e)
            {
                // TODO: Support ILogger in LobbyChatRoom
                Console.WriteLine(e);
            }
        }

        /// <summary>
        /// Sends a direct message from one client to another
        /// </summary>
        /// <param name="data">The data payload to send</param>
        /// <param name="sourceClientIndex">The originating client</param>
        /// <param name="destinationId">The destination client in the LIST of clients on the lobby screen</param>
        /// <returns>A promise to send the message to the intended recipient</returns>
        public async Task SendDirectMessageAsync(byte[] data, int sourceClientIndex, int destinationId)
        {
            var srcid = FindClientOffsetByClientIndex(sourceClientIndex);
            var destination = GetClientAtIndex(destinationId - 1);
            
            if (destination != null && clients.TryGetValue(sourceClientIndex, out var whoClient))
            {
                // Send to towho first
                byte[] temp = new byte[data.Length];
                data.CopyTo(temp, 0);
                temp[2] = (byte)(srcid >> 8);
                temp[3] = (byte)(srcid & 0xff);
                await destination.SendDataPacket(OpCodes.OPCODE_PRIVATE_BROADCAST, temp);

                // Now to the other
                temp = new byte[data.Length];
                data.CopyTo(temp, 0);
                temp[2] = 0xff;
                temp[3] = 0xff;
                await whoClient.SendDataPacket(OpCodes.OPCODE_PRIVATE_BROADCAST, temp);
            }
        }

        /// <summary>
        /// Sends a server message that looks like the player sent it
        /// </summary>
        /// <param name="data">The data payload to send</param>
        /// <returns>A promise to send the message to all connected clients</returns>
        public async Task SendServerMessageAsync(byte[] data)
        {
            foreach (var client in clients.Values)
            {
                await client.SendDataPacket(0x7862, data);
            }
        }

        /// <summary>
        /// Sends a server message that looks like the player sent it
        /// </summary>
        /// <param name="message">The message to send</param>
        /// <returns>A promise to send the message to all connected clients</returns>
        public async Task SendServerMessageAsync(string message)
        {
            // we will probably need to chunk the message up if it's too big
            if (message.Length > MAXIMUM_MESSAGE_LENGTH)
            {
                var chunkIndex = 1;
                foreach (var chunk in message.ChunksUpto(MAXIMUM_MESSAGE_LENGTH - 4))
                {
                    await SendServerMessageAsync($"{chunkIndex++}. {chunk}");
                }
                return;
            }

            using var m = new MemoryStream();
            var e = Encoding.GetEncoding("Shift-JIS");

            // this first write is a standard payload for a message that will appear as if the player
            // send it along to the server. It's roughly 16 bytes
            await m.WriteAsync(new byte[] { 255, 255, 0, 12, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });

            // now get the message payload
            m.WriteByte(32); // whitespace
            await m.WriteAsync(e.GetBytes(message));
            m.WriteByte(0); // terminate with zero

            // and send it off
            await SendServerMessageAsync(m.ToArray());
        }

        /// <summary>
        /// Sends a guild invitation to a connected person in the lobby
        /// </summary>
        /// <param name="data">The invitation payload</param>
        /// <param name="inviter">The client sending the invitation</param>
        /// <param name="invitee">The client receiving the invitation</param>
        /// <param name="guildID">The ID of the guild</param>
        /// <returns>A promise to send the invitation to the invitee</returns>
        public async Task InviteClientToGuildAsync(byte[] data, int inviter, int invitee, ushort guildID)
        {
            var srcid = FindClientOffsetByClientIndex(inviter);
            var inviteeClient = GetClientAtIndex(invitee - 1);
            byte[] temp = new byte[data.Length];
            data.CopyTo(temp, 0);
            MemoryStream m = new MemoryStream();
            await m.WriteAsync(BitConverter.GetBytes(((ushort)srcid).Swap()));
            await inviteeClient.SendDataPacket(OpCodes.ARGUMENT_INVITE_TO_GUILD, m.ToArray()); // Guild Inviation OPCode
        }

        /// <summary>
        /// Announces that a client is now leaving the lobby
        /// </summary>
        /// <param name="clientIndex">The ID of the client that is leaving</param>
        /// <returns>A promise to send the departure to all other clients</returns>
        public async Task ClientLeavingRoomAsync(int clientIndex)
        {
            MemoryStream m = new MemoryStream();
            await m.WriteAsync(BitConverter.GetBytes(((ushort)FindClientOffsetByClientIndex(clientIndex)).Swap()), 0, 2);
            byte[] buff = m.ToArray();
            this.clients.TryRemove(clientIndex, out _);
            foreach (var client in clients.Values)
            {
                await client.SendDataPacket(OpCodes.OPCODE_CLIENT_LEAVING_LOBBY, buff);
            }
        }


        /// <summary>
        /// Locates the client's offset in the player list
        /// </summary>
        /// <param name="clientIndex">The client index</param>
        /// <returns></returns>
        /// <remarks>
        /// The player list on the right-hand side of the lobby is responsible for showing who is logged in. This should, in theory, match up with who is logged in
        /// </remarks>
        private int FindClientOffsetByClientIndex(int clientIndex)
        {
            int result = -1;
            var count = 0;
            foreach (var client in this.clients)
            {
                if (client.Key == clientIndex) return count + 1;
                count++;
            }
            return result;
        }


        private GameClientAsync GetClientAtIndex(int index)
        {
            var count = 0;
            foreach(var item in this.clients)
            {
                if (count == index) return item.Value;
                count++;
            }
            return null;
        }
    }
}
