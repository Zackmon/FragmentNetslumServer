using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FragmentNetslumServer.Entities.Attributes;
using FragmentNetslumServer.Services;
using FragmentNetslumServer.Services.Interfaces;

namespace FragmentNetslumServer.Entities.OpCodeHandlers.Data.Ranking
{
    [OpCodeData(OpCodes.OPCODE_RANKING_VIEW_ALL)]
    public sealed class OPCODE_RANKING_VIEW_ALL : IOpCodeHandler
    {
        private readonly IRankingManagementService _rankingManagementService;

        public OPCODE_RANKING_VIEW_ALL(IRankingManagementService rankingManagementService)
        {
            _rankingManagementService = rankingManagementService;
        }

        public Task<IEnumerable<ResponseContent>> HandleIncomingRequestAsync(RequestContent request)
        {
            List<ResponseContent> responseContents = new List<ResponseContent>();
            var rankingArgs = (BitConverter.ToUInt16(request.Data, 0).Swap());
            if (rankingArgs == 0) // get the first ranking page
            {
                var rankCategoryList = _rankingManagementService.GetRankingCategory();
                
                responseContents.Add(request.CreateResponse(0x7833,
                    BitConverter.GetBytes(((ushort)rankCategoryList.Count).Swap())));

                foreach (var category in rankCategoryList)
                {
                    responseContents.Add(request.CreateResponse(0x7835, category));
                }
            }
            else if (rankingArgs >= 8) // get class List
            {
                request.Client._rankingCategoryID = rankingArgs;
                var rankClassList = Extensions.GetClassList();
                responseContents.Add(request.CreateResponse(0x7833, BitConverter.GetBytes(((ushort)rankClassList.Count).Swap())));
                foreach (var category in rankClassList)
                {
                     responseContents.Add(request.CreateResponse(0x7835, category));
                }
            }
            else
            {
                var playerRankingList = _rankingManagementService.GetPlayerRanking(request.Client._rankingCategoryID, rankingArgs);
                responseContents.Add(request.CreateResponse(0x7836, BitConverter.GetBytes(((uint)playerRankingList.Count).Swap())));
                foreach (var player in playerRankingList)
                {
                     responseContents.Add(request.CreateResponse(0x7837, player));
                }
            }

            return Task.FromResult<IEnumerable<ResponseContent>>(responseContents);
        }
    }
}
