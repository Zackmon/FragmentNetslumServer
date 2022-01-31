using FragmentNetslumServer.Entities.Attributes;
using FragmentNetslumServer.Services.Interfaces;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using static FragmentNetslumServer.Services.Extensions;

namespace FragmentNetslumServer.Entities.OpCodeHandlers.Data.Lobby
{

    [OpCodeData(OpCodes.OPCODE_DATA_LOBBY_ENTERROOM), Description("Informs a Lobby that a new Client has joined")]
    public sealed class OPCODE_DATA_LOBBY_ENTERROOM : IOpCodeHandler
    {
        private readonly ILobbyChatService lobbyChatService;
        private readonly ILogger logger;

        public OPCODE_DATA_LOBBY_ENTERROOM(ILobbyChatService lobbyChatService, ILogger logger)
        {
            this.lobbyChatService = lobbyChatService ?? throw new ArgumentNullException(nameof(lobbyChatService));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<ResponseContent>> HandleIncomingRequestAsync(RequestContent request)
        {
            LobbyChatRoom room;
            var currentLobbyIndex = (short)swap16(BitConverter.ToUInt16(request.Data, 0));
            var currentLobbyType = swap16(BitConverter.ToUInt16(request.Data, 2));
            logger.Verbose("Lobby Room ID: {@room_index}", currentLobbyIndex);
            logger.Verbose("Lobby Type ID: {@lobbyType}", currentLobbyType);

            if (currentLobbyType == OpCodes.LOBBY_TYPE_GUILD) //Guild Room
            {
                //TODO add Guild Specific Code
                room = lobbyChatService.GetOrAddLobby((ushort)currentLobbyIndex, "Guild Room", OpCodes.LOBBY_TYPE_GUILD, out var _);
            }
            else
            {
                lobbyChatService.TryGetLobby((ushort)currentLobbyIndex, out room);
            }

            var response = request.CreateResponse(OpCodes.OPCODE_DATA_LOBBY_ENTERROOM_OK, BitConverter.GetBytes(swap16((ushort)room.Clients.Count)));
            await room.ClientJoinedLobbyAsync(request.Client);
            logger.Information("Client #{0} has joined Lobby {1}. There are now {2} client(s) in the room", request.Client.ClientIndex, room.Name, room.Clients.Count );
            return new[] { response };
        }
    }
}
