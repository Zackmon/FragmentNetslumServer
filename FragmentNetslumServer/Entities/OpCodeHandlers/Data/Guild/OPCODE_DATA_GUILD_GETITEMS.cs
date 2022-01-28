using FragmentNetslumServer.Entities.Attributes;
using FragmentNetslumServer.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FragmentNetslumServer.Services;

namespace FragmentNetslumServer.Entities.OpCodeHandlers.Data.Guild
{
    [OpCodeData(OpCodes.OPCODE_DATA_GUILD_GETITEMS)]
    public sealed class OPCODE_DATA_GUILD_GETITEMS : IOpCodeHandler
    {
        private readonly IGuildManagementService _guildManagementService;

        public OPCODE_DATA_GUILD_GETITEMS(IGuildManagementService guildManagementService)
        {
            _guildManagementService = guildManagementService;
        }

        public Task<IEnumerable<ResponseContent>> HandleIncomingRequestAsync(RequestContent request)
        {
            List<ResponseContent> responseContents = new List<ResponseContent>();
            
            var u = BitConverter.ToUInt16(request.Data, 0).Swap();
            
            List<byte[]> allGuildItems = _guildManagementService.GetAllGuildItemsWithSettings(u);
            
            responseContents.Add(request.CreateResponse(0x7729,
                BitConverter.GetBytes(((ushort)allGuildItems.Count).Swap()))); // number of items

            foreach (var item in allGuildItems)
            {
                 responseContents.Add(request.CreateResponse(0x772A, item));
            }

            return Task.FromResult<IEnumerable<ResponseContent>>(responseContents);
        }
    }
}
