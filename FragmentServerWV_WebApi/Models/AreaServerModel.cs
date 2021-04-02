using System;
using System.Text;

namespace FragmentServerWV_WebApi.Models
{
    public class AreaServerModel
    {
        private string _serverName;
        private int _serverLevel;
        private string _serverStatus;
        private int _numberOfPlayers;


        public static AreaServerModel ConvertDate(FragmentServerWV.Entities.GameClientAsync client)
        {
            if (null == client.AreaServerName) 
                return null;
                

            AreaServerModel model = new AreaServerModel();
            
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            
            model._serverName = Encoding.GetEncoding("Shift-JIS").GetString(client.AreaServerName,0,client.AreaServerName.Length-1);
            model._serverLevel = client.AreaServerLevel;
            model._serverStatus = client.AreaServerStatus == 0 ? "Available" : "Busy";
            model._numberOfPlayers = client.Players;
            
            return model;
        }

        public string ServerName
        {
            get => _serverName;
            set => _serverName = value;
        }

        public int ServerLevel
        {
            get => _serverLevel;
            set => _serverLevel = value;
        }

        public string ServerStatus
        {
            get => _serverStatus;
            set => _serverStatus = value;
        }

        public int NumberOfPlayers
        {
            get => _numberOfPlayers;
            set => _numberOfPlayers = value;
        }
    }
}