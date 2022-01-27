using FragmentNetslumServer.Entities.Attributes;

namespace FragmentNetslumServer.Entities.OpCodeHandlers.Data
{
    [OpCodeData(OpCodes.OPCODE_DATA_LOGON_REPEAT)]
    public sealed class OPCODE_DATA_LOGON_REPEAT : SimpleResponseOpCodeHandler
    {
        public OPCODE_DATA_LOGON_REPEAT() : base(OpCodes.OPCODE_DATA_LOGON_RESPONSE, new byte[] { 0x02, 0x10 }) { }
    }
}
