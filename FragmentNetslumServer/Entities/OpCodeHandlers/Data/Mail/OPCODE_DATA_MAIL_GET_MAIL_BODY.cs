using FragmentNetslumServer.Entities.Attributes;
using FragmentNetslumServer.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using static FragmentNetslumServer.Services.Extensions;

namespace FragmentNetslumServer.Entities.OpCodeHandlers.Data.Mail
{
    [OpCodeData(OpCodes.OPCODE_DATA_MAIL_GET_MAIL_BODY), Description("Returns the full message body for the specified mail")]
    public sealed class OPCODE_DATA_MAIL_GET_MAIL_BODY : IOpCodeHandler
    {
        private readonly IMailService mailService;

        public OPCODE_DATA_MAIL_GET_MAIL_BODY(IMailService mailService)
        {
            this.mailService = mailService ?? throw new ArgumentNullException(nameof(mailService));
        }

        public async Task<IEnumerable<ResponseContent>> HandleIncomingRequestAsync(RequestContent request)
        {
            var mailId = (int)swap32(BitConverter.ToUInt32(request.Data, 4));
            var messageBodyModel = await mailService.GetMailContent(mailId);
            var messageBody = await mailService.ConvertMailBodyIntoBytes(messageBodyModel);
            return new[] { request.CreateResponse(OpCodes.OPCODE_DATA_MAIL_GET_MAIL_BODY_RESPONSE, messageBody) };
        }
    }
}
