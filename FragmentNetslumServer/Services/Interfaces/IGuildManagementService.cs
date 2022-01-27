using System.Collections.Generic;

namespace FragmentNetslumServer.Services.Interfaces
{
    public interface IGuildManagementService : IBaseService
    {
        byte[] AddItemToGuildInventory(ushort guildID, uint itemID, ushort itemQuantity, uint generalPrice, uint memberPrice, bool isGeneral, bool isMember, bool isGuildMaster);
        byte[] BuyItemFromGuild(byte[] argument);
        ushort CreateGuild(byte[] argument, uint masterPlayerID);
        byte[] DestroyGuild(ushort guildID);
        byte[] DonateCoinsToGuild(byte[] argument);
        List<byte[]> GetAllGuildItemsWithSettings(ushort guildID);
        byte[] GetGuildInfo(ushort guildID);
        List<byte[]> GetGuildItems(ushort guildId, bool isGeneral);
        List<byte[]> GetGuildMembersListByClass(ushort guildID, ushort categoryID, uint playerID);
        byte[] GetItemDonationSettings(bool isMaster);
        List<byte[]> GetListOfGuilds();
        byte[] GetPlayerGuild(uint characterID);
        byte[] GetPriceOfItemToBeDonated(ushort guildID, uint itemID);
        byte[] KickPlayerFromGuild(ushort guildID, uint playerToKick);
        byte[] LeaveGuild(ushort guildID, uint characterID);
        byte[] LeaveGuildAndAssignMaster(ushort guildID, uint playerToAssign);
        byte[] ReadByteGuildEmblem(byte[] data, int pos);
        byte[] ReadByteString(byte[] data, int pos);
        byte[] SetItemVisibilityAndPrice(byte[] argument);
        byte[] TakeItemFromGuild(ushort guildID, uint itemID, ushort quantity);
        byte[] TakeMoneyFromGuild(ushort guildID, uint amountOfMoney);
        byte[] UpdateGuildEmblemComment(byte[] argument, ushort guildID);
    }
}