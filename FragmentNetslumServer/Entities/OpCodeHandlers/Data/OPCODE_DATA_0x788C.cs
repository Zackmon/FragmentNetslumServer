using FragmentNetslumServer.Entities.Attributes;
using FragmentNetslumServer.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using FragmentNetslumServer.Services;

namespace FragmentNetslumServer.Entities.OpCodeHandlers.Data
{
    [OpCodeData(0x788C) , Description("Send a Direct message in the lobby ")]
    public sealed class OPCODE_DATA_0x788C : NoResponseOpCodeHandler
    {
        private readonly ILobbyChatService _lobbyChatService;
        public OPCODE_DATA_0x788C(ILobbyChatService lobbyChatService)
        {
            _lobbyChatService = lobbyChatService;
        }

        public override async Task<IEnumerable<ResponseContent>> HandleIncomingRequestAsync(RequestContent request)
        {
            var destid = BitConverter.ToUInt16(request.Data, 2).Swap();
            if (_lobbyChatService.TryFindLobby(request.Client, out var p))
            {
                await p.SendDirectMessageAsync(request.Data, request.Client.ClientIndex, destid);
            }

            return await base.HandleIncomingRequestAsync(request);
        }
    }
}
