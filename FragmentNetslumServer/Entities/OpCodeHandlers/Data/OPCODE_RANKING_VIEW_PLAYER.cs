using FragmentNetslumServer.Entities.Attributes;
using FragmentNetslumServer.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FragmentNetslumServer.Entities.OpCodeHandlers.Data
{
    [OpCodeData(OpCodes.OPCODE_RANKING_VIEW_PLAYER)]
    public sealed class OPCODE_RANKING_VIEW_PLAYER : IOpCodeHandler
    {
        public Task<IEnumerable<ResponseContent>> HandleIncomingRequestAsync(RequestContent request)
        {
            throw new NotImplementedException();
        }
    }
}
