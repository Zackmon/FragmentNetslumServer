using FragmentServerWV.Entities.Attributes;

namespace FragmentServerWV.Entities.OpCodeHandlers.Data
{
    [OpCodeData(OpCodes.OPCODE_DATA_DISKID)]
    public sealed class OPCODE_DATA_DISKID : SimpleResponseOpCodeHandler
    {
        public OPCODE_DATA_DISKID() : base(OpCodes.OPCODE_DATA_DISKID_OK, new byte[] { 0x36, 0x36, 0x31, 0x36 }) { }
    }
}
