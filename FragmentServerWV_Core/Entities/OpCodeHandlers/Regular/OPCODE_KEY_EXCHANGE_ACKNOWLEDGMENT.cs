using FragmentServerWV.Entities.Attributes;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FragmentServerWV.Entities.OpCodeHandlers.Regular
{
    [OpCode(OpCodes.OPCODE_KEY_EXCHANGE_ACKNOWLEDGMENT)]
    public sealed class OPCODE_KEY_EXCHANGE_ACKNOWLEDGMENT : NoResponseOpCodeHandler
    {
        public override Task<IEnumerable<ResponseContent>> HandleIncomingRequestAsync(RequestContent request)
        {
            request.Client.InitializeDecryptionKey(request.Client.from_key, true);
            request.Client.InitializeEncryptionKey(request.Client.to_key, true);
            return base.HandleIncomingRequestAsync(request);
        }
    }
}
