namespace FragmentNetslumServer.Models
{
    public class GuildRepositoryModel
    {
        public virtual int GuildID { get; set; }


        public virtual byte[] GuildName { get; set; }


        public virtual byte[] GuildEmblem { get; set; }


        public virtual byte[] GuildComment { get; set; }


        public virtual string EstablishmentDate { get; set; }


        public virtual int MasterPlayerID { get; set; }


        public virtual int GoldCoin { get; set; }


        public virtual int SilverCoin { get; set; }


        public virtual int BronzeCoin { get; set; }


        public virtual int Gp { get; set; }
    }
}