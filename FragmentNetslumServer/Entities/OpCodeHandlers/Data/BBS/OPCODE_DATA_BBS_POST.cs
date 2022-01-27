using FragmentNetslumServer.Entities.Attributes;
using FragmentNetslumServer.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static FragmentNetslumServer.Services.Extensions;

namespace FragmentNetslumServer.Entities.OpCodeHandlers.Data.BBS
{
    [OpCodeData(OpCodes.OPCODE_DATA_BBS_POST)]
    public sealed class OPCODE_DATA_BBS_POST : SimpleResponseOpCodeHandler
    {
        public OPCODE_DATA_BBS_POST() : base(0x7813, new byte[] { 0x00, 0x00 }) { }

        public override Task<IEnumerable<ResponseContent>> HandleIncomingRequestAsync(RequestContent request)
        {
            var id = swap32(BitConverter.ToUInt32(request.Data, 0));
            DBAccess.getInstance().CreateNewPost(request.Data, id);
            return base.HandleIncomingRequestAsync(request);
        }
    }
}
