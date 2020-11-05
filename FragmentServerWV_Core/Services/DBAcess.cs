using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FragmentServerWV.Models;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Criterion;

namespace FragmentServerWV.Services
{
    public class DBAcess
    {
        private static DBAcess _instance = null;
        private ISessionFactory _sessionFactory;
        private Encoding _encoding;
        private string _messageOfTheDay;

        public static DBAcess getInstance()
        {
            if (_instance == null)
            {
                _instance = new DBAcess();
            }

            return _instance;
        }

        public DBAcess()
        {

            var config = new Configuration().Configure();
            config.AddAssembly("FragmentServerWV_Core");
            _sessionFactory = config.BuildSessionFactory();

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            _encoding = Encoding.GetEncoding("Shift-JIS");

            _messageOfTheDay = LoadMessageOfDay();
        }

        public List<BbsCategoryModel> GetListOfBbsCategory()
        {
            List<BbsCategoryModel> categoryList = new List<BbsCategoryModel>();
            using (ISession session = _sessionFactory.OpenSession())
            {
                using ITransaction transaction = session.BeginTransaction();
                
                ICriteria criteria = session.CreateCriteria(typeof(BbsCategoryModel));
                IList<BbsCategoryModel> bbsCategoryModels = session.Query<BbsCategoryModel>().ToList();
                categoryList.AddRange(bbsCategoryModels);
                
                transaction.Commit();
                session.Close();
            }

            return categoryList;
        }


        public List<BbsThreadModel> getThreadsByCategoryID(int categoryID)
        {
            List<BbsThreadModel> threadLists = new List<BbsThreadModel>();

            using (ISession session = _sessionFactory.OpenSession())
            {
                using ITransaction transaction = session.BeginTransaction();
                threadLists.AddRange(
                    session.Query<BbsThreadModel>().Where
                            (x => x.categoryID == categoryID)
                        .ToList());
                transaction.Commit();

                session.Close();
            }

            return threadLists;
        }


        public List<BbsPostMetaModel> GetPostsMetaByThreadId(int threadID)
        {
            List<BbsPostMetaModel> postMetaList = new List<BbsPostMetaModel>();
            using ISession session = _sessionFactory.OpenSession();
            using ITransaction transaction = session.BeginTransaction();

            postMetaList.AddRange(
                session.Query<BbsPostMetaModel>().Where
                        (x => x.threadID == threadID)
                    .ToList());

            transaction.Commit();
            session.Close();

            return postMetaList;
        }


        public BbsPostBody GetPostBodyByPostId(int postID)
        {
            using ISession session = _sessionFactory.OpenSession();
            using ITransaction transaction = session.BeginTransaction();
            
            BbsPostBody postBody = session.Query<BbsPostBody>().SingleOrDefault(x => x.postID == postID);
            transaction.Commit();
            session.Close();

            return postBody;
        }

