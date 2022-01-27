using FragmentNetslumServer.Entities.Attributes;
using FragmentNetslumServer.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FragmentNetslumServer.Services;

namespace FragmentNetslumServer.Entities.OpCodeHandlers.Data.Guild
{
    [OpCodeData(OpCodes.OPCODE_DATA_INVITE_TO_GUILD)]
    public sealed class OPCODE_DATA_INVITE_TO_GUILD : IOpCodeHandler
    {
        
        private readonly ILobbyChatService _lobbyChatService;

        public OPCODE_DATA_INVITE_TO_GUILD(ILobbyChatService lobbyChatService)
        {
            _lobbyChatService = lobbyChatService;
            
        }
        public async Task<IEnumerable<ResponseContent>> HandleIncomingRequestAsync(RequestContent request)
        {
            var u = BitConverter.ToUInt16(request.Data, 0).Swap();
            // This is probably only possible in the MAIN lobby so
            if (_lobbyChatService.TryFindLobby(request.Client, out var lobby))
            {
                await lobby.InviteClientToGuildAsync(request.Data, request.Client.ClientIndex, u, request.Client._guildID);
                
                
                return new[]
                {
                    request.CreateResponse(0x772D, new byte[]{0x00 ,0x00})
                };//send to confirm that the player accepted the invite 
                
            }
            return new[]
            {
                ResponseContent.Empty
            };// failed to find lobby ???
        }
    }
}
