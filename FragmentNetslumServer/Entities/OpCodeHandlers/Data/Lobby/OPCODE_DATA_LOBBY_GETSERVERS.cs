using FragmentNetslumServer.Entities.Attributes;
using FragmentNetslumServer.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FragmentNetslumServer.Entities.OpCodeHandlers.Data.Lobby
{
    [OpCodeData(OpCodes.OPCODE_DATA_LOBBY_GETSERVERS)]
    public sealed class OPCODE_DATA_LOBBY_GETSERVERS : SimpleResponseOpCodeHandler
    {
        public OPCODE_DATA_LOBBY_GETSERVERS() : base(OpCodes.OPCODE_DATA_LOBBY_GETSERVERS_OK, new byte[] { 0x00, 0x00 })
        {
        }
    }
}
