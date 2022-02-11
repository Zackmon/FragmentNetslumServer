using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using FragmentNetslumServer.Entities.Attributes;
using FragmentNetslumServer.Models;
using FragmentNetslumServer.Services;
using FragmentNetslumServer.Services.Interfaces;

namespace FragmentNetslumServer.Entities.OpCodeHandlers.Data.News
{
    [OpCodeData(OpCodes.OPCODE_DATA_NEWS_GETMENU) , Description("Get List of Articles")]
    public sealed class OPCODE_DATA_NEWS_GETMENU : IOpCodeHandler
    {
        private readonly INewsService _newsService;
        private readonly Encoding _encoding;

        public OPCODE_DATA_NEWS_GETMENU(INewsService newsService)
        {
            _newsService = newsService;
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            _encoding = Encoding.GetEncoding("Shift-JIS");
        }

        public async Task<IEnumerable<ResponseContent>> HandleIncomingRequestAsync(RequestContent request)
        {
            var u = BitConverter.ToUInt16(request.Data, 0).Swap(); // Get CategoryID

            // we are skipping category listing and we will list only the articles , but either way here is the code for it 

            // Ignore categories only send the article list
            /*if (u == 0)
            {
                ushort count = 1;
                await SendDataPacket(OpCodes.OPCODE_DATA_NEWS_CATEGORYLIST, BitConverter.GetBytes(swap16(count)));

                ushort catID = 1;
                string catName = "Testing Category";
                using MemoryStream memoryStream = new MemoryStream();
                await memoryStream.WriteAsync(BitConverter.GetBytes(swap16(catID)));
                await memoryStream.WriteAsync(encoding.GetBytes(catName + char.MinValue));
                await SendDataPacket(OpCodes.OPCODE_DATA_NEWS_ENTRY_CATEGORY, memoryStream.ToArray());
            } else*/
            List<ResponseContent> listOfResponse = new List<ResponseContent>();
            
            // send articles
            ushort count = (ushort)(await _newsService.GetNewsArticles()).Count;
            listOfResponse.Add(request.CreateResponse(OpCodes.OPCODE_DATA_NEWS_ARTICLELIST,
                BitConverter.GetBytes((count.Swap()))));

            foreach (NewsSectionModel article in
                     (await _newsService.GetNewsArticles(
                         _encoding.GetString(request.Client
                             .save_id)))) // get the articles data and set the isNew flag based on the saveID
            {
                listOfResponse.Add(request.CreateResponse(OpCodes.OPCODE_DATA_NEWS_ENTRY_ARTICLE,
                    article.ArticleByteArray));
            }

            return listOfResponse;
        }
    }
}