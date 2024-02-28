using SI.BambooCard.Application.Models.HackerNews;

namespace SI.BambooCard.Application.Services
{
    /// <summary>
    ///     IHackerNewsService.
    /// </summary>
    public interface IHackerNewsService
    {
        /// <summary>
        /// Gets a collection of stories.
        /// </summary>
        /// <param name="takeElements">number of stories</param>
        /// <returns>IEnumerable<ItemDto>.</returns>
        Task<IEnumerable<ItemDto>> GetStories(int takeElements);
    }
}
