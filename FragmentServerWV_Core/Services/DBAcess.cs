using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FragmentServerWV.Models;
using NHibernate;
using NHibernate.Cfg;

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
                ICriteria criteria = session.CreateCriteria(typeof(BbsCategoryModel));
                IList<BbsCategoryModel> bbsCategoryModels = session.Query<BbsCategoryModel>().ToList();
                categoryList.AddRange(bbsCategoryModels);
                session.Close();
            }

            return categoryList;
        }


        public List<BbsThreadModel> getThreadsByCategoryID(int categoryID)
        {
            List<BbsThreadModel> threadLists = new List<BbsThreadModel>();

            using (ISession session = _sessionFactory.OpenSession())
            {
                threadLists.AddRange(
                    session.Query<BbsThreadModel>().Where
                            (x => x.categoryID == categoryID)
                        .ToList());
                session.Close();
            }

            return threadLists;
        }


        public List<BbsPostMetaModel> GetPostsMetaByThreadId(int threadID)
        {
            List<BbsPostMetaModel> postMetaList = new List<BbsPostMetaModel>();
            using ISession session = _sessionFactory.OpenSession();

            postMetaList.AddRange(
                session.Query<BbsPostMetaModel>().Where
                        (x => x.threadID == threadID)
                    .ToList());

            session.Close();

            return postMetaList;
        }


        public BbsPostBody GetPostBodyByPostId(int postID)
        {
            using ISession session = _sessionFactory.OpenSession();

            BbsPostBody postBody = session.Query<BbsPostBody>().SingleOrDefault(x => x.postID == postID);

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
            meta.username = username;
            meta.title = postTitle;
            meta.subtitle = postTitle.Substring(0, 16);
            meta.unk3 = "unk3";
            meta.threadID = threadID;

            using (ISession session = _sessionFactory.OpenSession())
            {
                try
                {
                    meta.unk0 = session.Query<BbsPostMetaModel>().Max(x => (int?) x.unk0).Value + 1;
                }
                catch (Exception e)
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

        public void PlayerLogin(GameClient client)
        {
            RankingDataModel model = new RankingDataModel();


            DateTime dateTime = DateTime.UtcNow;

            model.antiCheatEngineResult = "LEGIT";
            model.loginTime = dateTime.ToString("ddd MMM dd hh:mm:ss yyyy");
            model.diskID = "DUMMY DISK ID VALUE !";
            model.saveID = _encoding.GetString(client.save_id, 0, client.save_id.Length - 1);
            model.characterSaveID = _encoding.GetString(client.char_id, 0, client.char_id.Length - 1);
            model.characterName = _encoding.GetString(client.char_name, 0, client.char_name.Length - 1);

            PlayerClass playerClass = (PlayerClass) client.char_class;
            model.characterClassName = playerClass.ToString();
            model.characterLevel = client.char_level;

            model.characterHP = client.char_HP;
            model.characterSP = client.char_SP;
            model.characterGP = (int) client.char_GP;
            model.godStatusCounterOnline = client.online_god_counter;
            model.averageFieldLevel = client.offline_godcounter;
            model.accountID = client.AccountId;

            using ISession session = _sessionFactory.OpenSession();

            using ITransaction transaction = session.BeginTransaction();

            session.Save(model);
            transaction.Commit();

            session.Close();
        }


        public int GetPlayerAccountId(string saveID)
        {
            PlayerAccountIDModel playerAccountIdModel;

            using (ISession session = _sessionFactory.OpenSession())
            {
                playerAccountIdModel = session.Query<PlayerAccountIDModel>()
                    .SingleOrDefault(x => x.saveID == saveID);

                if (playerAccountIdModel == null || playerAccountIdModel.ID == -1 || playerAccountIdModel.ID == 0)
                {
                    Console.WriteLine("the Save ID " + saveID +
                                      " does not have an associated account ID , creating a new Account ID");

                    using (ITransaction transaction = session.BeginTransaction())
                    {
                        playerAccountIdModel = new PlayerAccountIDModel();
                        playerAccountIdModel.saveID = saveID;

                        session.Save(playerAccountIdModel);
                        transaction.Commit();
                    }

                    Console.WriteLine("the new Account ID for the save " + saveID + " is " + playerAccountIdModel.ID);
                }

                session.Close();
            }

            return playerAccountIdModel.ID;
        }

        public Boolean checkForNewMailByAccountID(int accountID)
        {
            Boolean newMail = false;
            using (ISession session = _sessionFactory.OpenSession())
            {
                newMail = session.Query<MailMetaModel>()
                    .Any(x => x.Receiver_Account_ID == accountID
                              && x.Mail_Delivered == false);

                session.Close();
            }

            return newMail;
        }

        public List<MailMetaModel> GetAccountMail(int accountID)
        {
            List<MailMetaModel> metaList = new List<MailMetaModel>();

            using ISession session = _sessionFactory.OpenSession();

            metaList = session.Query<MailMetaModel>()
                .Where(x => x.Receiver_Account_ID == accountID && x.Mail_Delivered == false)
                .ToList();

            //set the records as delivered 
            if (metaList.Count > 0)
            {
                using ITransaction transaction = session.BeginTransaction();
                foreach (MailMetaModel meta in metaList)
                {
                    meta.Mail_Delivered = true;
                    session.SaveOrUpdate(meta);
                }
                
                transaction.Commit();
            }

            session.Close();

            return metaList;
        }

        public MailBodyModel GetMailBodyByMailId(int mail_ID)
        {
            MailBodyModel bodyModel = new MailBodyModel();

            using (ISession session = _sessionFactory.OpenSession())
            {
                bodyModel = session.Query<MailBodyModel>()
                    .SingleOrDefault(x => x.Mail_ID == mail_ID);

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
                messageModel = session.Query<MessageOfTheDayModel>()
                    .SingleOrDefault(x => x.Id == 1);

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