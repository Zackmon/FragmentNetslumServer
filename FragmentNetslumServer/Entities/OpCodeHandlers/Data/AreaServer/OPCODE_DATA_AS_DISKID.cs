using System.ComponentModel;
using FragmentNetslumServer.Entities.Attributes;

namespace FragmentNetslumServer.Entities.OpCodeHandlers.Data.AreaServer
{
    [OpCodeData(OpCodes.OPCODE_DATA_AS_DISKID) , Description("Area Server DISK ID Check")]
    public sealed class OPCODE_DATA_AS_DISKID : SimpleResponseOpCodeHandler
    {
        public OPCODE_DATA_AS_DISKID() : base(OpCodes.OPCODE_DATA_AS_DISKID_OK, new byte[] { 0x00, 0x00 })
        {
        }
    }
}
