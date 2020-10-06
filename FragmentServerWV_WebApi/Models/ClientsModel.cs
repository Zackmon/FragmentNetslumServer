namespace FragmentServerWV_WebApi.Models
{
    public class ClientsModel
    { 
        public int save_slot;
        public string save_id;
        public string char_id;
        public string char_name;
        public int char_class;
        public int char_level;
        public string greeting;
        public int char_model;
        public int char_HP;
        public int char_SP;
        public int char_GP;
        public int online_god_counter;
        public int offline_godcounter;


        public ClientsModel()
        {
        }

        public ClientsModel(int saveSlot, string saveId, string charId,string charName, int charClass, int charLevel, string greeting, int charModel, int charHp, int charSp, int charGp, int onlineGodCounter, int offlineGodcounter)
        {
            save_slot = saveSlot;
            save_id = saveId;
            char_id = charId;
            char_name = charName;
            char_class = charClass;
            char_level = charLevel;
            this.greeting = greeting;
            char_model = charModel;
            char_HP = charHp;
            char_SP = charSp;
            char_GP = charGp;
            online_god_counter = onlineGodCounter;
            offline_godcounter = offlineGodcounter;
        }

        public int SaveSlot
        {
            get => save_slot;
            set => save_slot = value;
        }

        public string SaveId
        {
            get => save_id;
            set => save_id = value;
        }

        public string CharId
        {
            get => char_id;
            set => char_id = value;
        }

        public int CharClass
        {
            get => char_class;
            set => char_class = value;
        }

        public int CharLevel
        {
            get => char_level;
            set => char_level = value;
        }

        public string Greeting
        {
            get => greeting;
            set => greeting = value;
        }

        public int CharModel
        {
            get => char_model;
            set => char_model = value;
        }

        public int CharHp
        {
            get => char_HP;
            set => char_HP = value;
        }

        public int CharSp
        {
            get => char_SP;
            set => char_SP = value;
        }

        public int CharGp
        {
            get => char_GP;
            set => char_GP = value;
        }

        public int OnlineGodCounter
        {
            get => online_god_counter;
            set => online_god_counter = value;
        }

        public int OfflineGodcounter
        {
            get => offline_godcounter;
            set => offline_godcounter = value;
        }
    }
}