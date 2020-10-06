using System.Collections.Generic;
using FragmentServerWV_WebApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MySqlX.XDevAPI;

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
        public IEnumerable<ClientsModel> Get()
        {
            ClientsModel test = new ClientsModel(0, "13041305062c00000631", "130413072018000064e2", 1, 1, "よろしく", 1,
                200, 100, 1000, 0, 0);
            
            List<ClientsModel> clientsModels = new List<ClientsModel>();
            clientsModels.Add(test);

            return clientsModels;
        }
    }
}