using FragmentNetslumServer.Entities.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using static FragmentNetslumServer.Services.Extensions;

namespace FragmentNetslumServer.Entities.OpCodeHandlers.Data.AreaServer
{
    [OpCodeData(OpCodes.OPCODE_DATA_AS_UPDATE_USERNUM), Description("Updates how many players are currently on an Area Server")]
    public sealed class OPCODE_DATA_AS_UPDATE_USERNUM : NoResponseOpCodeHandler
    {
        public override Task<IEnumerable<ResponseContent>> HandleIncomingRequestAsync(RequestContent request)
        {
            request.Client.as_usernum = swap16(BitConverter.ToUInt16(request.Data, 2));
            return base.HandleIncomingRequestAsync(request);
        }
    }
}
