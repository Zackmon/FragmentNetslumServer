using System;
using System.Collections.Generic;
using System.IO;
using FragmentNetslumServer.Models;

namespace FragmentNetslumServer.Services
{
    public class GuildManagementService : BaseManagementService
    {
        private static GuildManagementService _instance = null;

        //private Encoding _encoding;

        public GuildManagementService() : base()
        {

        }

        public static GuildManagementService GetInstance()
        {
            if (_instance == null)
            {
                _instance = new GuildManagementService();
            }

            return _instance;
        }


        public byte[] GetPlayerGuild(uint characterID)
        {
            lock (this)
            {
                CharacterRepositoryModel characterRepositoryModel = DBAccess.getInstance().GetCharacterInfo(characterID);

                MemoryStream m = new MemoryStream();

                if (characterRepositoryModel.GuildMaster == 1) // the player is guild master
                {
                    m.Write(new byte[] { 0x01 });
                    m.Write(BitConverter.GetBytes(((ushort)characterRepositoryModel.GuildID).Swap()));
                }
                else if (characterRepositoryModel.GuildMaster == 2) // the player is a normal member
                {
                    m.Write(new byte[] { 0x02 });
                    m.Write(BitConverter.GetBytes(((ushort)characterRepositoryModel.GuildID).Swap()));
                }
                else
                {
                    m.Write(new byte[] { 0x00, 0x00, 0x00 });
                }


                return m.ToArray();
            }

        }

        public ushort CreateGuild(byte[] argument, uint masterPlayerID)
        {
            lock (this)
            {
                int pos = 0;

                byte[] guildNameBytes = ReadByteString(argument, pos);
                pos += guildNameBytes.Length;

                byte[] guildCommentBytes = ReadByteString(argument, pos);
                pos += guildCommentBytes.Length;

                byte[] guildEmblem = ReadByteGuildEmblem(argument, pos);

                GuildRepositoryModel model = new GuildRepositoryModel();
                model.GuildName = guildNameBytes;
                model.GuildComment = guildCommentBytes;
                model.GuildEmblem = guildEmblem;
                model.GoldCoin = 0;
                model.SilverCoin = 0;
                model.BronzeCoin = 0;
                model.Gp = 0;
                model.MasterPlayerID = (int)masterPlayerID;
                model.EstablishmentDate = DateTime.Now.ToString("yyyy/MM/dd");

                ushort guildID = DBAccess.getInstance().CreateGuild(model);

                DBAccess.getInstance().EnrollPlayerInGuild(guildID, masterPlayerID, true);

                return guildID;
            }

        }

        public byte[] UpdateGuildEmblemComment(byte[] argument, ushort guildID)
        {
            lock (this)
            {
                int pos = 0;

                byte[] guildNameBytes = ReadByteString(argument, pos);
                pos += guildNameBytes.Length;

                byte[] guildCommentBytes = ReadByteString(argument, pos);
                pos += guildCommentBytes.Length;

                byte[] guildEmblem = ReadByteGuildEmblem(argument, pos);

                GuildRepositoryModel guildRepositoryModel = DBAccess.getInstance().GetGuildInfo(guildID);

                guildRepositoryModel.GuildComment = guildCommentBytes;
                guildRepositoryModel.GuildEmblem = guildEmblem;

                DBAccess.getInstance().UpdateGuildInfo(guildRepositoryModel);

                return new byte[] { 0x00, 0x00 };
            }
        }


