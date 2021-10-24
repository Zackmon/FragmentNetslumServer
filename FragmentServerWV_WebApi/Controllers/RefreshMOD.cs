using FragmentServerWV.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FragmentServerWV_WebApi.Controllers
{ 
    [ApiController]
    [Route("motd")]
    public class RefreshMod : ControllerBase
    {
        private readonly ILogger<RefreshMod> _logger;
        
        public RefreshMod(ILogger<RefreshMod> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public string Get()
        {
           DBAcess.getInstance().RefreshMessageOfTheDay();

           
            return "Message Of the Day Refreshed";
        }

        [HttpPut]
        public IActionResult Put(string motd)
        {
            DBAcess.getInstance().SetMessageOfDay(motd);
            return Ok();
        }

    }
}