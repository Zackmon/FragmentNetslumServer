using FragmentServerWV.Entities.Attributes;
using FragmentServerWV.Services.Interfaces;
using System;
using System.Threading.Tasks;

namespace FragmentServerWV.Entities.OpCodeHandlers.Regular
{
    [OpCode(OpCodes.OPCODE_KEY_EXCHANGE_ACKNOWLEDGMENT)]
    public sealed class OPCODE_KEY_EXCHANGE_ACKNOWLEDGMENT : IOpCodeHandler
    {
        public Task<ResponseContent> HandleIncomingRequestAsync(RequestContent request)
        {
            
        }
    }
}