        public byte[] DonateCoinsToGuild(byte[] argument)
        {
            lock (this)
            {
                ushort guildIDCointToDonate = swap16(BitConverter.ToUInt16(argument, 0));
                ushort goldCoinDonate = swap16(BitConverter.ToUInt16(argument, 2));
                ushort silverCoinDonate = swap16(BitConverter.ToUInt16(argument, 4));
                ushort bronzeCoinDonate = swap16(BitConverter.ToUInt16(argument, 6));

                Console.WriteLine("Gold Coin Donation " + goldCoinDonate);
                Console.WriteLine("Silver Coin Donation " + silverCoinDonate);
                Console.WriteLine("Bronze Coin Donation " + bronzeCoinDonate);

                GuildRepositoryModel guildRepositoryModel = DBAccess.getInstance().GetGuildInfo(guildIDCointToDonate);
                guildRepositoryModel.GoldCoin += goldCoinDonate;
                guildRepositoryModel.SilverCoin += silverCoinDonate;
                guildRepositoryModel.BronzeCoin += bronzeCoinDonate;

                DBAccess.getInstance().UpdateGuildInfo(guildRepositoryModel);

                MemoryStream m = new MemoryStream();
                m.Write(BitConverter.GetBytes(swap16(goldCoinDonate)));
                m.Write(BitConverter.GetBytes(swap16(silverCoinDonate)));
                m.Write(BitConverter.GetBytes(swap16(bronzeCoinDonate)));

                return m.ToArray();
            }
        }

        public List<byte[]> GetGuildItems(ushort guildId, bool isGeneral)
        {
            lock (this)
            {
                List<GuildItemShopModel> guildItemList = DBAccess.getInstance().GetGuildsItems(guildId);

                if (guildItemList == null)
                {
                    guildItemList = new List<GuildItemShopModel>();
                }

                List<byte[]> listOfItems = new List<byte[]>();
                MemoryStream m = new MemoryStream();


                foreach (var item in guildItemList)
                {
                    if (isGeneral)
                    {
                        if (item.AvailableForGeneral)
                        {
                            m = new MemoryStream();
                            m.Write(BitConverter.GetBytes(swap32((uint)item.ItemID)), 0, 4);
                            m.Write(BitConverter.GetBytes(swap16((ushort)item.Quantity)), 0, 2);
                            m.Write(BitConverter.GetBytes(swap32((uint)item.GeneralPrice)), 0, 4);
                            listOfItems.Add(m.ToArray());
                        }
                    }
                    else
                    {
                        if (item.AvailableForMember)
                        {
                            m = new MemoryStream();
                            m.Write(BitConverter.GetBytes(swap32((uint)item.ItemID)), 0, 4);
                            m.Write(BitConverter.GetBytes(swap16((ushort)item.Quantity)), 0, 2);
                            m.Write(BitConverter.GetBytes(swap32((uint)item.MemberPrice)), 0, 4);
                            listOfItems.Add(m.ToArray());
                        }
                    }


                }

                return listOfItems;
            }

        }

        public List<byte[]> GetAllGuildItemsWithSettings(ushort guildID)
        {
            lock (this)
            {
                List<GuildItemShopModel> guildItemList = DBAccess.getInstance().GetGuildsItems(guildID);

                List<byte[]> itemList = new List<byte[]>();

                if (guildItemList == null)
                {
                    guildItemList = new List<GuildItemShopModel>();
                }

                MemoryStream m = new MemoryStream();

                foreach (var item in guildItemList)
                {
                    m = new MemoryStream();
                    m.Write(BitConverter.GetBytes(swap32((uint)item.ItemID)), 0, 4);
                    m.Write(BitConverter.GetBytes(swap16((ushort)item.Quantity)), 0, 2);
                    m.Write(BitConverter.GetBytes(swap32((uint)item.GeneralPrice)), 0, 4);
                    m.Write(BitConverter.GetBytes(swap32((uint)item.MemberPrice)), 0, 4);

                    if (item.AvailableForGeneral)
                    {
                        m.Write(new byte[] { 0x01 });
                    }
                    else
                    {
                        m.Write(new byte[] { 0x00 });
                    }

                    if (item.AvailableForMember)
                    {
                        m.Write(new byte[] { 0x01 });
                    }
                    else
                    {
                        m.Write(new byte[] { 0x00 });
                    }

                    itemList.Add(m.ToArray());
                }

                return itemList;
            }
        }

        public byte[] GetItemDonationSettings(bool isMaster)
        {
            lock (this)
            {
                if (isMaster)
                {
                    return new byte[] { 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01 };
                }

                return new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }; // don't allow normal member to edit price or publish setting just take thier item XD
            }

        }

