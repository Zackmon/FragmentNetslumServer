namespace FragmentNetslumServer.Models
{
    public class CharacterRepositoryModel
    {
        public virtual int PlayerID { get; set; }
        public virtual byte[] CharacterName { get; set; }
        public virtual int ClassID { get; set; }
        public virtual int CharacterLevel { get; set; }
        public virtual byte[] Greeting { get; set; }
        public virtual int GuildID { get; set; }
        public virtual int GuildMaster { get; set; }
        public virtual int ModelNumber { get; set; }
        public virtual bool OnlineStatus { get; set; }
        public virtual int accountID { get; set; }
        public virtual string characterSaveID { get; set; }
        
        public virtual int charHP { get; set; }
        public virtual int charSP { get; set; }
        public virtual int charGP { get; set; }
        public virtual int charOnlineGoat { get; set; }
        public virtual int charOfflineGoat { get; set; }
        public virtual int charGoldCoin { get; set; }
        public virtual int charSilverCoin { get; set; }
        public virtual int charBronzeCoin { get; set; }

        public CharacterRepositoryModel()
        {
            PlayerID = -1;
        }
    }
}