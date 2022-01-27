namespace FragmentNetslumServer.Models
{
    public class PlayerAccountIDModel
    {
        public virtual int ID { get; set; }
        public virtual string saveID { get; set; }

        public PlayerAccountIDModel()
        {
            ID = -1;
        }
    }
}