        public byte[] AddItemToGuildInventory(ushort guildID, uint itemID, ushort itemQuantity, uint generalPrice,
            uint memberPrice, bool isGeneral, bool isMember, bool isGuildMaster)
        {

            lock (this)
            {
                bool isNewItem = false;
                GuildItemShopModel guildItemShopModel = DBAccess.getInstance().GetSingleGuildItem(guildID, itemID);

                if (guildItemShopModel == null || guildItemShopModel.ItemShopID == -1 || guildItemShopModel.ItemShopID == 0)
                {
                    guildItemShopModel = new GuildItemShopModel();
                    guildItemShopModel.ItemID = (int)itemID;
                    guildItemShopModel.GuildID = guildID;
                    guildItemShopModel.Quantity = 0;
                    guildItemShopModel.GeneralPrice = 0;
                    guildItemShopModel.MemberPrice = 0;
                    guildItemShopModel.AvailableForGeneral = false;
                    guildItemShopModel.AvailableForMember = false;

                    isNewItem = true;
                }

                guildItemShopModel.Quantity += itemQuantity;



                if (isGuildMaster)
                {
                    // edit price and publishing settings
                    guildItemShopModel.GeneralPrice = (int)generalPrice;
                    guildItemShopModel.MemberPrice = (int)memberPrice;
                    guildItemShopModel.AvailableForGeneral = isGeneral;
                    guildItemShopModel.AvailableForMember = isMember;
                }

                DBAccess.getInstance().UpdateSingleGuildItem(guildItemShopModel, isNewItem);

                MemoryStream m = new MemoryStream();
                m.Write(BitConverter.GetBytes(swap16(itemQuantity)));
                return m.ToArray();
            }

        }

        public byte[] GetPriceOfItemToBeDonated(ushort guildID, uint itemID)
        {
            lock (this)
            {
                GuildItemShopModel guildItemShopModel = DBAccess.getInstance().GetSingleGuildItem(guildID, itemID);

                if (guildItemShopModel == null || guildItemShopModel.ItemShopID == -1 || guildItemShopModel.ItemShopID == 0)
                {
                    MemoryStream m = new MemoryStream();

                    m = new MemoryStream();
                    m.Write(BitConverter.GetBytes(swap32(0)));
                    m.Write(BitConverter.GetBytes(swap32(0)));

                    return m.ToArray();
                }
                else
                {
                    MemoryStream m = new MemoryStream();

                    m = new MemoryStream();
                    m.Write(BitConverter.GetBytes(swap32((uint)guildItemShopModel.GeneralPrice)));
                    m.Write(BitConverter.GetBytes(swap32((uint)guildItemShopModel.MemberPrice)));

                    return m.ToArray();
                }

            }


        }

        public byte[] BuyItemFromGuild(byte[] argument)
        {
            lock (this)
            {
                ushort guildIDBuying = swap16(BitConverter.ToUInt16(argument, 0));
                uint itemIDBuying = swap32(BitConverter.ToUInt32(argument, 2));
                ushort quantityOfBuying = swap16(BitConverter.ToUInt16(argument, 6));
                uint priceOfEachPiece = swap32(BitConverter.ToUInt32(argument, 8));
                Console.WriteLine("Guild ID " + guildIDBuying + "\nItem ID = " + itemIDBuying + "\nitem Quantity " +
                                  quantityOfBuying + "\nprice of each piece " + priceOfEachPiece);

                GuildItemShopModel guildItemShopModel =
                    DBAccess.getInstance().GetSingleGuildItem(guildIDBuying, itemIDBuying);

                GuildRepositoryModel guildRepositoryModel = DBAccess.getInstance().GetGuildInfo(guildIDBuying);
                guildItemShopModel.Quantity -= quantityOfBuying;

                uint totalGPToAdd = priceOfEachPiece * quantityOfBuying;

                guildRepositoryModel.Gp += (int)totalGPToAdd;

                DBAccess.getInstance().UpdateSingleGuildItem(guildItemShopModel, false);
                DBAccess.getInstance().UpdateGuildInfo(guildRepositoryModel);

                MemoryStream m = new MemoryStream();
                m.Write(BitConverter.GetBytes(swap16(quantityOfBuying)));
                return m.ToArray();
            }
        }

