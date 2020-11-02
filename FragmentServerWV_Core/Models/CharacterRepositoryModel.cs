namespace FragmentServerWV.Models
{
    public class CharacterRepositoryModel
    {
        public virtual int PlayerID { get; set; }
        public virtual byte[] CharachterName { get; set; }
        public virtual int ClassID { get; set; }
        public virtual int CharachterLevel { get; set; }
        public virtual byte[] Greeting { get; set; }
        public virtual int GuildID { get; set; }
        public virtual int GuildMaster { get; set; }
        public virtual int ModelNumber { get; set; }
        public virtual bool OnlineStatus { get; set; }
        public virtual int accountID { get; set; }
        public virtual string charachterSaveID { get; set; }

        public CharacterRepositoryModel()
        {
            PlayerID = -1;
        }
    }
}