        public void CreateNewPost(byte[] argument, uint u)
        {
            byte[] threadIdBytes = new byte[4];
            byte[] usernameBytes = new byte[16];
            byte[] postTitleBytes = new byte[36];
            byte[] postBodyBytes = new byte[600];

            Buffer.BlockCopy(argument, 0, threadIdBytes, 0, 4);
            Buffer.BlockCopy(argument, 4, usernameBytes, 0, 16);
            Buffer.BlockCopy(argument, 84, postTitleBytes, 0, 32);
            Buffer.BlockCopy(argument, 134, postBodyBytes, 0, 600);

            Console.WriteLine("Thread ID  = " + BitConverter.ToString(threadIdBytes));
            Console.WriteLine("username " + BitConverter.ToString(usernameBytes));
            Console.WriteLine("post Title " + BitConverter.ToString(postTitleBytes));
            Console.WriteLine("post Body " + BitConverter.ToString(postBodyBytes));


            // int threadIDX = BitConverter.ToInt32(threadIdBytes);
            String username = _encoding.GetString(usernameBytes);
            String postTitle = _encoding.GetString(postTitleBytes);
            String postBody = _encoding.GetString(postBodyBytes);

            int threadID = 0;
            if (u == 0)
            {
                // Create a new Thread before posting 

                BbsThreadModel thread = new BbsThreadModel();
                thread.threadTitle = postTitleBytes;
                thread.categoryID = 1;

                using (ISession session = _sessionFactory.OpenSession())
                {
                    //threadID = session.Query<BbsThreadModel>().Max(x => (int?) x.threadID).Value + 1;
                    //thread.threadID = threadID;
                    using (ITransaction transaction = session.BeginTransaction())
                    {
                        session.Save(thread);
                        transaction.Commit();
                        threadID = thread.threadID;
                    }

                    session.Close();
                }
            }
            else
            {
                threadID = Convert.ToInt32(u);
            }


            // post to an existing thread 

            BbsPostMetaModel meta = new BbsPostMetaModel();


            meta.unk2 = 0;
            meta.date = DateTime.UtcNow;
            meta.username = usernameBytes;
            meta.title = postTitleBytes;
            meta.subtitle = new byte[16];
            if (postTitle.Length > 16)
                Buffer.BlockCopy(postTitleBytes, 0, meta.subtitle, 0, 16);
            else
                meta.subtitle = postTitleBytes;
            meta.unk3 = "unk3";
            meta.threadID = threadID;

            using (ISession session = _sessionFactory.OpenSession())
            {
                try
                {
                    meta.unk0 = session.Query<BbsPostMetaModel>().Max(x => (int?) x.unk0).Value + 1;
                }
                catch (Exception)
                {
                    Console.WriteLine("BBS is empty , creating needed data");
                    meta.unk0 = 1;
                }
                
                //int postBodyID = session.Query<BbsPostBody>().Max(x => (int?) x.postBodyID).Value + 1;
                //meta.postID = postID;
                using (ITransaction transaction = session.BeginTransaction())
                {
                    session.Save(meta);
                    //transaction.Commit();

                    BbsPostBody body = new BbsPostBody();

                    body.postBody = postBodyBytes;
                    body.postID = meta.postID;

                    session.Save(body);
                    transaction.Commit();
                }

                session.Close();
            }

            // Console.WriteLine("Thread ID  = " + threadIDX);
            Console.WriteLine("username " + username);
            Console.WriteLine("post Title " + postTitle);
            Console.WriteLine("post Body " + postBody);
        }

        public uint PlayerLogin(GameClient client)
        {
            
            ///////////////Player Logging ///////////////////////////////////////////////////////////////////////////////
            RankingDataModel model = new RankingDataModel();


            DateTime dateTime = DateTime.UtcNow;

            model.antiCheatEngineResult = "LEGIT";
            model.loginTime = dateTime.ToString("ddd MMM dd hh:mm:ss yyyy");
            model.diskID = "DUMMY DISK ID VALUE !";
            model.saveID = _encoding.GetString(client.save_id, 0, client.save_id.Length - 1);
            model.characterSaveID = _encoding.GetString(client.char_id, 0, client.char_id.Length - 1);
            model.characterName = _encoding.GetString(client.char_name, 0, client.char_name.Length - 1);
            //Buffer.BlockCopy(client.char_name,0,model.characterName,0,client.char_name.Length-1);

            PlayerClass playerClass = (PlayerClass) client.char_class;
            model.characterClassName = playerClass.ToString();
            model.characterLevel = client.char_level;

            model.characterHP = client.char_HP;
            model.characterSP = client.char_SP;
            model.characterGP = (int) client.char_GP;
            model.godStatusCounterOnline = client.online_god_counter;
            model.averageFieldLevel = client.offline_godcounter;
            model.accountID = client.AccountId;
            ///////////////Player Logging ///////////////////////////////////////////////////////////////////////////////
            
            using ISession session = _sessionFactory.OpenSession();

            using ITransaction transaction = session.BeginTransaction();

            CharacterRepositoryModel characterRepositoryModel;
            
            characterRepositoryModel = session.Query<CharacterRepositoryModel>().SingleOrDefault(
                x => x.accountID == client.AccountId && x.charachterSaveID.Equals(model.characterSaveID));
            if (characterRepositoryModel == null || characterRepositoryModel.PlayerID == -1 ||
                characterRepositoryModel.PlayerID == 0)
            {
                characterRepositoryModel = new CharacterRepositoryModel();
                
                characterRepositoryModel.GuildID = 0;
                characterRepositoryModel.GuildMaster = 0;
                
                characterRepositoryModel.accountID = client.AccountId;
                characterRepositoryModel.charachterSaveID = model.characterSaveID;
                
            }
            characterRepositoryModel.CharachterName = client.char_name;
            characterRepositoryModel.Greeting = client.greeting;
            characterRepositoryModel.ClassID = client.char_class;
            characterRepositoryModel.CharachterLevel = client.char_level;
            characterRepositoryModel.OnlineStatus = true;
            characterRepositoryModel.ModelNumber = (int) client.char_model;
            characterRepositoryModel.charHP = client.char_HP;
            characterRepositoryModel.charSP = client.char_SP;
            characterRepositoryModel.charGP = (int) client.char_GP;
            characterRepositoryModel.charOnlineGoat = client.online_god_counter;
            characterRepositoryModel.charOfflineGoat = client.offline_godcounter;
            characterRepositoryModel.charGoldCoin = client.goldCoinCount;
            characterRepositoryModel.charSilverCoin = client.silverCoinCount;
            characterRepositoryModel.charBronzeCoin = client.bronzeCoinCount;


            session.Save(model);
            session.SaveOrUpdate(characterRepositoryModel);
            transaction.Commit();

            session.Close();

            return (uint) characterRepositoryModel.PlayerID;
        }

