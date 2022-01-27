using FragmentServerWV.Entities.Attributes;
using FragmentServerWV.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FragmentServerWV.Entities.OpCodeHandlers.Data
{
    [OpCodeData(OpCodes.OPCODE_DATA_RETURN_DESKTOP)]
    public sealed class OPCODE_DATA_RETURN_DESKTOP : SimpleResponseOpCodeHandler
    {
        public OPCODE_DATA_RETURN_DESKTOP() : base(OpCodes.OPCODE_DATA_RETURN_DESKTOP_OK, new byte[] { 0x00, 0x00 }) { }
        public override Task<IEnumerable<ResponseContent>> HandleIncomingRequestAsync(RequestContent request)
        {
            DBAcess.getInstance().setPlayerAsOffline(request.Client._characterPlayerID);
            return base.HandleIncomingRequestAsync(request);
        }
    }
}
