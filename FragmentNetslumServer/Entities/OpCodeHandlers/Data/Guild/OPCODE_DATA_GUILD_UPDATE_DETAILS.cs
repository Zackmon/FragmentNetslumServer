using FragmentNetslumServer.Entities.Attributes;
using FragmentNetslumServer.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FragmentNetslumServer.Entities.OpCodeHandlers.Data.Guild
{
    [OpCodeData(OpCodes.OPCODE_DATA_GUILD_UPDATE_DETAILS)]
    public sealed class OPCODE_DATA_GUILD_UPDATE_DETAILS : IOpCodeHandler
    {
        private readonly IGuildManagementService _guildManagementService;

        public OPCODE_DATA_GUILD_UPDATE_DETAILS(IGuildManagementService guildManagementService)
        {
            _guildManagementService = guildManagementService;
            
        }
        
        public Task<IEnumerable<ResponseContent>> HandleIncomingRequestAsync(RequestContent request)
        {
            return Task.FromResult<IEnumerable<ResponseContent>>(new[]
            {
                request.CreateResponse(0x761D,
                    _guildManagementService.UpdateGuildEmblemComment(request.Data,request.Client._guildID))
            });
        }
    }
}