        public void setPlayerAsOffline(uint playerID)
        {
            using ISession session = _sessionFactory.OpenSession();

            using ITransaction transaction = session.BeginTransaction();
            
            CharacterRepositoryModel characterRepositoryModel = session.Query<CharacterRepositoryModel>().SingleOrDefault(
                x => x.PlayerID == playerID);

            characterRepositoryModel.OnlineStatus = false;
            
            session.SaveOrUpdate(characterRepositoryModel);
            transaction.Commit();
            session.Close();
        }

        public void updatePlayerInfo(CharacterRepositoryModel characterRepositoryModel)
        {
            using ISession session = _sessionFactory.OpenSession();

            using ITransaction transaction = session.BeginTransaction();
            
            session.SaveOrUpdate(characterRepositoryModel);
            transaction.Commit();
            session.Close();
        }

        public CharacterRepositoryModel GetCharacterInfo(uint playerID)
        {
            using ISession session = _sessionFactory.OpenSession();

            using ITransaction transaction = session.BeginTransaction();
            
            CharacterRepositoryModel characterRepositoryModel = session.Query<CharacterRepositoryModel>().SingleOrDefault(
                x => x.PlayerID == playerID);

            
            
            transaction.Commit();
            session.Close();

            return characterRepositoryModel;
        }

        public void EnrollPlayerInGuild(ushort guildID, uint playerID, bool isMaster)
        {
            using ISession session = _sessionFactory.OpenSession();

            using ITransaction transaction = session.BeginTransaction();
            
            CharacterRepositoryModel characterRepositoryModel = session.Query<CharacterRepositoryModel>().SingleOrDefault(
                x => x.PlayerID == playerID);

            characterRepositoryModel.GuildID = guildID;
            if (isMaster)
            {
                characterRepositoryModel.GuildMaster = 1;
            }
            else
            {
                characterRepositoryModel.GuildMaster = 2;
            }
            
            session.SaveOrUpdate(characterRepositoryModel);

            transaction.Commit();
            session.Close();

        }

        public ushort CreateGuild(GuildRepositoryModel guildRepositoryModel)
        {

            using ISession session = _sessionFactory.OpenSession();

            using ITransaction transaction = session.BeginTransaction();

            session.Save(guildRepositoryModel);
            transaction.Commit();
            session.Close();

            return (ushort) guildRepositoryModel.GuildID;
        }

        public GuildRepositoryModel GetGuildInfo(ushort guildID)
        {
            using ISession session = _sessionFactory.OpenSession();

            using ITransaction transaction = session.BeginTransaction();
            
            GuildRepositoryModel guildRepositoryModel = session.Query<GuildRepositoryModel>().SingleOrDefault(
                x => x.GuildID == guildID);


            transaction.Commit();
            session.Close();
            
            return guildRepositoryModel;

        }

        public void UpdateGuildInfo(GuildRepositoryModel guildRepositoryModel)
        {
            using ISession session = _sessionFactory.OpenSession();

            using ITransaction transaction = session.BeginTransaction();

            session.SaveOrUpdate(guildRepositoryModel);
            transaction.Commit();
            session.Close();
        }

        public List<GuildItemShopModel> GetGuildsItems(ushort guildID)
        {
            
            using ISession session = _sessionFactory.OpenSession();

            using ITransaction transaction = session.BeginTransaction();
            
            List<GuildItemShopModel> guildItemShopList = session.Query<GuildItemShopModel>().Where(
                x => x.GuildID == guildID).ToList();


            transaction.Commit();
            session.Close();

            return guildItemShopList;
        }


        public GuildItemShopModel GetSingleGuildItem(ushort guildID, uint itemID)
        {
            using ISession session = _sessionFactory.OpenSession();

            using ITransaction transaction = session.BeginTransaction();

            GuildItemShopModel guildItemShopList = session.Query<GuildItemShopModel>().SingleOrDefault(
                x => x.GuildID == guildID && x.ItemID == itemID);


            transaction.Commit();
            session.Close();

            return guildItemShopList;   
        }

