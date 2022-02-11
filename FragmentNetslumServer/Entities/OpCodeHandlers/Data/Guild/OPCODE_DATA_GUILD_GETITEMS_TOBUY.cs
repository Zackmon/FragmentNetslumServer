using FragmentNetslumServer.Entities.Attributes;
using FragmentNetslumServer.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FragmentNetslumServer.Services;

namespace FragmentNetslumServer.Entities.OpCodeHandlers.Data.Guild
{
    [OpCodeData(OpCodes.OPCODE_DATA_GUILD_GETITEMS_TOBUY)]
    public sealed class OPCODE_DATA_GUILD_GETITEMS_TOBUY : IOpCodeHandler
    {
        
        private readonly IGuildManagementService _guildManagementService;

        public OPCODE_DATA_GUILD_GETITEMS_TOBUY(IGuildManagementService guildManagementService)
        {
            _guildManagementService = guildManagementService;
        }
        
        public Task<IEnumerable<ResponseContent>> HandleIncomingRequestAsync(RequestContent request)
        {
            List<ResponseContent> responseContents = new List<ResponseContent>();
            
            var u = BitConverter.ToUInt16(request.Data, 0).Swap();
            List<byte[]> membersItemList = _guildManagementService.GetGuildItems(u, false);
            
            responseContents.Add(request.CreateResponse(0x7709,
                BitConverter.GetBytes(((ushort)membersItemList.Count).Swap()))); // number of items

            foreach (var item in membersItemList)
            {
                responseContents.Add(request.CreateResponse(0x770a, item));
            }
            
            return Task.FromResult<IEnumerable<ResponseContent>>(responseContents);
        }
    }
}
