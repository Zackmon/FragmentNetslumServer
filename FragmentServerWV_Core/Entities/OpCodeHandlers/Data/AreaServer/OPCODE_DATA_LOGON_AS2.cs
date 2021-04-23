using FragmentServerWV.Entities.Attributes;
using System.ComponentModel;

namespace FragmentServerWV.Entities.OpCodeHandlers.Data.AreaServer
{
    [OpCodeData(OpCodes.OPCODE_DATA_LOGON_AS2), Description("Handles an Area Server login request (I think)")]
    public sealed class OPCODE_DATA_LOGON_AS2 : SimpleResponseOpCodeHandler
    {
        public OPCODE_DATA_LOGON_AS2() : base(0x701C, new byte[] { 0x02, 0x11 }) { }
    }
}