        public void UpdateSingleGuildItem(GuildItemShopModel guildItemShopModel, bool isNewItem)
        {
            using ISession session = _sessionFactory.OpenSession();

            using ITransaction transaction = session.BeginTransaction();

            if (isNewItem)
            {
                session.Save(guildItemShopModel);
            }
            else
            {
                session.SaveOrUpdate(guildItemShopModel);
            }

            transaction.Commit();
            session.Close();
        }


        public List<CharacterRepositoryModel> GetAllGuildMembers(ushort guildID)
        {
            using ISession session = _sessionFactory.OpenSession();

            using ITransaction transaction = session.BeginTransaction();
            
            List<CharacterRepositoryModel> guildItemShopList = session.Query<CharacterRepositoryModel>().Where(
                x => x.GuildID == guildID).ToList();


            transaction.Commit();
            session.Close();

            return guildItemShopList;
        }

        public CharacterRepositoryModel GetCharacterRepositoryModel(uint playerID)
        {
            using ISession session = _sessionFactory.OpenSession();

            using ITransaction transaction = session.BeginTransaction();

            CharacterRepositoryModel characterRepositoryModel = session.Query<CharacterRepositoryModel>().SingleOrDefault(
                x => x.PlayerID == playerID);


            transaction.Commit();
            session.Close();

            return characterRepositoryModel;
        }

        public List<GuildRepositoryModel> GetAllGuilds()
        {
            using ISession session = _sessionFactory.OpenSession();

            using ITransaction transaction = session.BeginTransaction();

            List<GuildRepositoryModel> guildRepositoryModels = session.Query<GuildRepositoryModel>().ToList();

            transaction.Commit();
            session.Close();
            return guildRepositoryModels;
        }

        public void DeleteGuild(ushort guildID)
        {
            using ISession session = _sessionFactory.OpenSession();

            using ITransaction transaction = session.BeginTransaction();

            var updateCharQuery = session.CreateSQLQuery(
                "update CharacterRepository set guildID = 0, guildMaster = 0 where guildID = :guildID");
            var deleteItemsFromShopQuery =session.CreateSQLQuery( "delete from GuildItemShop where  guildID = :guildID");
            var deleteGuildFromTable = session.CreateSQLQuery("delete from GuildRepository where guildID = :guildID");
            
            updateCharQuery.SetInt32("guildID", guildID);
            deleteItemsFromShopQuery.SetInt32("guildID", guildID);
            deleteGuildFromTable.SetInt32("guildID", guildID);
            
            
            updateCharQuery.ExecuteUpdate();
            deleteItemsFromShopQuery.ExecuteUpdate();
            deleteGuildFromTable.ExecuteUpdate();
            
            transaction.Commit();

            session.Close();

        }




        public int GetPlayerAccountId(string saveID)
        {
            PlayerAccountIDModel playerAccountIdModel;

            using (ISession session = _sessionFactory.OpenSession())
            {
                using ITransaction transaction = session.BeginTransaction();
                playerAccountIdModel = session.Query<PlayerAccountIDModel>()
                    .SingleOrDefault(x => x.saveID == saveID);

                if (playerAccountIdModel == null || playerAccountIdModel.ID == -1 || playerAccountIdModel.ID == 0)
                {
                    Console.WriteLine("the Save ID " + saveID +
                                      " does not have an associated account ID , creating a new Account ID");

                   
                        playerAccountIdModel = new PlayerAccountIDModel();
                        playerAccountIdModel.saveID = saveID;

                        session.Save(playerAccountIdModel);

                        Console.WriteLine("the new Account ID for the save " + saveID + " is " + playerAccountIdModel.ID);
                }
                
                transaction.Commit();
                session.Close();
            }

            return playerAccountIdModel.ID;
        }

