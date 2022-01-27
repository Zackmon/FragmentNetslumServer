using FragmentServerWV.Entities.Attributes;
using FragmentServerWV.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FragmentServerWV.Entities.OpCodeHandlers.Data.Lobby
{
    [OpCodeData(OpCodes.OPCODE_DATA_LOBBY_CHATROOM_GETLIST)]
    public sealed class OPCODE_DATA_LOBBY_CHATROOM_GETLIST : IOpCodeHandler
    {
        public Task<IEnumerable<ResponseContent>> HandleIncomingRequestAsync(RequestContent request) =>
            Task.FromResult<IEnumerable<ResponseContent>>(new[]
            {
                request.CreateResponse(OpCodes.OPCODE_DATA_LOBBY_CHATROOM_CATEGORY, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }),
                request.CreateResponse(OpCodes.OPCODE_DATA_LOBBY_CHATROOM_CATEGORY, new byte[] { 0x00, 0x01, 0x00, 0x00 })
            });
    }
}
