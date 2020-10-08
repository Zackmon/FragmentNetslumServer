
using System;
using System.Collections.Generic;
using System.Text;
using FragmentServerWV.Models;

namespace FragmentServerWV_WebApi.Models
{
    public class ClientsModel
    { 
        public List<PlayerModel> _playerList = new List<PlayerModel>();
        public List<AreaServerModel> _areaServerList = new List<AreaServerModel>();


        public List<PlayerModel> PlayerList
        {
            get => _playerList;
            set => _playerList = value;
        }

        public List<AreaServerModel> AreaServerList
        {
            get => _areaServerList;
            set => _areaServerList = value;
        }
    }
}