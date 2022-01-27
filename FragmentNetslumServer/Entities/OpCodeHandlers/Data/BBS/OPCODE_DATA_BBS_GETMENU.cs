using FragmentServerWV.Entities.Attributes;
using FragmentServerWV.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using static FragmentServerWV.Services.Extensions;

namespace FragmentServerWV.Entities.OpCodeHandlers.Data.BBS
{
    [OpCodeData(OpCodes.OPCODE_DATA_BBS_GETMENU)]
    public sealed class OPCODE_DATA_BBS_GETMENU : IOpCodeHandler
    {
        private readonly IBulletinBoardService bulletinBoardService;

        public OPCODE_DATA_BBS_GETMENU(IBulletinBoardService bulletinBoardService)
        {
            this.bulletinBoardService = bulletinBoardService ?? throw new ArgumentNullException(nameof(bulletinBoardService));
        }

        public async Task<IEnumerable<ResponseContent>> HandleIncomingRequestAsync(RequestContent request)
        {
            var responses = new List<ResponseContent>();
            var u = swap16(BitConverter.ToUInt16(request.Data, 0));
            if (u == 0)
            {
                var categoryList = await bulletinBoardService.GetCategoriesAsync();
                responses.Add(request.CreateResponse(OpCodes.OPCODE_DATA_BBS_CATEGORYLIST, BitConverter.GetBytes(swap16((ushort)categoryList.Count))));
                foreach (var category in categoryList)
                {
                    var categoryData = await bulletinBoardService.ConvertCategoryToBytesAsync(category);
                    responses.Add(request.CreateResponse(OpCodes.OPCODE_DATA_BBS_ENTRY_CATEGORY, categoryData));
                }
            }
            else
            {
                var categoryID = Convert.ToInt32(u);
                var threadsList = await bulletinBoardService.GetThreadsAsync(categoryID);
                responses.Add(request.CreateResponse(OpCodes.OPCODE_DATA_BBS_THREADLIST, BitConverter.GetBytes(swap16((ushort)threadsList.Count))));
                foreach (var thread in threadsList)
                {
                    var threadData = await bulletinBoardService.ConvertThreadToBytesAsync(thread);
                    responses.Add(request.CreateResponse(OpCodes.OPCODE_DATA_BBS_ENTRY_THREAD, threadData));
                }
            }
            return responses;
        }
    }
}
