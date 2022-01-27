using FragmentServerWV.Entities.Attributes;
using FragmentServerWV.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace FragmentServerWV.Entities.OpCodeHandlers.Data.Lobby
{
    [OpCodeData(OpCodes.OPCODE_DATA_LOBBY_STATUS_UPDATE), Description("Propagates updates from a Client to the entire Lobby")]
    public sealed class OPCODE_DATA_LOBBY_STATUS_UPDATE : NoResponseOpCodeHandler
    {
        private readonly ILobbyChatService lobbyChatService;

        public OPCODE_DATA_LOBBY_STATUS_UPDATE(ILobbyChatService lobbyChatService)
        {
            this.lobbyChatService = lobbyChatService ?? throw new ArgumentNullException(nameof(lobbyChatService));
        }

        public override async Task<IEnumerable<ResponseContent>> HandleIncomingRequestAsync(RequestContent request)
        {
            if (lobbyChatService.TryFindLobby(request.Client, out var rm))
            {
                await rm.UpdateLobbyStatusAsync(request.Data, request.Client.ClientIndex);
            }
            return await base.HandleIncomingRequestAsync(request);
        }
    }
}
