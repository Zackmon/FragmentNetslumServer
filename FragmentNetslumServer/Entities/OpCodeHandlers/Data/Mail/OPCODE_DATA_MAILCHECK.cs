using FragmentNetslumServer.Entities.Attributes;
using FragmentNetslumServer.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FragmentNetslumServer.Services;

namespace FragmentNetslumServer.Entities.OpCodeHandlers.Data.Mail
{
    [OpCodeData(OpCodes.OPCODE_DATA_MAILCHECK)]
    public sealed class OPCODE_DATA_MAILCHECK : IOpCodeHandler
    {
        public async Task<IEnumerable<ResponseContent>> HandleIncomingRequestAsync(RequestContent request)
        {
            bool isNew = await Task.Run(() =>
                DBAccess.getInstance().checkForNewMailByAccountID(Extensions.ReadAccountId(request.Data, 0)));
            ResponseContent responseContent;
            if (isNew)
            {
                responseContent = request.CreateResponse(OpCodes.OPCODE_DATA_MAILCHECK_OK,
                    new byte[] { 0x00, 0x00, 0x01, 0x00 });
            }
            else
            {
                responseContent = request.CreateResponse(OpCodes.OPCODE_DATA_MAILCHECK_OK, new byte[] { 0x00, 0x01 });
            }

            return new[] { responseContent };
        }
    }
}
