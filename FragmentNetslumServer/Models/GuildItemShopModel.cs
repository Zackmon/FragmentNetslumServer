namespace FragmentNetslumServer.Models
{
    public class GuildItemShopModel
    {
        public virtual int ItemShopID { get; set; }


        public virtual int GuildID { get; set; }


        public virtual int ItemID { get; set; }


        public virtual int Quantity { get; set; }


        public virtual int GeneralPrice { get; set; }


        public virtual int MemberPrice { get; set; }


        public virtual bool AvailableForGeneral { get; set; }


        public virtual bool AvailableForMember { get; set; }

        public GuildItemShopModel()
        {
            ItemShopID = -1;
        }
    }
}