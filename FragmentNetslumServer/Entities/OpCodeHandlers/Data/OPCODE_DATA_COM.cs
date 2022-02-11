using FragmentNetslumServer.Entities.Attributes;
using FragmentNetslumServer.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FragmentNetslumServer.Entities.OpCodeHandlers.Data
{
    [OpCodeData(OpCodes.OPCODE_DATA_COM)]
    public sealed class OPCODE_DATA_COM : SimpleResponseOpCodeHandler
    {
        
        public OPCODE_DATA_COM() : base(OpCodes.OPCODE_DATA_COM_OK, new byte[] { 0xDE, 0xAD })
        {
        }
    }
}
