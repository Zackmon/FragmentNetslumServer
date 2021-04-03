using FragmentServerWV.Entities;
using FragmentServerWV.Services;
using FragmentServerWV.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace FragmentServerWV
{
    public class LobbyChatRoom
    {
        public string name;
        public ushort ID;
        public ushort type;
        private readonly IClientProviderService clientProviderService;
        public List<int> Users;

        /// <summary>
        /// Creates a new Lobby Room
        /// </summary>
        /// <param name="desc">The name / description of the lobby</param>
        /// <param name="id">The ID of the lobby</param>
        /// <param name="t">The type of lobby</param>
        public LobbyChatRoom(
            string desc,
            ushort id,
            ushort t,
            IClientProviderService clientProviderService)
        {
            Users = new List<int>();
            name = desc;
            ID = id;
            type = t;
            this.clientProviderService = clientProviderService;
        }


        public async Task DispatchAllStatusAsync(int clientIndex)
        {
            if (!clientProviderService.TryGetClient((uint)clientIndex, out var client)) return;
            foreach (var c in clientProviderService.Clients)
                if (c.ClientIndex != clientIndex && c.LobbyIndex == ID)
                    await client.SendDataPacket(0x7009, c.last_status);
        }

        

        /// <summary>
        /// Updates all clients in the lobby room with the incoming client's new status
        /// </summary>
        /// <param name="data">The data to submit</param>
        /// <param name="clientIndex">The client the data is originating from</param>
        public async Task DispatchStatusAsync(byte[] data, int clientIndex)
        {
            try
            {
                var m = new MemoryStream();
                await m.WriteAsync(BitConverter.GetBytes(((ushort)FindRoomIndexById(clientIndex)).Swap()), 0, 2);
                await m.WriteAsync(BitConverter.GetBytes(((ushort)(data.Length)).Swap()), 0, 2);
                await m.WriteAsync(data, 0, data.Length);
                byte[] buff = m.ToArray();

                foreach (var client in clientProviderService.Clients)
                {
                    if (!client.isAreaServer && client.ClientIndex != clientIndex && client.LobbyIndex == ID)
                    {
                        await client.SendDataPacket(0x7009, buff);
                    }
                    else if (client.ClientIndex == clientIndex)
                    {
                        client.last_status = buff;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                //throw;
            }
        }

        public async Task SendPublicMessageAsync(byte[] data, int clientIndex)
        {
            try
            {
                int id = FindRoomIndexById(clientIndex);
                byte[] temp = new byte[data.Length];
                data.CopyTo(temp, 0);
                foreach (var client in clientProviderService.Clients)
                {
                    if (!client.isAreaServer && client.LobbyIndex == ID && client.ClientIndex != clientIndex)
                    {
                        temp[0] = (byte)(id >> 8);
                        temp[1] = (byte)(id & 0xff);
                        await client.SendDataPacket(0x7862, temp);
                    }
                    else if (client.ClientIndex == clientIndex)
                    {
                        temp[0] = 0xff;
                        temp[1] = 0xff;
                        await client.SendDataPacket(0x7862, temp);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                //throw;
            }
        }

        public async Task SendDirectMessageAsync(byte[] data, int sourceClientIndex, int destinationClientIndex)
        {

            int srcid = FindRoomIndexById(sourceClientIndex);
            int towho = Users[destinationClientIndex - 1];

            if (clientProviderService.TryGetClient((uint)towho, out var toWhoClient) &&
                clientProviderService.TryGetClient((uint)sourceClientIndex, out var whoClient))
            {

                // Send to towho first
                byte[] temp = new byte[data.Length];
                data.CopyTo(temp, 0);
                temp[2] = (byte)(srcid >> 8);
                temp[3] = (byte)(srcid & 0xff);
                await toWhoClient.SendDataPacket(OpCodes.OPCODE_PRIVATE_BROADCAST, temp);

                // Now to the other
                temp = new byte[data.Length];
                data.CopyTo(temp, 0);
                temp[2] = 0xff;
                temp[3] = 0xff;
                await whoClient.SendDataPacket(OpCodes.OPCODE_PRIVATE_BROADCAST, temp);

            }

        }

        public async Task SendServerMessageAsync(byte[] data)
        {
            foreach (var client in clientProviderService.Clients)
            {
                if (client.LobbyIndex != ID) continue;
                await client.SendDataPacket(0x7862, data);
            }
        }

        public async Task SendServerMessageAsync(string message)
        {
            using var m = new MemoryStream();
            var e = Encoding.GetEncoding("Shift-JIS");

            // this first write is a standard payload for a message that will appear as if the player
            // send it along to the server. It's roughly 16 bytes
            await m.WriteAsync(new byte[] { 255, 255, 0, 12, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });

            // now get the message payload
            await m.WriteAsync(e.GetBytes(message));

            // and send it off
            await SendServerMessageAsync(m.ToArray());
        }


        public async Task InviteClientToGuildAsync(byte[] data, int inviter, int invitee, ushort guildID)
        {
            int srcid = FindRoomIndexById(inviter);
            int towho = Users[invitee - 1];

            if (clientProviderService.TryGetClient((uint)towho, out var recipient))
            {
                byte[] temp = new byte[data.Length];
                data.CopyTo(temp, 0);
                MemoryStream m = new MemoryStream();
                await m.WriteAsync(BitConverter.GetBytes(((ushort)srcid).Swap()));
                await recipient.SendDataPacket(OpCodes.ARGUMENT_INVITE_TO_GUILD, m.ToArray()); // Guild Inviation OPCode
            }
            
        }

        public async Task ClientLeavingRoomAsync(int clientIndex)
        {
            MemoryStream m = new MemoryStream();
            await m.WriteAsync(BitConverter.GetBytes(((ushort)FindRoomIndexById(clientIndex)).Swap()), 0, 2);
            byte[] buff = m.ToArray();

            foreach (var client in clientProviderService.Clients)
            {
                if (client.isAreaServer) continue;
                if (client.ClientIndex == clientIndex) continue;
                if (client.currentLobbyIndex != ID) continue;
                await client.SendDataPacket(OpCodes.OPCODE_CLIENT_LEAVING_LOBBY, buff);
            }
        }



        private int FindRoomIndexById(int id)
        {
            int result = -1;
            for (int i = 0; i < Users.Count; i++)
                if (Users[i] == id)
                    return i + 1;
            return result;
        }
    }
}
