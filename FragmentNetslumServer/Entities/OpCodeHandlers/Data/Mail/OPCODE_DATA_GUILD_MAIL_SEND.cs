using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using FragmentNetslumServer.Entities.Attributes;
using FragmentNetslumServer.Services.Interfaces;

namespace FragmentNetslumServer.Entities.OpCodeHandlers.Data.Mail;

[OpCodeData(OpCodes.OPCODE_DATA_GUILD_MAIL_SEND), Description("Send a mail to all the guild members")]
public sealed class OPCODE_DATA_GUILD_MAIL_SEND : SimpleResponseOpCodeHandler
{
    private readonly IMailService mailService;

    public OPCODE_DATA_GUILD_MAIL_SEND(IMailService mailService) : base(OpCodes.OPCODE_DATA_GUILD_MAIL_SEND_OK, new byte[] { 0x00, 0x00 })
    {
        this.mailService = mailService ?? throw new ArgumentNullException(nameof(mailService));
    }
    public override async Task<IEnumerable<ResponseContent>> HandleIncomingRequestAsync(RequestContent request)
    {
        await mailService.SaveGuildMailAsync(request.Data);
        return await base.HandleIncomingRequestAsync(request);
    }
}