        public byte[] SetItemVisibilityAndPrice(byte[] argument) // from guild master window 
        {
            lock (this)
            {
                ushort GuildIDMaster = swap16(BitConverter.ToUInt16(argument, 0));
                uint itemIDmaster = swap32(BitConverter.ToUInt32(argument, 2));
                uint GeneralPriceMaster = swap32(BitConverter.ToUInt32(argument, 6));
                uint MemberPriceMaster = swap32(BitConverter.ToUInt32(argument, 10));
                Boolean isGeneralMaster = argument[14] == 0x01;
                Boolean isMemberMaster = argument[15] == 0x01;

                GuildItemShopModel guildItemShopModel =
                    DBAccess.getInstance().GetSingleGuildItem(GuildIDMaster, itemIDmaster);

                guildItemShopModel.GeneralPrice = (int)GeneralPriceMaster;
                guildItemShopModel.MemberPrice = (int)MemberPriceMaster;
                guildItemShopModel.AvailableForGeneral = isGeneralMaster;
                guildItemShopModel.AvailableForMember = isMemberMaster;

                DBAccess.getInstance().UpdateSingleGuildItem(guildItemShopModel, false);

                Console.Write("GenePrice " + GeneralPriceMaster + "\nMemberPrice " + MemberPriceMaster + "\nisGeneral " +
                              isGeneralMaster + "\nisMember " + isMemberMaster);

                return new byte[] { 0x00, 0x00 };
            }
        }


        public byte[] TakeMoneyFromGuild(ushort guildID, uint amountOfMoney)
        {
            lock (this)
            {
                GuildRepositoryModel guildRepositoryModel = DBAccess.getInstance().GetGuildInfo(guildID);
                guildRepositoryModel.Gp -= (int)amountOfMoney;

                DBAccess.getInstance().UpdateGuildInfo(guildRepositoryModel);

                MemoryStream m = new MemoryStream();
                m.Write(BitConverter.GetBytes(swap32(amountOfMoney)));
                return m.ToArray();
            }
        }

        public byte[] TakeItemFromGuild(ushort guildID, uint itemID, ushort quantity)
        {
            lock (this)
            {
                GuildItemShopModel guildItemShopModel = DBAccess.getInstance().GetSingleGuildItem(guildID, itemID);
                guildItemShopModel.Quantity -= quantity;
                DBAccess.getInstance().UpdateSingleGuildItem(guildItemShopModel, false);

                MemoryStream m = new MemoryStream();
                m.Write(BitConverter.GetBytes(swap16(quantity)));
                return m.ToArray();
            }
        }



