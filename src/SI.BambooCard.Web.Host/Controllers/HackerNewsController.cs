using Microsoft.AspNetCore.Mvc;
using SI.BambooCard.Application.Models.HackerNews;
using SI.BambooCard.Application.Services;

namespace SI.BambooCard.Web.Host.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HackerNewsController : ControllerBase
    {
        private readonly IHackerNewsService _hackerNewsService;
        public HackerNewsController(IHackerNewsService hackerNewsService)
        {
            _hackerNewsService = hackerNewsService;
        }

        [HttpGet("beststories/{takeElements}")]
        public async Task<ActionResult<IEnumerable<ItemDto>>> GetStories(int takeElements = 1)
        {
            var stories = await _hackerNewsService.GetStories(takeElements);
            return stories.Any() ?
                (ActionResult<IEnumerable<ItemDto>>)Ok(stories) :
                (ActionResult<IEnumerable<ItemDto>>)NotFound("None items was founded"); 
        }
    }
}
