using FragmentNetslumServer.Entities.Attributes;
using FragmentNetslumServer.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FragmentNetslumServer.Services;

namespace FragmentNetslumServer.Entities.OpCodeHandlers.Data.Guild
{
    [OpCodeData(OpCodes.OPCODE_DATA_GUILD_DONATE_ITEM)]
    public sealed class OPCODE_DATA_GUILD_DONATE_ITEM : IOpCodeHandler
    {
        private readonly IGuildManagementService _guildManagementService;

        public OPCODE_DATA_GUILD_DONATE_ITEM(IGuildManagementService guildManagementService)
        {
            _guildManagementService = guildManagementService;
        }

        public Task<IEnumerable<ResponseContent>> HandleIncomingRequestAsync(RequestContent request)
        {
            request.Client._itemDontationID = BitConverter.ToUInt32(request.Data, 2).Swap();
            request.Client._itemDonationQuantity = BitConverter.ToUInt16(request.Data, 6).Swap();
            
            return Task.FromResult<IEnumerable<ResponseContent>>(new [] {request.CreateResponse(0x7704,
                _guildManagementService.GetPriceOfItemToBeDonated(request.Client._guildID,
                    request.Client._itemDontationID))});
        }
    }
}
