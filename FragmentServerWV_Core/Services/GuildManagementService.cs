using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FragmentServerWV.Services
{
    public class GuildManagementService
    {
        private static GuildManagementService _instance = null;

        private Encoding _encoding;

        public GuildManagementService()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            _encoding = Encoding.GetEncoding("Shift-JIS");
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
            return new byte[] {0x00 //0 = no guild 1= master 2= member
                , 0x00, 0x00}; // Guild ID 
        }

        public ushort CreateGuild(byte[] argument)
        {
            int pos = 0;

            byte[] guildNameBytes = ReadByteString(argument, pos);
            pos += guildNameBytes.Length;

            byte[] guildCommentBytes = ReadByteString(argument, pos);
            pos += guildCommentBytes.Length;

            byte[] guildEmblem = ReadByteGuildEmblem(argument, pos);

            //TODO create Guild in DB and return actual Guild ID

            return 1;
        }

        public byte[] UpdateGuildEmblemComment(byte[] argument)
        {
            int pos = 0;

            byte[] guildNameBytes = ReadByteString(argument, pos);
            pos += guildNameBytes.Length;

            byte[] guildCommentBytes = ReadByteString(argument, pos);
            pos += guildCommentBytes.Length;

            byte[] guildEmblem = ReadByteGuildEmblem(argument, pos);

            return new byte[] {0x00, 0x00};
        }


        public byte[] DonateCoinsToGuild(byte[] argument)
        {
            ushort guildIDCointToDonate = swap16(BitConverter.ToUInt16(argument, 0));
            ushort goldCoinDonate = swap16(BitConverter.ToUInt16(argument, 2));
            ushort silverCoinDonate = swap16(BitConverter.ToUInt16(argument, 4));
            ushort bronzeCoinDonate = swap16(BitConverter.ToUInt16(argument, 6));

            Console.WriteLine("Gold Coin Donation " + goldCoinDonate);
            Console.WriteLine("Silver Coin Donation " + silverCoinDonate);
            Console.WriteLine("Bronze Coin Donation " + bronzeCoinDonate);

            MemoryStream m = new MemoryStream();
            m.Write(BitConverter.GetBytes(swap16(goldCoinDonate)));
            m.Write(BitConverter.GetBytes(swap16(silverCoinDonate)));
            m.Write(BitConverter.GetBytes(swap16(bronzeCoinDonate)));

            return m.ToArray();
        }

        public List<byte[]> GetGuildItems(ushort guildId, bool isGeneral)
        {
            //TODO get list of items of the guild from DB based on guild ID

            List<byte[]> listOfItems = new List<byte[]>();
            MemoryStream m = new MemoryStream();

            uint itemIDGeneral = 720952;
            ushort itemQuanitiyGeneral = 250;
            uint itemPriceGeneral = 50;
            m = new MemoryStream();
            m.Write(BitConverter.GetBytes(swap32(itemIDGeneral)), 0, 4);
            m.Write(BitConverter.GetBytes(swap16(itemQuanitiyGeneral)), 0, 2);
            m.Write(BitConverter.GetBytes(swap32(itemPriceGeneral)), 0, 4);

            listOfItems.Add(m.ToArray());

            return listOfItems;
        }

        public List<byte[]> GetAllGuildItemsWithSettings(ushort guildID)
        {
            List<byte[]> itemList = new List<byte[]>();

            MemoryStream m = new MemoryStream();

            uint itemID2 = 720952;
            ushort itemQuanitiy2 = 1;
            uint itemPrice2 = 3200;
            m = new MemoryStream();
            m.Write(BitConverter.GetBytes(swap32(itemID2)), 0, 4);
            m.Write(BitConverter.GetBytes(swap16(itemQuanitiy2)), 0, 2);
            m.Write(BitConverter.GetBytes(swap32(itemPrice2)), 0, 4);
            m.Write(BitConverter.GetBytes(swap32(itemPrice2)), 0, 4);
            m.Write(new byte[] {0x01, 0x01}); // first one is for general // second is for members 
            //m.Write(BitConverter.GetBytes(swap16(1)),0,2);
            itemList.Add(m.ToArray());

            m = new MemoryStream();
            m.Write(BitConverter.GetBytes(swap32(itemID2)), 0, 4);
            m.Write(BitConverter.GetBytes(swap16(itemQuanitiy2)), 0, 2);
            m.Write(BitConverter.GetBytes(swap32(itemPrice2)), 0, 4);
            m.Write(BitConverter.GetBytes(swap32(itemPrice2)), 0, 4);
            m.Write(new byte[] {0x00, 0x00});
            m.Write(BitConverter.GetBytes(swap32(50)), 0, 4);
            itemList.Add(m.ToArray());

            return itemList;
        }

        public byte[] GetItemDonationSettings(uint itemID, bool isMaster)
        {
            if (isMaster)
            {
                return new byte[] {0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01};
            }
            else
            {
                return new byte[]
                {
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
                }; // don't allow normal member to edit price or publish setting just take thier item XD
            }
        }

        public byte[] AddItemToGuildInventory(ushort guildID, uint itemID, ushort itemQuantity, uint generalPrice,
            uint memberPrice, bool isGeneral, bool isMember, bool isGuildMaster)
        {
            //TODO add item to guild inventory 

            if (isGuildMaster)
            {
                // edit price and publishing settings
            }

            MemoryStream m = new MemoryStream();
            m.Write(BitConverter.GetBytes(swap16(itemQuantity)));
            return m.ToArray();
        }

        public byte[] GetPriceOfItemToBeDonated(uint itemID)
        {
            MemoryStream m = new MemoryStream();

            m = new MemoryStream();
            m.Write(BitConverter.GetBytes(swap32(500)));
            m.Write(BitConverter.GetBytes(swap32(500)));

            return m.ToArray();
        }

        public byte[] BuyItemFromGuild(byte[] argument)
        {
            ushort guildIDBuying = swap16(BitConverter.ToUInt16(argument, 0));
            uint itemIDBuying = swap32(BitConverter.ToUInt32(argument, 2));
            ushort quantityOfBuying = swap16(BitConverter.ToUInt16(argument, 6));
            uint priceOfEachPiece = swap32(BitConverter.ToUInt32(argument, 8));
            Console.WriteLine("Guild ID " + guildIDBuying + "\nItem ID = " + itemIDBuying + "\nitem Quantity " +
                              quantityOfBuying + "\nprice of each piece " + priceOfEachPiece);

            uint totalGPToAdd = priceOfEachPiece * quantityOfBuying;
            //TODO add GP to guild inventory

            MemoryStream m = new MemoryStream();
            m.Write(BitConverter.GetBytes(swap16(quantityOfBuying)));
            return m.ToArray();
        }

        public byte[] SetItemVisibilityAndPrice(byte[] argument)
        {
            ushort GuildIDMaster = swap16(BitConverter.ToUInt16(argument, 0));
            uint itemIDmaster = swap32(BitConverter.ToUInt32(argument, 2));
            uint GeneralPriceMaster = swap32(BitConverter.ToUInt32(argument, 6));
            uint MemberPriceMaster = swap32(BitConverter.ToUInt32(argument, 10));
            Boolean isGeneralMaster = argument[14] == 0x01;
            Boolean isMemberMaster = argument[15] == 0x01;
            ;

            Console.Write("GenePrice " + GeneralPriceMaster + "\nMemberPrice " + MemberPriceMaster + "\nisGeneral " +
                          isGeneralMaster + "\nisMember " + isMemberMaster);

            return new byte[] {0x00, 0x00};
        }


        public byte[] TakeMoneyFromGuild(ushort guildID, uint amountOfMoney)
        {
            MemoryStream m = new MemoryStream();
            m.Write(BitConverter.GetBytes(swap32(amountOfMoney)));
            return m.ToArray();
        }

        public byte[] TakeItemFromGuild(ushort guildID, uint itemID, ushort quantity)
        {
            MemoryStream m = new MemoryStream();
            m.Write(BitConverter.GetBytes(swap32(quantity)));
            return m.ToArray();
        }

        public List<byte[]> GetClassList()
        {
            List<byte[]> classList = new List<byte[]>();
            MemoryStream m = new MemoryStream();

            m.Write(BitConverter.GetBytes(swap16(1)));
            m.Write(_encoding.GetBytes("All"));
            m.Write(new byte[] {0x00});
            classList.Add(m.ToArray());

            m = new MemoryStream();
            m.Write(BitConverter.GetBytes(swap16(2)));
            m.Write(_encoding.GetBytes("Twin Blade"));
            m.Write(new byte[] {0x00});
            classList.Add(m.ToArray());

            m = new MemoryStream();
            m.Write(BitConverter.GetBytes(swap16(3)));
            m.Write(_encoding.GetBytes("Blademaster"));
            m.Write(new byte[] {0x00});
            classList.Add(m.ToArray());

            m = new MemoryStream();
            m.Write(BitConverter.GetBytes(swap16(4)));
            m.Write(_encoding.GetBytes("Heavy Blade"));
            m.Write(new byte[] {0x00});
            classList.Add(m.ToArray());

            m = new MemoryStream();
            m.Write(BitConverter.GetBytes(swap16(5)));
            m.Write(_encoding.GetBytes("Heavy Axe"));
            m.Write(new byte[] {0x00});
            classList.Add(m.ToArray());

            m = new MemoryStream();
            m.Write(BitConverter.GetBytes(swap16(6)));
            m.Write(_encoding.GetBytes("Long Arm"));
            m.Write(new byte[] {0x00});
            classList.Add(m.ToArray());


            m = new MemoryStream();
            m.Write(BitConverter.GetBytes(swap16(7)));
            m.Write(_encoding.GetBytes("Wavemaster"));
            m.Write(new byte[] {0x00});
            classList.Add(m.ToArray());

            return classList;
        }

        public List<byte[]> GetGuildMembersListByClass(ushort guildID, ushort categoryID)
        {
            List<byte[]> membersList = new List<byte[]>();

            MemoryStream m = new MemoryStream();

            string memName = "zackmon";
            byte[] className = {0x02};
            ushort memLevel = 50;
            string memGreeting = "Greeting";
            byte[] memStatus = {0x00};
            byte[] modelNumber = {0x00, 0x00, 0x45, 0x01};
            byte[] isMaster = {0x00};


            m = new MemoryStream();

            m.Write(_encoding.GetBytes(memName));
            m.Write(new byte[] {0x00});
            m.Write(className);
            m.Write(BitConverter.GetBytes(swap16(memLevel)));
            m.Write(_encoding.GetBytes(memGreeting));
            m.Write(new byte[] {0x00});
            m.Write(memStatus);
            m.Write(modelNumber);
            //m.Write(new byte[]{0x00});
            m.Write(new byte[] {0x00, 0x00, 0x00, 0x00}); // Player ID
            m.Write(isMaster);

            membersList.Add(m.ToArray());

            memName = "zack2";
            m = new MemoryStream();

            m.Write(_encoding.GetBytes(memName));
            m.Write(new byte[] {0x00});
            m.Write(className);
            m.Write(BitConverter.GetBytes(swap16(memLevel)));
            m.Write(_encoding.GetBytes(memGreeting));
            m.Write(new byte[] {0x00});
            m.Write(memStatus);
            m.Write(modelNumber);
            //m.Write(new byte[]{0x00});
            m.Write(new byte[] {0x00, 0x00, 0x00, 0x01}); //Player ID
            m.Write(isMaster);
            membersList.Add(m.ToArray());

            switch (categoryID)
            {
                case 1: //ALL Members
                    break;
                case 2: //TwinBlade
                    break;
                case 3: // Blademaster
                    break;
                case 4: // Heavy Blade
                    break;
                case 5: // Heavy Axe
                    break;
                case 6: // Long Arm
                    break;
                case 7: // Wavemaster
                    break;
                default:
                    break;
            }

            return membersList;
        }

        public byte[] KickPlayerFromGuild(ushort guildID, uint playerToKick)
        {
            return new byte[] {0x00, 0x00};
        }

        public byte[] LeaveGuild(ushort guildID, uint characterID)
        {
            return new byte[] {0x00, 0x00};
        }

        public byte[] LeaveGuildAndAssignMaster(ushort guildID, uint playerToAssign)
        {
            return new byte[] {0x00, 0x00};
        }


        public byte[] DestroyGuild(ushort guildID)
        {
            return new byte[] {0x00, 0x00};
        }


        public List<byte[]> GetListOfGuilds()
        {
            //TODO Get List of Guilds from DB

            List<byte[]> listOfGuilds = new List<byte[]>();

            MemoryStream m = new MemoryStream();

            ushort guildID = 8585;
            string guildName = "ZackGuild1234567";

            m.Write(BitConverter.GetBytes(swap16(guildID)), 0, 2);
            m.Write(_encoding.GetBytes(guildName));
            m.Write(new byte[] {0x00});

            listOfGuilds.Add(m.ToArray());

            return listOfGuilds;
        }


        public byte[] GetGuildInfo(ushort guildID)
        {
            //TODO get Guild info from DB using the guildID;

            MemoryStream m = new MemoryStream();

            byte[] guildName = _encoding.GetBytes("ZackGuild1234567");

            m.Write(guildName);


            TimeSpan t = DateTime.UtcNow - new DateTime(1970, 1, 1);
            int secondsSinceEpoch = (int) t.TotalSeconds;
            m.Write(new byte[] {0x00});
            m.Write(_encoding.GetBytes("2020/10/31")); //date

            m.Write(new byte[] {0x00});


            byte[] guildMaster = _encoding.GetBytes("zackTest");
            //Buffer.BlockCopy(,0,guildMaster,0,_encoding.GetBytes("zackTest").Length);
            m.Write(guildMaster);
            m.Write(new byte[] {0x00});


            //byte[] avgText = _encoding.GetBytes("fiejfiejfijefijefiej");
            //   m.Write(avgText);
            // m.Write(new byte[] {0x00});
            //m.Write(new byte[] {0x01});// level
            //uint date  = 50;
            //m.Write(BitConverter.GetBytes(swap32( date)), 0, 4);
            //  byte[] guildMaster = new byte[18];
            //  Buffer.BlockCopy(_encoding.GetBytes("zackTest"),0,guildMaster,0,_encoding.GetBytes("zackTest").Length);
            // m.Write(guildMaster);

            ushort memTotal = 21;
            m.Write(BitConverter.GetBytes(swap16(memTotal)), 0, 2);

            ushort twinBlade = 6;
            m.Write(BitConverter.GetBytes(swap16(twinBlade)), 0, 2);
            ushort bladeMaster = 5;
            m.Write(BitConverter.GetBytes(swap16(bladeMaster)), 0, 2);
            ushort heavyBlade = 4;
            m.Write(BitConverter.GetBytes(swap16(heavyBlade)), 0, 2);
            ushort heaveyAxes = 3;
            m.Write(BitConverter.GetBytes(swap16(heaveyAxes)), 0, 2);
            ushort longArm = 2;
            m.Write(BitConverter.GetBytes(swap16(longArm)), 0, 2);
            ushort waveMaster = 1;
            m.Write(BitConverter.GetBytes(swap16(waveMaster)), 0, 2);

            ushort avgLevel = 55;
            m.Write(BitConverter.GetBytes(swap16(avgLevel)), 0, 2);


            uint goldCoins = 50;
            m.Write(BitConverter.GetBytes(swap32(goldCoins)), 0, 4);
            uint silverCoins = 40;
            m.Write(BitConverter.GetBytes(swap32(silverCoins)), 0, 4);
            uint bronzeCoins = 30;
            m.Write(BitConverter.GetBytes(swap32(bronzeCoins)), 0, 4);

            uint GP = 5000;
            m.Write(BitConverter.GetBytes(swap32(GP)), 0, 4);

            byte[] guildComment = _encoding.GetBytes("This is the Guild Comment");
            m.Write(guildComment);
            m.Write(new byte[] {0x00});
            //"This is the Guild Comment";

            // m.Write(guildComment);

            byte[] guildEmblem =
            {
                0x53, 0x45, 0x39, 0x46, 0x54, 0x55, 0x49, 0x77, 0x4D, 0x41, 0x41, 0x65, 0x41, 0x41, 0x41, 0x41, 0x67,
                0x49, 0x43, 0x41, 0x41, 0x49, 0x43, 0x41, 0x67, 0x41, 0x43, 0x41, 0x67, 0x49, 0x41, 0x41, 0x67, 0x49,
                0x43, 0x41, 0x41, 0x45, 0x38, 0x4B, 0x41, 0x41, 0x41, 0x41, 0x41, 0x49, 0x41, 0x41, 0x67, 0x49, 0x41,
                0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x47, 0x41, 0x67, 0x49, 0x43, 0x41, 0x41, 0x41, 0x41, 0x41,
                0x41, 0x41, 0x41, 0x43, 0x67, 0x49, 0x43, 0x41, 0x67, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41,
                0x34, 0x43, 0x41, 0x67, 0x49, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x53, 0x41, 0x67, 0x49,
                0x43, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x46, 0x67, 0x49, 0x43, 0x41, 0x67, 0x41, 0x41,
                0x41, 0x41, 0x41, 0x41, 0x41, 0x42, 0x6F, 0x43, 0x41, 0x67, 0x49, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41,
                0x41, 0x41, 0x65, 0x41, 0x67, 0x49, 0x43, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x49, 0x67,
                0x49, 0x43, 0x41, 0x67, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x43, 0x59, 0x43, 0x41, 0x67, 0x49,
                0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x71, 0x41, 0x67, 0x49, 0x43, 0x41, 0x41, 0x41, 0x41,
                0x41, 0x41, 0x41, 0x41, 0x4C, 0x67, 0x49, 0x43, 0x41, 0x67, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41,
                0x44, 0x49, 0x43, 0x41, 0x67, 0x49, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x32, 0x41, 0x67,
                0x49, 0x43, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x4F, 0x67, 0x49, 0x43, 0x41, 0x67, 0x41,
                0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x44, 0x34, 0x43, 0x41, 0x67, 0x49, 0x41, 0x41, 0x41, 0x41, 0x41,
                0x41, 0x41, 0x42, 0x43, 0x41, 0x67, 0x49, 0x43, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x52,
                0x67, 0x49, 0x43, 0x41, 0x67, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x45, 0x6F, 0x43, 0x41, 0x67,
                0x49, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x42, 0x4F, 0x41, 0x67, 0x49, 0x43, 0x41, 0x41, 0x41,
                0x41, 0x41, 0x41, 0x41, 0x41, 0x55, 0x67, 0x49, 0x43, 0x41, 0x67, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41,
                0x41, 0x46, 0x59, 0x43, 0x41, 0x67, 0x49, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x42, 0x61, 0x41,
                0x67, 0x49, 0x43, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x58, 0x67, 0x49, 0x43, 0x41, 0x67,
                0x41, 0x38, 0x51, 0x41, 0x41, 0x41, 0x41, 0x47, 0x49, 0x43, 0x41, 0x67, 0x49, 0x42, 0x61, 0x43, 0x67,
                0x41, 0x41, 0x41, 0x42, 0x6D, 0x41, 0x67, 0x49, 0x42, 0x54, 0x59, 0x78, 0x63, 0x41, 0x41, 0x41, 0x41,
                0x61, 0x67, 0x49, 0x43, 0x41, 0x4D, 0x41, 0x49, 0x42, 0x41, 0x41, 0x41, 0x41, 0x47, 0x34, 0x43, 0x41,
                0x67, 0x49, 0x41, 0x42, 0x42, 0x77, 0x41, 0x41, 0x41, 0x42, 0x79, 0x41, 0x67, 0x49, 0x43, 0x41, 0x41,
                0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x64, 0x67, 0x49, 0x43, 0x41, 0x67, 0x41, 0x3D, 0x3D
            };
            m.Write(guildEmblem);
            m.Write(new byte[] {0x00});

            return m.ToArray();
        }

        public byte[] ReadByteGuildEmblem(byte[] data, int pos)
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


        //Copy from the GameClient Code
        public ushort swap16(ushort data)
        {
            ushort result = 0;
            result = (ushort) ((data >> 8) + ((data & 0xFF) << 8));
            return result;
        }


        public uint swap32(uint data)
        {
            uint result = 0;
            result |= (uint) ((data & 0xFF) << 24);
            result |= (uint) (((data >> 8) & 0xFF) << 16);
            result |= (uint) (((data >> 16) & 0xFF) << 8);
            result |= (uint) ((data >> 24) & 0xFF);
            return result;
        }

        public byte[] ReadByteString(byte[] data, int pos)
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