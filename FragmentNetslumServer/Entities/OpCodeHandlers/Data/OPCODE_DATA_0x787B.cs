using FragmentServerWV.Entities.Attributes;

namespace FragmentServerWV.Entities.OpCodeHandlers.Data
{
    [OpCodeData(0x787B)]
    public sealed class OPCODE_DATA_0x787B : SimpleResponseOpCodeHandler
    {
        public OPCODE_DATA_0x787B() : base(0x787C, new byte[] { 0x00, 0x00 })
        {
        }
    }
}