        public List<byte[]> GetGuildMembersListByClass(ushort guildID, ushort categoryID, uint playerID)
        {
            lock (this)
            {
                List<CharacterRepositoryModel> allMembers = DBAccess.getInstance().GetAllGuildMembers(guildID);

                if (allMembers == null)
                {
                    allMembers = new List<CharacterRepositoryModel>();
                }

                MemoryStream m = new MemoryStream();

                List<byte[]> membersList = new List<byte[]>();

                if (categoryID == 1) // get all members 
                {
                    foreach (var member in allMembers)
                    {
                        if (member.PlayerID == playerID)
                            continue;

                        m = new MemoryStream();

                        m.Write(member.CharacterName);

                        if (member.ClassID == 0)
                        {
                            m.Write(new byte[] { 0x00 });
                        }
                        if (member.ClassID == 1)
                        {
                            m.Write(new byte[] { 0x01 });
                        }
                        if (member.ClassID == 2)
                        {
                            m.Write(new byte[] { 0x02 });
                        }
                        if (member.ClassID == 3)
                        {
                            m.Write(new byte[] { 0x03 });
                        }
                        if (member.ClassID == 4)
                        {
                            m.Write(new byte[] { 0x04 });
                        }
                        if (member.ClassID == 5)
                        {
                            m.Write(new byte[] { 0x05 });
                        }

                        m.Write(BitConverter.GetBytes(swap16((ushort)member.CharacterLevel)));
                        m.Write(member.Greeting);

                        if (member.OnlineStatus)
                        {
                            m.Write(new byte[] { 0x01 });
                        }
                        else
                        {
                            m.Write(new byte[] { 0x00 });
                        }


                        m.Write(BitConverter.GetBytes(swap32((uint)member.ModelNumber)));

                        m.Write(BitConverter.GetBytes(swap32((uint)member.PlayerID))); // Player ID
                        m.Write(BitConverter.GetBytes(member.GuildMaster));

                        membersList.Add(m.ToArray());
                    }

                    return membersList;
                }

                int classID = categoryID - 2;



                foreach (var member in allMembers)
                {
                    if (member.PlayerID == playerID)
                        continue;


                    if (member.ClassID != classID)
                        continue;


                    m = new MemoryStream();

                    m.Write(member.CharacterName);



                    if (member.ClassID == 0)
                    {
                        m.Write(new byte[] { 0x00 });
                    }
                    if (member.ClassID == 1)
                    {
                        m.Write(new byte[] { 0x01 });
                    }
                    if (member.ClassID == 2)
                    {
                        m.Write(new byte[] { 0x02 });
                    }
                    if (member.ClassID == 3)
                    {
                        m.Write(new byte[] { 0x03 });
                    }
                    if (member.ClassID == 4)
                    {
                        m.Write(new byte[] { 0x04 });
                    }
                    if (member.ClassID == 5)
                    {
                        m.Write(new byte[] { 0x05 });
                    }

                    m.Write(BitConverter.GetBytes(swap16((ushort)member.CharacterLevel)));
                    m.Write(member.Greeting);


                    if (member.OnlineStatus)
                    {
                        m.Write(new byte[] { 0x01 });
                    }
                    else
                    {
                        m.Write(new byte[] { 0x00 });
                    }

                    m.Write(BitConverter.GetBytes(swap32((uint)member.ModelNumber)));

                    m.Write(BitConverter.GetBytes(swap32((uint)member.PlayerID))); // Player ID
                    m.Write(BitConverter.GetBytes(member.GuildMaster));

                    membersList.Add(m.ToArray());
                }

                return membersList;
            }
        }

        public byte[] KickPlayerFromGuild(ushort guildID, uint playerToKick)
        {
            lock (this)
            {
                CharacterRepositoryModel characterRepositoryModel = DBAccess.getInstance().GetCharacterInfo(playerToKick);
                characterRepositoryModel.GuildID = 0;
                characterRepositoryModel.GuildMaster = 0;
                DBAccess.getInstance().updatePlayerInfo(characterRepositoryModel);

                return new byte[] { 0x00, 0x00 };
            }
        }

        public byte[] LeaveGuild(ushort guildID, uint characterID)
        {
            lock (this)
            {
                CharacterRepositoryModel characterRepositoryModel = DBAccess.getInstance().GetCharacterInfo(characterID);
                characterRepositoryModel.GuildID = 0;
                characterRepositoryModel.GuildMaster = 0;
                DBAccess.getInstance().updatePlayerInfo(characterRepositoryModel);
                return new byte[] { 0x00, 0x00 };
            }
        }

        public byte[] LeaveGuildAndAssignMaster(ushort guildID, uint playerToAssign)
        {
            lock (this)
            {
                CharacterRepositoryModel characterRepositoryModel = DBAccess.getInstance().GetCharacterInfo(playerToAssign);
                characterRepositoryModel.GuildMaster = 1;
                DBAccess.getInstance().updatePlayerInfo(characterRepositoryModel);
                return new byte[] { 0x00, 0x00 };
            }
        }


        public byte[] DestroyGuild(ushort guildID)
        {
            lock (this)
            {
                DBAccess.getInstance().DeleteGuild(guildID);
                return new byte[] { 0x00, 0x00 };
            }
        }


        public List<byte[]> GetListOfGuilds()
        {
            lock (this)
            {
                List<GuildRepositoryModel> guildRepositoryModels = DBAccess.getInstance().GetAllGuilds();

                if (guildRepositoryModels == null)
                {
                    guildRepositoryModels = new List<GuildRepositoryModel>();
                }

                List<byte[]> listOfGuilds = new List<byte[]>();

                MemoryStream m = new MemoryStream();

                foreach (var guild in guildRepositoryModels)
                {
                    m = new MemoryStream();
                    m.Write(BitConverter.GetBytes(swap16((ushort)guild.GuildID)), 0, 2);
                    m.Write(guild.GuildName);

                    listOfGuilds.Add(m.ToArray());
                }

                return listOfGuilds;
            }
        }


