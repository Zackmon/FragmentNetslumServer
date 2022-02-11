using FragmentNetslumServer.Entities.Attributes;
using FragmentNetslumServer.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FragmentNetslumServer.Entities.OpCodeHandlers.Data
{
    [OpCodeData(OpCodes.OPCODE_DATA_SELECT2_CHAR)]
    public sealed class OPCODE_DATA_SELECT2_CHAR : SimpleResponseOpCodeHandler
    {
        public OPCODE_DATA_SELECT2_CHAR() : base(OpCodes.OPCODE_DATA_SELECT2_CHAROK, new byte[] { 0x30, 0x30, 0x30, 0x30 })
        {
        }
    }
}
