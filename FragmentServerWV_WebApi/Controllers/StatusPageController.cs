using System;
using System.Collections.Generic;
using FragmentServerWV;
using FragmentServerWV_WebApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FragmentServerWV_WebApi.Controllers
{
    [ApiController]
    [Route("status")]
    public class StatusPageController : ControllerBase
    {
        private readonly ILogger<StatusPageController> _logger;

        public StatusPageController(ILogger<StatusPageController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public ClientsModel Get()
        {
            // ClientsModel test = new ClientsModel(0, "13041305062c00000631", "130413072018000064e2","Zackmon", "1", 1, "よろしく", 1,
            //     200, 100, 1000, 0, 0);

            ClientsModel clientList = new ClientsModel();
            // clientList.Add(test);

            foreach (GameClient client in Server.Instance.GameClientService.Clients)
            {
                if (!client._exited)
                {
                    if (client.isAreaServer)
                    {
                        AreaServerModel model = AreaServerModel.ConvertDate(client);
                        if (model!= null)
                                clientList._areaServerList.Add(model);
                    }
                    else
                    {
                        PlayerModel model = PlayerModel.ConvertData(client);
                        if (model != null)
                            clientList.PlayerList.Add(model);
                    }
                }
            }

            return clientList;
        }
    }
}