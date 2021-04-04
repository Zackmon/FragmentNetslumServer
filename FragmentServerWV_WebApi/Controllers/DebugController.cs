using FragmentServerWV.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace FragmentServerWV_WebApi.Controllers
{

    [ApiController]
    [Route("debug")]
    public class DebugController : ControllerBase
    {
        private readonly ILogger<DebugController> logger;
        private readonly ILobbyChatService lobbyChatService;

        public DebugController(
            ILogger<DebugController> logger,
            ILobbyChatService lobbyChatService)
        {
            this.logger = logger;
            this.lobbyChatService = lobbyChatService;
        }


        [HttpGet]
        public async Task<IActionResult> Get()
        {
            // foreach(var lobby in lobbyChatService.)
            await lobbyChatService.Main.SendServerMessageAsync("This is a test message from a WebAPI controller");
            return Ok();
        }
    }
}
