using FragmentNetslumServer.Entities.Attributes;
using FragmentNetslumServer.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FragmentNetslumServer.Services;
using Serilog;

namespace FragmentNetslumServer.Entities.OpCodeHandlers.Data.Guild
{
    [OpCodeData(OpCodes.OPCODE_DATA_GUILD_TAKE_ITEM)]
    public sealed class OPCODE_DATA_GUILD_TAKE_ITEM : IOpCodeHandler
    {
        private readonly ILogger _logger;
        private readonly IGuildManagementService _guildManagementService;

        public OPCODE_DATA_GUILD_TAKE_ITEM(ILogger logger,IGuildManagementService guildManagementService)
        {
            _logger = logger;
            _guildManagementService = guildManagementService;
            
        }
        public Task<IEnumerable<ResponseContent>> HandleIncomingRequestAsync(RequestContent request)
        {
            ushort guildIDTakeItem = BitConverter.ToUInt16(request.Data, 0).Swap();
            uint itemIDToTakeOut = BitConverter.ToUInt32(request.Data, 2).Swap();
            ushort quantityToTake = BitConverter.ToUInt16(request.Data, 6).Swap();
            
            _logger.Debug("Guild ID {guildIDTakeItem}\nItem ID to take {itemIDToTakeOut}\n quantity to take out {quantityToTake}" ,guildIDTakeItem , itemIDToTakeOut,quantityToTake);
            
            return Task.FromResult<IEnumerable<ResponseContent>>(new[]
            {
                request.CreateResponse(0x7711,
                    _guildManagementService.TakeItemFromGuild(guildIDTakeItem, itemIDToTakeOut,quantityToTake))
            });
        }
    }
}
