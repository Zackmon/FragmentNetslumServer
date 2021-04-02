using FragmentServerWV;
using FragmentServerWV.Services.Interfaces;
using FragmentServerWV_WebApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;

namespace FragmentServerWV_WebApi.Controllers
{
    [ApiController]
    [Route("status")]
    public class StatusPageController : ControllerBase
    {
        private readonly ILogger<StatusPageController> _logger;
        private readonly IClientProviderService clientProviderService;
        private readonly IClientConnectionService clientConnectionService;

        public StatusPageController(
            ILogger<StatusPageController> logger,
            IClientProviderService clientProviderService,
            IClientConnectionService clientConnectionService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.clientProviderService = clientProviderService ?? throw new ArgumentNullException(nameof(clientProviderService));
            this.clientConnectionService = clientConnectionService ?? throw new ArgumentNullException(nameof(clientConnectionService));
        }

        [HttpGet]
        public ClientsModel Get()
        {
            ClientsModel clientList = new ClientsModel();
            foreach (var client in clientProviderService.Clients)
            {
                if (client.IsAreaServer)
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

            return clientList;
        }
    }
}