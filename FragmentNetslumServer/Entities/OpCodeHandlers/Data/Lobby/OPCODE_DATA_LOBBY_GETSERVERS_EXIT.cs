using FragmentNetslumServer.Entities.Attributes;
using FragmentNetslumServer.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FragmentNetslumServer.Entities.OpCodeHandlers.Data.Lobby
{
    [OpCodeData(OpCodes.OPCODE_DATA_LOBBY_GETSERVERS_EXIT)]
    public sealed class OPCODE_DATA_LOBBY_GETSERVERS_EXIT : SimpleResponseOpCodeHandler
    {
        public OPCODE_DATA_LOBBY_GETSERVERS_EXIT() : base(OpCodes.OPCODE_DATA_LOBBY_GETSERVERS_EXIT_OK, new byte[] { 0x00, 0x00 })
        {
        }
    }
}
