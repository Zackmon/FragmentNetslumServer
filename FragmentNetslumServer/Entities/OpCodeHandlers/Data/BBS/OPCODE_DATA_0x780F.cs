using FragmentServerWV.Entities.Attributes;

namespace FragmentServerWV.Entities.OpCodeHandlers.Data.BBS
{
    [OpCodeData(0x780F)]
    public sealed class OPCODE_DATA_0x780F : SimpleResponseOpCodeHandler
    {
        public OPCODE_DATA_0x780F() : base(0x7810, new byte[] { 0x01, 0x92 }) { }
    }
}
