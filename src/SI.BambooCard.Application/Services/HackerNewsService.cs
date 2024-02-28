using System.Text;
using System.Net.Mime;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SI.BambooCard.Application.Models.HackerNews;
using SI.BambooCard.Core.Dto;

namespace SI.BambooCard.Application.Services
{
    public class HackerNewsService : IHackerNewsService
    { 
        private readonly ILogger<HackerNewsService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly HttpClient _httpClient;
        private readonly Uri _bestStoriesUrl;
        private readonly Uri _bestItemDetailsUrl;
        private const string _hackerNewsApiName = "HackerNewsApi";
        private const string _version = "v0";
        private const int _maxConcurrency = 20;

        public HackerNewsService(IHttpClientFactory httpClientFactory, ILogger<HackerNewsService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _httpClient = _httpClientFactory.CreateClient(_hackerNewsApiName);
            _httpClient.BaseAddress = new Uri($"{_httpClient.BaseAddress}/{_version}");
            _bestStoriesUrl = new Uri($"{_httpClient.BaseAddress}/beststories.json?print=pretty");
            _bestItemDetailsUrl = new Uri($"{_httpClient.BaseAddress}/item");
            _logger = logger;
        }

        /// <summary>
        /// Gets a collection of stories.
        /// </summary>
        /// <param name="takeElements">number of stories</param>
        /// <returns>IEnumerable<ItemDto>.</returns>
        public async Task<IEnumerable<ItemDto>> GetStories(int takeElements)
        {
            // 1). Get a set of stories collection 
            var storiesIDsContent = await GetContent(RequestMessage(_bestStoriesUrl, HttpMethod.Get));
            // 2). Deserialize item IDs
            var bestStoriesIDs = Deserialize<IEnumerable<int>>(storiesIDsContent);
            if (bestStoriesIDs == null || !bestStoriesIDs.Any())
            {
                _logger.LogError("Failed to retrieve {bestStoriesIDs} story IDs.", bestStoriesIDs);
                return Enumerable.Empty<ItemDto>();
            }
            // 3). Execute reading stories details in parallel in batches           
            var result = await ExecuteBatchRun(bestStoriesIDs.Take(takeElements).ToList().AsReadOnly());

            return (IEnumerable<ItemDto>)result;
        }

        private async Task<IReadOnlyCollection<IDto>> ExecuteBatchRun(IReadOnlyList<int> bestStoriesIds)
        {
            var semaphore = new SemaphoreSlim(_maxConcurrency);
            var responses = new ConcurrentBag<Task<ItemDto>>(); 
            var tasks = bestStoriesIds.Select(async id =>
            {
                // wait for each n tasks before running the next n tasks
                // throttle the number of threads to n
                await semaphore.WaitAsync();
                var task = GetStory<ItemDto>(id);
                try
                { 
                    responses.Add(task);
                    return await task;
                }
                finally
                {
                    semaphore.Release();
                }
            });

            _logger.LogTrace("Trying to get details on {bestStoriesIds} stories in batch mode.", bestStoriesIds);
            await Task.WhenAll(tasks);

            return responses.Select(x => x.Result).ToList().AsReadOnly();
        }

        private async Task<T> GetStory<T>(int id) where T : IDto, new()
        { 
            var content = await GetContent(RequestMessage(GetStoryDetailUri(id), HttpMethod.Get)) ?? string.Empty;
            var result = Deserialize<T>(content);
            return result ?? new T();
        }
         
        private async Task<string> GetContent(HttpRequestMessage httpRequestMessage)
        {
            var response = await _httpClient.SendAsync(httpRequestMessage).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            response.Content?.Dispose();
            return content;
        }

        private Uri GetStoryDetailUri(int id)
        {
            return new Uri($"{_bestItemDetailsUrl}/{id}.json?print=pretty");
        }
        private static T? Deserialize<T>(string content)
        {
            return JsonConvert.DeserializeObject<T>(content, new JsonSerializerSettings()
            {
                Error = (sender, error) => error.ErrorContext.Handled = true
            });
        }

        private static HttpRequestMessage RequestMessage(Uri uri, HttpMethod method)
        {
            return new HttpRequestMessage
            {
                Method = method,
                RequestUri = uri,
                Content = new StringContent("default.json", Encoding.UTF8, MediaTypeNames.Application.Json),
            };
        }
    }
}
