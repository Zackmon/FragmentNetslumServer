using FragmentNetslumServer.Entities.Attributes;
using FragmentNetslumServer.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static FragmentNetslumServer.Services.Extensions;

namespace FragmentNetslumServer.Entities.OpCodeHandlers.Data.Guild
{
    [OpCodeData(OpCodes.OPCODE_DATA_GUILD_GET_LIST_OF_ITEMS)]
    public sealed class OPCODE_DATA_GUILD_GET_LIST_OF_ITEMS : IOpCodeHandler
    {
        private readonly IGuildManagementService guildManagementService;

        public OPCODE_DATA_GUILD_GET_LIST_OF_ITEMS(IGuildManagementService guildManagementService)
        {
            this.guildManagementService = guildManagementService;
        }

        public Task<IEnumerable<ResponseContent>> HandleIncomingRequestAsync(RequestContent request)
        {
            var responses = new List<ResponseContent>();
            var guildId = swap16(BitConverter.ToUInt16(request.Data, 0));
            bool isGeneral = guildId != request.Client._guildID;

            var listOfItemsForGeneralStore = guildManagementService.GetGuildItems(guildId, isGeneral);
            responses.Add(request.CreateResponse(OpCodes.OPCODE_DATA_GUILD_ITEMS_COUNT, BitConverter.GetBytes(swap16((ushort)listOfItemsForGeneralStore.Count))));

            foreach (var item in listOfItemsForGeneralStore)
            {
                responses.Add(request.CreateResponse(OpCodes.OPCODE_DATA_GUILD_ITEM_DETAILS, item));
            }

            return Task.FromResult<IEnumerable<ResponseContent>>(responses);
        }
    }
}
