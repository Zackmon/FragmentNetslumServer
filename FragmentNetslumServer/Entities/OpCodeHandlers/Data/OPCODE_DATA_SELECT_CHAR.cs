using FragmentNetslumServer.Entities.Attributes;
using FragmentNetslumServer.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FragmentNetslumServer.Entities.OpCodeHandlers.Data
{
    [OpCodeData(OpCodes.OPCODE_DATA_SELECT_CHAR)]
    public sealed class OPCODE_DATA_SELECT_CHAR : SimpleResponseOpCodeHandler
    {
        public OPCODE_DATA_SELECT_CHAR() : base(OpCodes.OPCODE_DATA_SELECT_CHAROK, new byte[] { 0x00, 0x00 })
        {
        }
    }
}
