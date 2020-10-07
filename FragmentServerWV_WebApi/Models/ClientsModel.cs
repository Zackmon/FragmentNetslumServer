
using System;
using System.Text;
using FragmentServerWV.Models;

namespace FragmentServerWV_WebApi.Models
{
    public class ClientsModel
    { 
        public int save_slot;
        public string save_id;
        public string char_id;
        public string char_name;
        public string char_class;
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

        public ClientsModel(int saveSlot, string saveId, string charId,string charName, string charClass, int charLevel, string greeting, int charModel, int charHp, int charSp, int charGp, int onlineGodCounter, int offlineGodcounter)
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

        public static ClientsModel ConvertData(FragmentServerWV.GameClient client)
        {
            ClientsModel model = new ClientsModel();
            
            /*
             *
             *Console.WriteLine("Character Date \n save_slot "+ save_slot + "\n char_id " +Encoding.ASCII.GetString(save_id) + " \n char_name " + Encoding.ASCII.GetString(char_id) +
                              "\n char_class " + char_class + "\n char_level " + char_level + "\n greeting "+ Encoding.ASCII.GetString(greeting) +"\n charmodel " +char_model + "\n char_hp " + char_HP+
                              "\n char_sp " + char_SP + "\n char_gp " + char_GP + "\n onlien god counter "+ online_god_counter + "\n offline god counter "+ offline_godcounter +"\n\n\n\n full byte araray " + BitConverter.ToString(data));
             * 
             */

            model.save_slot = client.save_slot;
            model.save_id = Encoding.ASCII.GetString(client.save_id);
            model.char_id = Encoding.ASCII.GetString(client.char_id);
            model.char_name = Encoding.ASCII.GetString(client.char_name);
            PlayerClass playerClass = (PlayerClass) client.char_class;
            model.char_class = playerClass.ToString();
            model.char_level = client.char_class;
            model.greeting = Encoding.ASCII.GetString(client.greeting);
            model.char_model = Convert.ToInt32(client.char_model);
            model.char_HP = Convert.ToInt32(client.char_HP);
            model.char_SP = Convert.ToInt32(client.char_SP);
            model.char_GP = Convert.ToInt32(client.char_GP);
            model.online_god_counter = Convert.ToInt32(client.online_god_counter);
            model.offline_godcounter = Convert.ToInt32(client.offline_godcounter);

            return model;


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

        public string CharClass
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