        public List<CharacterRepositoryModel> GetRanking(ushort categoryID, ushort classID)
        {
            using ISession session = _sessionFactory.OpenSession();

            using ITransaction transaction = session.BeginTransaction();

            
            
            var criteria = session.CreateCriteria<CharacterRepositoryModel>();

            if (classID != 1)
            {
                classID -= 2;
                //query = session.CreateSQLQuery("select * from CharacterRepository order by :category DESC LIMIT 10 ");
                criteria.Add(Expression.Eq("ClassID",(int) classID));
            }
            /*else
            {
                query = session.CreateSQLQuery("select * from CharacterRepository where classID = :classID order by :category DESC LIMIT 10 ");
                query.SetInt32("classID",classID - 2 );
            }*/

            switch (categoryID)
            {
                case 8: //Level
                    //query.SetString("category", "charachterLevel");
                    criteria.AddOrder(Order.Desc("CharachterLevel"));
                    break;
                case 9: //HP
                    //query.SetString("category", "charHP");
                    criteria.AddOrder(Order.Desc("charHP"));
                    break;
                case 10://SP
                    //query.SetString("category", "charSP");
                    criteria.AddOrder(Order.Desc("charSP"));
                    break;
                case 11://GP
                    //query.SetString("category", "charGP");
                    criteria.AddOrder(Order.Desc("charGP"));
                    break;
                case 12: // Online Gott Status
                    //query.SetString("category", "charOnlineGoat");
                    criteria.AddOrder(Order.Desc("charOnlineGoat"));
                    break;
                case 13: //Offline Gott Statue
                    //query.SetString("category", "charOfflineGoat");
                    criteria.AddOrder(Order.Desc("charOfflineGoat"));
                    break;
                case 14: //Gold Coin
                    //query.SetString("category", "charGoldCoin");
                    criteria.AddOrder(Order.Desc("charGoldCoin"));
                    break;
                case 15: // Silver Coin
                    //query.SetString("category", "charSilverCoin");
                    criteria.AddOrder(Order.Desc("charSilverCoin"));
                    break;
                case 16: // Bronze Coin 
                    //query.SetString("category", "charBronzeCoin");
                    criteria.AddOrder(Order.Desc("charBronzeCoin"));
                    break;
            }

            

            //query.AddEntity(typeof(CharacterRepositoryModel));

            var queryList = criteria.SetMaxResults(10).List<CharacterRepositoryModel>();
            
            List<CharacterRepositoryModel> modelList = new List<CharacterRepositoryModel>();
            modelList.AddRange(queryList);

            transaction.Commit();
            session.Close();
            

            return modelList;
        }

        public Boolean checkForNewMailByAccountID(int accountID)
        {
            Boolean newMail = false;
            using (ISession session = _sessionFactory.OpenSession())
            {
                using ITransaction transaction = session.BeginTransaction();
                
                newMail = session.Query<MailMetaModel>()
                    .Any(x => x.Receiver_Account_ID == accountID
                              && x.Mail_Delivered == false);

                transaction.Commit();
                session.Close();
            }

            return newMail;
        }

        public List<MailMetaModel> GetAccountMail(int accountID)
        {
            List<MailMetaModel> metaList = new List<MailMetaModel>();

            using ISession session = _sessionFactory.OpenSession();
            using ITransaction transaction = session.BeginTransaction();
            
            metaList = session.Query<MailMetaModel>()
                .Where(x => x.Receiver_Account_ID == accountID && x.Mail_Delivered == false)
                .ToList();

            //set the records as delivered 
            if (metaList.Count > 0)
            {
                
                foreach (MailMetaModel meta in metaList)
                {
                    meta.Mail_Delivered = true;
                    session.SaveOrUpdate(meta);
                }
                
               
            }

            transaction.Commit();
            session.Close();

            return metaList;
        }

        public MailBodyModel GetMailBodyByMailId(int mail_ID)
        {
            MailBodyModel bodyModel = new MailBodyModel();

            using (ISession session = _sessionFactory.OpenSession())
            {
                using ITransaction transaction = session.BeginTransaction();
                
                bodyModel = session.Query<MailBodyModel>()
                    .SingleOrDefault(x => x.Mail_ID == mail_ID);

                transaction.Commit();
                session.Close();
            }


            return bodyModel;
        }

        public void CreateNewMail(MailMetaModel metaModel, MailBodyModel bodyModel)
        {
            using ISession session = _sessionFactory.OpenSession();
            using ITransaction transaction = session.BeginTransaction();

            session.Save(metaModel);
            
            bodyModel.Mail_ID = metaModel.Mail_ID;
            
            session.Save(bodyModel);
            
            transaction.Commit();

            session.Close();
        }

        public string LoadMessageOfDay()
        {
            MessageOfTheDayModel messageModel = new MessageOfTheDayModel();

            using (ISession session = _sessionFactory.OpenSession())
            {
                using ITransaction transaction = session.BeginTransaction();
                
                messageModel = session.Query<MessageOfTheDayModel>()
                    .SingleOrDefault(x => x.Id == 1);

                transaction.Commit();
                session.Close();
            }


            return messageModel.Message;
        }

        public void RefreshMessageOfTheDay()
        {
            _messageOfTheDay = LoadMessageOfDay();
        }

        public string MessageOfTheDay
        {
            get => _messageOfTheDay;
            set => _messageOfTheDay = value;
        }
    }
}