        public byte[] GetGuildInfo(ushort guildID)
        {
            lock (this)
            {
                GuildRepositoryModel guildRepositoryModel = DBAccess.getInstance().GetGuildInfo(guildID);
                CharacterRepositoryModel guildMaster =
                    DBAccess.getInstance().GetCharacterInfo((uint)guildRepositoryModel.MasterPlayerID);
                List<CharacterRepositoryModel> listOfmembers = DBAccess.getInstance().GetAllGuildMembers(guildID);


                MemoryStream m = new MemoryStream();

                m.Write(guildRepositoryModel.GuildName);
                m.Write(_encoding.GetBytes(guildRepositoryModel.EstablishmentDate)); //date
                m.Write(new byte[] { 0x00 });
                m.Write(guildMaster.CharacterName);


                int memTotal = listOfmembers.Count;
                int twinBlade = 0;
                int bladeMaster = 0;
                int heavyBlade = 0;
                int heaveyAxes = 0;
                int longArm = 0;
                int waveMaster = 0;
                int totalLevel = 0;


                foreach (var member in listOfmembers)
                {
                    if (member.ClassID == 0)
                    {
                        twinBlade++;
                    }
                    if (member.ClassID == 1)
                    {
                        bladeMaster++;
                    }
                    if (member.ClassID == 2)
                    {
                        heavyBlade++;
                    }
                    if (member.ClassID == 3)
                    {
                        heaveyAxes++;
                    }
                    if (member.ClassID == 4)
                    {
                        longArm++;
                    }

                    if (member.ClassID == 5)
                    {
                        waveMaster++;
                    }

                    totalLevel += member.CharacterLevel;

                }

                int avgLevel = 0;

                if (memTotal != 0)
                    avgLevel = totalLevel / memTotal;


                m.Write(BitConverter.GetBytes(swap16((ushort)memTotal)), 0, 2);
                m.Write(BitConverter.GetBytes(swap16((ushort)twinBlade)), 0, 2);
                m.Write(BitConverter.GetBytes(swap16((ushort)bladeMaster)), 0, 2);
                m.Write(BitConverter.GetBytes(swap16((ushort)heavyBlade)), 0, 2);
                m.Write(BitConverter.GetBytes(swap16((ushort)heaveyAxes)), 0, 2);
                m.Write(BitConverter.GetBytes(swap16((ushort)longArm)), 0, 2);
                m.Write(BitConverter.GetBytes(swap16((ushort)waveMaster)), 0, 2);
                m.Write(BitConverter.GetBytes(swap16((ushort)avgLevel)), 0, 2);


                m.Write(BitConverter.GetBytes(swap32((uint)guildRepositoryModel.GoldCoin)), 0, 4);
                m.Write(BitConverter.GetBytes(swap32((uint)guildRepositoryModel.SilverCoin)), 0, 4);
                m.Write(BitConverter.GetBytes(swap32((uint)guildRepositoryModel.BronzeCoin)), 0, 4);

                m.Write(BitConverter.GetBytes(swap32((uint)guildRepositoryModel.Gp)), 0, 4);

                m.Write(guildRepositoryModel.GuildComment);

                m.Write(guildRepositoryModel.GuildEmblem);
                m.Write(new byte[] { 0x00 });

                return m.ToArray();
            }
        }

        public byte[] ReadByteGuildEmblem(byte[] data, int pos)
        {
            lock (this)
            {
                MemoryStream m = new MemoryStream();
                while (true)
                {
                    byte b = data[pos++];
                    m.WriteByte(b);
                    if (b == 0x3D)
                        break;
                    if (pos >= data.Length)
                        break;
                }

                return m.ToArray();
            }
        }




        public byte[] ReadByteString(byte[] data, int pos)
        {
            lock (this)
            {
                MemoryStream m = new MemoryStream();
                while (true)
                {
                    byte b = data[pos++];
                    m.WriteByte(b);
                    if (b == 0)
                        break;
                    if (pos >= data.Length)
                        break;
                }

                return m.ToArray();
            }
        }
    }
}