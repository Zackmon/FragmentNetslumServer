using FragmentNetslumServer.Entities.Attributes;
using FragmentNetslumServer.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static FragmentNetslumServer.Services.Extensions;

namespace FragmentNetslumServer.Entities.OpCodeHandlers.Data.BBS
{
    [OpCodeData(OpCodes.OPCODE_DATA_BBS_THREAD_GET_CONTENT)]
    public sealed class OPCODE_DATA_BBS_THREAD_GET_CONTENT : IOpCodeHandler
    {
        private readonly IBulletinBoardService bulletinBoardService;

        public OPCODE_DATA_BBS_THREAD_GET_CONTENT(IBulletinBoardService bulletinBoardService)
        {
            this.bulletinBoardService = bulletinBoardService ?? throw new ArgumentNullException(nameof(bulletinBoardService));
        }

        public async Task<IEnumerable<ResponseContent>> HandleIncomingRequestAsync(RequestContent request)
        {
            var responses = new List<ResponseContent>();
            var q = swap32(BitConverter.ToUInt32(request.Data, 4));
            var postID = Convert.ToInt32(q);
            var bbsPostBody = await bulletinBoardService.GetThreadPostContentAsync(postID);
            var bbsPostData = await bulletinBoardService.ConvertThreadPostToBytesAsync(bbsPostBody);
            responses.Add(request.CreateResponse(0x781d, bbsPostData));
            return responses;
        }
    }
}
