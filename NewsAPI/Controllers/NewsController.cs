using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NewsAPI.Entities;
using NewsAPI.Entities.Models;
using NewsAPI.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NewsAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class NewsController : Controller
    {
        private readonly ILogger<NewsController> _logger;

        public readonly NewsService _newsService;

        public NewsController(ILogger<NewsController> logger, NewsService newssService)
        {
            _logger = logger;
            _newsService = newssService;
        }

        [HttpGet("{newsId}")]
        public async Task<ActionResult<News>> Get(long newsId)
        {
            return await _newsService.Get(newsId);
        }

        [HttpPost]
        public async Task<ActionResult> Post(AddInputNewsModel news)
        {
            await _newsService.Add(news);

            return NoContent();
        }

        [HttpPut("{newsId}")]
        public async Task<ActionResult> Put(long newsId, [FromBody] UpdateInputNewsModel news)
        {
            await _newsService.Update(newsId, news);

            return NoContent();
        }

        [HttpDelete("{newsId}")]
        public async Task<ActionResult> Delete(long newsId)
        {
            await _newsService.Delete(newsId);

            return NoContent();
        }
    }
}
