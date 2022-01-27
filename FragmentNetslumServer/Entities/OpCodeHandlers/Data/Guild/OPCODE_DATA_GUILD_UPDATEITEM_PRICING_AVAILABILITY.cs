using FragmentNetslumServer.Entities.Attributes;
using FragmentNetslumServer.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FragmentNetslumServer.Entities.OpCodeHandlers.Data.Guild
{
    [OpCodeData(OpCodes.OPCODE_DATA_GUILD_UPDATEITEM_PRICING_AVAILABILITY)]
    public sealed class OPCODE_DATA_GUILD_UPDATEITEM_PRICING_AVAILABILITY : IOpCodeHandler
    {
        public Task<IEnumerable<ResponseContent>> HandleIncomingRequestAsync(RequestContent request)
        {
            throw new NotImplementedException();
        }
    }
}
