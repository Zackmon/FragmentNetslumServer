using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FragmentNetslumServer.Entities.Attributes;
using FragmentNetslumServer.Services;
using FragmentNetslumServer.Services.Interfaces;

namespace FragmentNetslumServer.Entities.OpCodeHandlers.Data.Ranking
{
    [OpCodeData(OpCodes.OPCODE_RANKING_VIEW_PLAYER)]
    public sealed class OPCODE_RANKING_VIEW_PLAYER : IOpCodeHandler
    {
        private readonly IRankingManagementService _rankingManagementService;

        public OPCODE_RANKING_VIEW_PLAYER(IRankingManagementService rankingManagementService)
        {
            _rankingManagementService = rankingManagementService;
        }

        public Task<IEnumerable<ResponseContent>> HandleIncomingRequestAsync(RequestContent request)
        {
            uint rankPlayerID = BitConverter.ToUInt32(request.Data, 0).Swap();
            ResponseContent response = request.CreateResponse(0x7839, _rankingManagementService.GetRankingPlayerInfo(rankPlayerID));

            return Task.FromResult<IEnumerable<ResponseContent>>(new [] {response});
        }
    }
}
