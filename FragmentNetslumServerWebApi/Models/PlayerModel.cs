﻿using System.Text;
using FragmentNetslumServer.Entities;
using FragmentNetslumServer.Models;

namespace FragmentNetslumServerWebApi.Models
{
    public class PlayerModel
    {
        public int save_slot;
        public string save_id;
        public string char_id;
        public string char_name;
        public string char_class;
        public int char_level;
        public string greeting;
        public int char_model;
        public string charModelFile;
        public int char_HP;
        public int char_SP;
        public int char_GP;
        public int online_god_counter;
        public int offline_godCounter;
        
        public static PlayerModel ConvertData(GameClientAsync client)
        {
            PlayerModel model = new PlayerModel();
            
            /*
             *
             *Console.WriteLine("Character Date \n save_slot "+ save_slot + "\n char_id " +Encoding.ASCII.GetString(save_id) + " \n char_name " + Encoding.ASCII.GetString(char_id) +
                              "\n char_class " + char_class + "\n char_level " + char_level + "\n greeting "+ Encoding.ASCII.GetString(greeting) +"\n charmodel " +char_model + "\n char_hp " + char_HP+
                              "\n char_sp " + char_SP + "\n char_gp " + char_GP + "\n onlien god counter "+ online_god_counter + "\n offline god counter "+ offline_godcounter +"\n\n\n\n full byte araray " + BitConverter.ToString(data));
             * 
             */

            if (null == client.save_id || null == client.char_id ||
                null == client.char_name ||  null == client.greeting)
                return null;
            
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            model.save_slot = client.save_slot;
            model.save_id = Encoding.GetEncoding("Shift-JIS").GetString(client.save_id,0,client.save_id.Length-1);
            model.char_id = Encoding.GetEncoding("Shift-JIS").GetString(client.char_id,0,client.char_id.Length-1);
            model.char_name = Encoding.GetEncoding("Shift-JIS").GetString(client.char_name,0,client.char_name.Length-1);
           
            PlayerClass playerClass = (PlayerClass) client.char_class;
            model.char_class = playerClass.ToString();
            model.char_level = client.char_level;

            model.greeting = Encoding.GetEncoding("Shift-JIS").GetString(client.greeting,0,client.greeting.Length-1);
            model.char_model = (int) client.char_model;
            model.char_HP = client.char_HP;
            model.char_SP = client.char_SP;
            model.char_GP = (int) client.char_GP;
            model.online_god_counter = client.online_god_counter;
            model.offline_godCounter = client.offline_godcounter;
            model.charModelFile = client.charModelFile;

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

        public string CharName
        {
            get => char_name;
            set => char_name = value;
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

        public int OfflineGodCounter
        {
            get => offline_godCounter;
            set => offline_godCounter = value;
        }

        public string CharModelFile
        {
            get => charModelFile;
            set => charModelFile = value;
        }
    }
    
    
}