using FragmentNetslumServer.Entities.Attributes;
using FragmentNetslumServer.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static FragmentNetslumServer.Services.Extensions;

namespace FragmentNetslumServer.Entities.OpCodeHandlers.Data.Lobby
{

    [OpCodeData(OpCodes.OPCODE_DATA_LOBBY_GETMENU), Description("Returns a list of available lobbies and participants")]
    public sealed class OPCODE_DATA_LOBBY_GETMENU : IOpCodeHandler
    {
        private readonly ILobbyChatService lobbyChatService;

        public OPCODE_DATA_LOBBY_GETMENU(ILobbyChatService lobbyChatService)
        {
            this.lobbyChatService = lobbyChatService ?? throw new System.ArgumentNullException(nameof(lobbyChatService));
        }

        public async Task<IEnumerable<ResponseContent>> HandleIncomingRequestAsync(RequestContent request)
        {
            var responses = new List<ResponseContent>();
            var nonGuildLobbies = new List<LobbyChatRoom>(lobbyChatService.Lobbies.Values.Where(c => c.Type == OpCodes.LOBBY_TYPE_MAIN));

            responses.Add(request.CreateResponse(OpCodes.OPCODE_DATA_LOBBY_LOBBYLIST, BitConverter.GetBytes(swap16((ushort)nonGuildLobbies.Count))));
            foreach (var room in nonGuildLobbies)
            {
                var m = new MemoryStream();
                await m.WriteAsync(BitConverter.GetBytes(swap16(room.ID)), 0, 2);
                foreach (char c in room.Name)
                    m.WriteByte((byte)c);
                m.WriteByte(0);
                await m.WriteAsync(BitConverter.GetBytes(swap16((ushort)room.Clients.Count)), 0, 2);
                await m.WriteAsync(BitConverter.GetBytes(swap16((ushort)(room.Clients.Count + 1))), 0, 2);
                // looks like some form of padding to align the message
                while (((m.Length + 2) % 8) != 0) m.WriteByte(0);
                responses.Add(request.CreateResponse(OpCodes.OPCODE_DATA_LOBBY_ENTRY_LOBBY, m.ToArray()));
            }

            return responses;
        }
    }
}
