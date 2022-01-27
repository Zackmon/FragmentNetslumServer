using FragmentServerWV.Entities.Attributes;
using FragmentServerWV.Services;
using FragmentServerWV.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static FragmentServerWV.Services.Extensions;

namespace FragmentServerWV.Entities.OpCodeHandlers.Data.Guild
{
    [OpCodeData(OpCodes.OPCODE_DATA_GUILD_GET_LIST_OF_ITEMS)]
    public sealed class OPCODE_DATA_GUILD_GET_LIST_OF_ITEMS : IOpCodeHandler
    {
        public Task<IEnumerable<ResponseContent>> HandleIncomingRequestAsync(RequestContent request)
        {
            var responses = new List<ResponseContent>();
            var guildId = swap16(BitConverter.ToUInt16(request.Data, 0));
            var listOfItemsForGeneralStore = GuildManagementService.GetInstance().GetGuildItems(guildId, true);
            responses.Add(request.CreateResponse(OpCodes.OPCODE_DATA_GUILD_ITEMS_COUNT, BitConverter.GetBytes(swap16((ushort)listOfItemsForGeneralStore.Count))));

            foreach (var item in listOfItemsForGeneralStore)
            {
                responses.Add(request.CreateResponse(OpCodes.OPCODE_DATA_GUILD_ITEM_DETAILS, item));
            }

            return Task.FromResult<IEnumerable<ResponseContent>>(responses);
        }
    }
}
