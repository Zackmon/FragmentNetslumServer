using System;
using System.Collections.Generic;
using System.IO;
using FragmentNetslumServer.Enumerations;
using FragmentNetslumServer.Models;
using FragmentNetslumServer.Services.Interfaces;

namespace FragmentNetslumServer.Services
{
    public sealed class RankingManagementService : BaseManagementService, IRankingManagementService
    {

        public string ServiceName => "Ranking Management Service";

        public ServiceStatusEnum ServiceStatus => ServiceStatusEnum.Active;



        public RankingManagementService() : base()
        {
        }



        public List<byte[]> GetRankingCategory()
        {
            List<byte[]> rankingList = new List<byte[]>();

            MemoryStream m = new MemoryStream();

            m.Write(BitConverter.GetBytes(swap16(8)));
            m.Write(_encoding.GetBytes("Level"));
            m.Write(new byte[] { 0x00 });
            rankingList.Add(m.ToArray());

            m = new MemoryStream();
            m.Write(BitConverter.GetBytes(swap16(9)));
            m.Write(_encoding.GetBytes("HP"));
            m.Write(new byte[] { 0x00 });
            rankingList.Add(m.ToArray());

            m = new MemoryStream();
            m.Write(BitConverter.GetBytes(swap16(10)));
            m.Write(_encoding.GetBytes("SP"));
            m.Write(new byte[] { 0x00 });
            rankingList.Add(m.ToArray());

            m = new MemoryStream();
            m.Write(BitConverter.GetBytes(swap16(11)));
            m.Write(_encoding.GetBytes("GP"));
            m.Write(new byte[] { 0x00 });
            rankingList.Add(m.ToArray());

            m = new MemoryStream();
            m.Write(BitConverter.GetBytes(swap16(12)));
            m.Write(_encoding.GetBytes("Online Gott Statue"));
            m.Write(new byte[] { 0x00 });
            rankingList.Add(m.ToArray());

            m = new MemoryStream();
            m.Write(BitConverter.GetBytes(swap16(13)));
            m.Write(_encoding.GetBytes("Offline Gott Statue"));
            m.Write(new byte[] { 0x00 });
            rankingList.Add(m.ToArray());


            m = new MemoryStream();
            m.Write(BitConverter.GetBytes(swap16(14)));
            m.Write(_encoding.GetBytes("Gold Coin"));
            m.Write(new byte[] { 0x00 });
            rankingList.Add(m.ToArray());

            m = new MemoryStream();
            m.Write(BitConverter.GetBytes(swap16(15)));
            m.Write(_encoding.GetBytes("Silver Coin"));
            m.Write(new byte[] { 0x00 });
            rankingList.Add(m.ToArray());

            m = new MemoryStream();
            m.Write(BitConverter.GetBytes(swap16(16)));
            m.Write(_encoding.GetBytes("Bronze Coin"));
            m.Write(new byte[] { 0x00 });
            rankingList.Add(m.ToArray());


            return rankingList;
        }


        public List<byte[]> GetPlayerRanking(ushort categoryID, ushort classID)
        {
            lock (this)
            {
                List<CharacterRepositoryModel> characterList = DBAccess.getInstance().GetRanking(categoryID, classID);

                List<byte[]> rankingList = new List<byte[]>();

                MemoryStream m = new MemoryStream();

                foreach (var player in characterList)
                {
                    m = new MemoryStream();
                    m.Write(player.CharacterName);
                    m.Write(BitConverter.GetBytes(swap32((uint)player.PlayerID)));
                    rankingList.Add(m.ToArray());
                }

                return rankingList;
            }
        }


        public byte[] GetRankingPlayerInfo(uint playerID)
        {
            lock (this)
            {
                CharacterRepositoryModel characterRepositoryModel = DBAccess.getInstance().GetCharacterInfo(playerID);
                GuildRepositoryModel guildRepositoryModel = null;
                bool inGuild = false;
                if (characterRepositoryModel.GuildID != 0)
                {
                    guildRepositoryModel = DBAccess.getInstance().GetGuildInfo((ushort)characterRepositoryModel.GuildID);
                    inGuild = true;
                }

                MemoryStream m = new MemoryStream();

                m.Write(characterRepositoryModel.CharacterName);

                if (characterRepositoryModel.ClassID == 0)
                {
                    m.Write(new byte[] { 0x00 });
                }
                if (characterRepositoryModel.ClassID == 1)
                {
                    m.Write(new byte[] { 0x01 });
                }
                if (characterRepositoryModel.ClassID == 2)
                {
                    m.Write(new byte[] { 0x02 });
                }
                if (characterRepositoryModel.ClassID == 3)
                {
                    m.Write(new byte[] { 0x03 });
                }
                if (characterRepositoryModel.ClassID == 4)
                {
                    m.Write(new byte[] { 0x04 });
                }
                if (characterRepositoryModel.ClassID == 5)
                {
                    m.Write(new byte[] { 0x05 });
                }

                m.Write(BitConverter.GetBytes(swap16((ushort)characterRepositoryModel.CharacterLevel)));
                m.Write(characterRepositoryModel.Greeting);

                if (inGuild && guildRepositoryModel != null)
                {
                    m.Write(guildRepositoryModel.GuildName);
                }
                else
                {
                    m.Write(new byte[] { 0x00 });
                }

                m.Write(BitConverter.GetBytes(swap32((uint)characterRepositoryModel.ModelNumber)));

                if (characterRepositoryModel.OnlineStatus)
                {
                    m.Write(new byte[] { 0x01 });
                }
                else
                {
                    m.Write(new byte[] { 0x00 });
                }

                if (characterRepositoryModel.GuildMaster == 1)
                {
                    m.Write(new byte[] { 0x01 });
                }
                else if (characterRepositoryModel.GuildMaster == 2)
                {
                    m.Write(new byte[] { 0x02 });
                }
                else
                {
                    m.Write(new byte[] { 0x03 });
                }

                return m.ToArray();
            }
        }

    }

}