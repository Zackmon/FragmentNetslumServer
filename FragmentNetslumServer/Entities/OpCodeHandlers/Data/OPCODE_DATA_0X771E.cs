using FragmentServerWV.Entities.Attributes;

namespace FragmentServerWV.Entities.OpCodeHandlers.Data
{
    [OpCodeData(0x771E)]
    public sealed class OPCODE_DATA_0X771E : SimpleResponseOpCodeHandler
    {
        public OPCODE_DATA_0X771E() : base(0x771F, new byte[] { 0x00, 0x00 }) { }
    }
}
