using FragmentServerWV.Entities.Attributes;
using FragmentServerWV.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FragmentServerWV.Entities.OpCodeHandlers.Data
{
    [OpCodeData(OpCodes.OPCODE_DATA_LOGON_REPEAT)]
    public sealed class OPCODE_DATA_LOGON_REPEAT : IOpCodeHandler
    {
        public Task<IEnumerable<ResponseContent>> HandleIncomingRequestAsync(RequestContent request) =>
            Task.FromResult<IEnumerable<ResponseContent>>(new[] { new ResponseContent(
                request,
                OpCodes.OPCODE_DATA_LOGON_RESPONSE,
                new byte[] { 0x02, 0x10 },
                null)});
    }
}
