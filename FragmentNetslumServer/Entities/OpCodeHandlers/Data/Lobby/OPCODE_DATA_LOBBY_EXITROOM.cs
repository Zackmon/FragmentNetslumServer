using FragmentNetslumServer.Entities.Attributes;
using FragmentNetslumServer.Services.Interfaces;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace FragmentNetslumServer.Entities.OpCodeHandlers.Data.Lobby
{
    [OpCodeData(OpCodes.OPCODE_DATA_LOBBY_EXITROOM), Description("Announces to the client's current Lobby that it is leaving")]
    public sealed class OPCODE_DATA_LOBBY_EXITROOM : SimpleResponseOpCodeHandler
    {
        private readonly ILobbyChatService lobbyChatService;

        public OPCODE_DATA_LOBBY_EXITROOM(ILobbyChatService lobbyChatService) : base(OpCodes.OPCODE_DATA_LOBBY_EXITROOM_OK, new byte[] { 0x00, 0x00 })
        {
            this.lobbyChatService = lobbyChatService ?? throw new System.ArgumentNullException(nameof(lobbyChatService));
        }

        public override async Task<IEnumerable<ResponseContent>> HandleIncomingRequestAsync(RequestContent request)
        {
            if (lobbyChatService.TryFindLobby(request.Client, out var lobby))
            {
                await lobbyChatService.AnnounceRoomDeparture(lobby, (uint)request.Client.ClientIndex);
            }
            return await base.HandleIncomingRequestAsync(request);
        }
    }
}
