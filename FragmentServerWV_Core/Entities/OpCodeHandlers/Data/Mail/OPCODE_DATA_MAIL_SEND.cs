using FragmentServerWV.Entities.Attributes;
using FragmentServerWV.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace FragmentServerWV.Entities.OpCodeHandlers.Data.Mail
{

    [OpCodeData(OpCodes.OPCODE_DATA_MAIL_SEND), Description("Sends mail from one player to another")]
    public sealed class OPCODE_DATA_MAIL_SEND : SimpleResponseOpCodeHandler
    {
        private readonly IMailService mailService;

        public OPCODE_DATA_MAIL_SEND(IMailService mailService) : base(OpCodes.OPCODE_DATA_MAIL_SEND_OK, new byte[] { 0x00, 0x00 })
        {
            this.mailService = mailService ?? throw new ArgumentNullException(nameof(mailService));
        }

        public override async Task<IEnumerable<ResponseContent>> HandleIncomingRequestAsync(RequestContent request)
        {
            // Do I need to do this still???
            //var buffer = new byte[4096];
            //while (ns.DataAvailable) await ns.ReadAsync(buffer, 0, buffer.Length);
            await mailService.SaveMailAsync(request.Data);
            return await base.HandleIncomingRequestAsync(request);
        }
    }
}
