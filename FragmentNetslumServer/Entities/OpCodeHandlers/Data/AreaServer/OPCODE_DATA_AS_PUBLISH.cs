using FragmentNetslumServer.Entities.Attributes;

namespace FragmentNetslumServer.Entities.OpCodeHandlers.Data.AreaServer
{
    [OpCodeData(OpCodes.OPCODE_DATA_AS_PUBLISH)]
    public sealed class OPCODE_DATA_AS_PUBLISH : SimpleResponseOpCodeHandler
    {
        public OPCODE_DATA_AS_PUBLISH() : base(OpCodes.OPCODE_DATA_AS_PUBLISH_OK, new byte[] { 0x00, 0x00 })
        {
        }
    }
}
