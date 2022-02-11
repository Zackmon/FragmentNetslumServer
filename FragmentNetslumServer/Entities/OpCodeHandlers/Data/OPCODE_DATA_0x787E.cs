using FragmentNetslumServer.Entities.Attributes;
using FragmentNetslumServer.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FragmentNetslumServer.Entities.OpCodeHandlers.Data
{
    [OpCodeData(0x787E)]
    public sealed class OPCODE_DATA_0x787E : SimpleResponseOpCodeHandler
    {
        public OPCODE_DATA_0x787E() : base(0x787F,new byte[] { 0x00, 0x00 })
        {
        }
    }
}
