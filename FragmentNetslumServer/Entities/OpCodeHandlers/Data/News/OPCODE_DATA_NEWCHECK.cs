using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using FragmentNetslumServer.Entities.Attributes;
using FragmentNetslumServer.Services.Interfaces;

namespace FragmentNetslumServer.Entities.OpCodeHandlers.Data.News
{
    [OpCodeData(OpCodes.OPCODE_DATA_NEWCHECK) , Description("Check if there's new Articles to read")]
    public sealed class OPCODE_DATA_NEWCHECK : IOpCodeHandler
    {
        private readonly INewsService _newsService;
        private readonly Encoding _encoding;
        public OPCODE_DATA_NEWCHECK(INewsService newsService)
        {
            _newsService = newsService;
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            _encoding = Encoding.GetEncoding("Shift-JIS");
        }

        public async Task<IEnumerable<ResponseContent>> HandleIncomingRequestAsync(RequestContent request)
        {
            bool isNew = await _newsService.CheckIfNewNewsForSaveId(_encoding.GetString(request.Client.save_id));

            if (isNew)
            {
                return new[] { request.CreateResponse(OpCodes.OPCODE_DATA_NEWCHECK_OK, new byte[] { 0x00, 0x01 }) }; // send the new flag   
            }
            else
            {
                return new[] { request.CreateResponse(OpCodes.OPCODE_DATA_NEWCHECK_OK, new byte[] { 0x00, 0x00 }) }; //  there are no new articles to read
            }
        }
    }
}
