using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using System.Net;
using SI.BambooCard.Application.Models.HackerNews;


namespace SI.BambooCard.IntegrationTests
{
    public class BambooCardApplicationIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        public BambooCardApplicationIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Theory]
        [InlineData($"/api/v1/hackernews/beststories/1")]
        public async Task Get_EndpointsReturnSuccessAndCorrectContentType(string url)
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync(url);
            
            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal("application/json; charset=utf-8",
            response.Content.Headers.ContentType.ToString());
        }

        [Theory]
        [InlineData($"/api/v1/hackernews/beststories/")]
        public async Task Get_WhenQueryArgIsNotPassed_ReturnsNotFound(string url)
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync(url);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }


        [Theory]
        [InlineData(1)]
        [InlineData(200)]
        public async Task Get_WhenArgPassed_ReturnsProperNumbersOfDtos(int takeElements)
        {
            // Arrange
            var client = _factory.CreateClient();
            var url = $"/api/v1/hackernews/beststories/{takeElements}";

            // Act
            var response = await client.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();
            var actualResult = Deserialize<IList<ItemDto>>(content);

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(takeElements, actualResult.Count()); 
        }

        private static T Deserialize<T>(string content)
        {
            return JsonConvert.DeserializeObject<T>(content, new JsonSerializerSettings()
            {
                Error = (sender, error) => error.ErrorContext.Handled = true
            });
        }
    }
}