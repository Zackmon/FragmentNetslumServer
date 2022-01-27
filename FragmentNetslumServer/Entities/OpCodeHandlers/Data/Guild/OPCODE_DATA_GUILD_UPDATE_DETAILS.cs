using FragmentNetslumServer.Entities.Attributes;
using FragmentNetslumServer.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FragmentNetslumServer.Entities.OpCodeHandlers.Data.Guild
{
    [OpCodeData(OpCodes.OPCODE_DATA_GUILD_UPDATE_DETAILS)]
    public sealed class OPCODE_DATA_GUILD_UPDATE_DETAILS : IOpCodeHandler
    {
        public Task<IEnumerable<ResponseContent>> HandleIncomingRequestAsync(RequestContent request)
        {
            throw new NotImplementedException();
        }
    }
}
