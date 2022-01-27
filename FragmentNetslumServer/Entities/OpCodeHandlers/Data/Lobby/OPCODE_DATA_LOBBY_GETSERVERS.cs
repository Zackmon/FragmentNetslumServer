using FragmentNetslumServer.Entities.Attributes;
using FragmentNetslumServer.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FragmentNetslumServer.Entities.OpCodeHandlers.Data.Lobby
{
    [OpCodeData(OpCodes.OPCODE_DATA_LOBBY_GETSERVERS)]
    public sealed class OPCODE_DATA_LOBBY_GETSERVERS : IOpCodeHandler
    {
        public Task<IEnumerable<ResponseContent>> HandleIncomingRequestAsync(RequestContent request)
        {
            throw new NotImplementedException();
        }
    }
}
