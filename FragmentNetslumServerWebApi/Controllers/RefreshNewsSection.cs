using System.Threading.Tasks;
using FragmentNetslumServer.Services;
using FragmentNetslumServer.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FragmentNetslumServerWebApi.Controllers;

[ApiController]
[Route("refreshNews")]
public class RefreshNewsSection
{
    private readonly ILogger<RefreshNewsSection> _logger;
    private readonly INewsService _newsService;

    public RefreshNewsSection(ILogger<RefreshNewsSection> logger , INewsService newsService)
    {
        _logger = logger;
        _newsService = newsService;
    }
    
    [HttpGet]
    public async Task<string> Get()
    {
        await _newsService.RefreshNewsList();

           
        return "News Section Refreshed";
    }
}