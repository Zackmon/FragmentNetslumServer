using FragmentNetslumServer.Entities.Attributes;

namespace FragmentNetslumServer.Entities.OpCodeHandlers.Data
{
    [OpCodeData(OpCodes.OPCODE_DATA_UNREGISTER_CHAR)]
    public sealed class OPCODE_DATA_UNREGISTER_CHAR : SimpleResponseOpCodeHandler
    {
        public OPCODE_DATA_UNREGISTER_CHAR() : base(OpCodes.OPCODE_DATA_UNREGISTER_CHAROK, new byte[] { 0x00, 0x00 }) { }
    }
}
