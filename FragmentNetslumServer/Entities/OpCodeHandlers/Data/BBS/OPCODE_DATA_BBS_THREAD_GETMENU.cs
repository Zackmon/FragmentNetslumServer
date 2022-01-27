using FragmentNetslumServer.Entities.Attributes;
using FragmentNetslumServer.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static FragmentNetslumServer.Services.Extensions;

namespace FragmentNetslumServer.Entities.OpCodeHandlers.Data.BBS
{
    [OpCodeData(OpCodes.OPCODE_DATA_BBS_THREAD_GETMENU)]
    public sealed class OPCODE_DATA_BBS_THREAD_GETMENU : IOpCodeHandler
    {
        private readonly IBulletinBoardService bulletinBoardService;

        public OPCODE_DATA_BBS_THREAD_GETMENU(IBulletinBoardService bulletinBoardService)
        {
            this.bulletinBoardService = bulletinBoardService ?? throw new ArgumentNullException(nameof(bulletinBoardService));
        }

        public async Task<IEnumerable<ResponseContent>> HandleIncomingRequestAsync(RequestContent request)
        {
            var responses = new List<ResponseContent>();
            var i = swap32(BitConverter.ToUInt32(request.Data, 0));
            var threadID = Convert.ToInt32(i);
            var postMetaList = await bulletinBoardService.GetThreadDetailsAsync(threadID);
            responses.Add(request.CreateResponse(OpCodes.OPCODE_DATA_BBS_THREAD_LIST, BitConverter.GetBytes(swap32((uint)postMetaList.Count))));
            foreach (var meta in postMetaList)
            {
                var postMetaBytes = await bulletinBoardService.ConvertThreadDetailsToBytesAsync(meta);
                responses.Add(request.CreateResponse(OpCodes.OPCODE_DATA_BBS_ENTRY_POST_META, postMetaBytes));
            }
            return responses;
        }
    }
}
