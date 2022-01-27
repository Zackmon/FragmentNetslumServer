using FragmentServerWV.Entities.Attributes;

namespace FragmentServerWV.Entities.OpCodeHandlers.Data.BBS
{
    [OpCodeData(OpCodes.OPCODE_DATA_BBS_GET_UPDATES)]
    public sealed class OPCODE_DATA_BBS_GET_UPDATES : SimpleResponseOpCodeHandler
    {
        public OPCODE_DATA_BBS_GET_UPDATES() : base(0x786b, new byte[] { 0x00, 0x00 }) { }
    }
}
