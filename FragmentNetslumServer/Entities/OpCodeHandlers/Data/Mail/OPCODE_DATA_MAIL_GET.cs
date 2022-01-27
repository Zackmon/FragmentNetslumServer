using FragmentServerWV.Entities.Attributes;
using FragmentServerWV.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using static FragmentServerWV.Services.Extensions;

namespace FragmentServerWV.Entities.OpCodeHandlers.Data.Mail
{

    [OpCodeData(OpCodes.OPCODE_DATA_MAIL_GET), Description("Retrieves mail for a requesting client")]
    public sealed class OPCODE_DATA_MAIL_GET : IOpCodeHandler
    {
        private readonly IMailService mailService;

        public OPCODE_DATA_MAIL_GET(IMailService mailService)
        {
            this.mailService = mailService ?? throw new ArgumentNullException(nameof(mailService));
        }

        public async Task<IEnumerable<ResponseContent>> HandleIncomingRequestAsync(RequestContent request)
        {
            var responses = new List<ResponseContent>();
            var accountId = ReadAccountID(request.Data, 0);
            var mail = await mailService.GetMailAsync(accountId);
            responses.Add(request.CreateResponse(OpCodes.OPCODE_DATA_MAIL_GETOK, BitConverter.GetBytes(swap32((uint)mail.Count))));
            foreach (var item in mail)
            {
                var mailContent = await mailService.ConvertMailMetaIntoBytes(item);
                responses.Add(request.CreateResponse(OpCodes.OPCODE_DATA_MAIL_GET_NEWMAIL_HEADER, mailContent));
            }
            return responses;
        }

        static int ReadAccountID(byte[] data, int pos)
        {
            byte[] accountID = new byte[4];
            Buffer.BlockCopy(data, pos, accountID, 0, 4);
            return (int)swap32(BitConverter.ToUInt32(accountID));
        }

    }
}
