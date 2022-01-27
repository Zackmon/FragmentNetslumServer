using FragmentNetslumServer.Entities.Attributes;
using FragmentNetslumServer.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FragmentNetslumServer.Entities.OpCodeHandlers.Data.Lobby
{
    [OpCodeData(OpCodes.OPCODE_DATA_LOBBY_EVENT)]
    public sealed class OPCODE_DATA_LOBBY_EVENT : NoResponseOpCodeHandler
    {
        private readonly ILobbyChatService _lobbyChatService;

        public OPCODE_DATA_LOBBY_EVENT(ILobbyChatService lobbyChatService)
        {
            _lobbyChatService = lobbyChatService;
        }

        public override async Task<IEnumerable<ResponseContent>> HandleIncomingRequestAsync(RequestContent request)
        {
            if (_lobbyChatService.TryFindLobby(request.Client, out var lcr))
            {
                await lcr.SendPublicMessageAsync(request.Data, request.Client.ClientIndex);
            }
            return await base.HandleIncomingRequestAsync(request);
        }
    }
}
