using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SI.BambooCard.Core.Dto;

namespace SI.BambooCard.Application.Models.HackerNews
{
    public class ItemDto : IDto
    {
        [JsonProperty(PropertyName = "title")]
        public string? Title { get; set; }

        [JsonProperty(PropertyName = "url")]
        public string? Url { get; set; }

        [JsonProperty(PropertyName = "by")]
        public string? PostedBy { get; set; }

        [JsonProperty(PropertyName = "time")]
        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime Time { get; set; }

        [JsonProperty(PropertyName = "score")]
        public int Score { get; set; }

        [JsonProperty(PropertyName = "descendants")]
        public int CommentCount { get; set; }

    }
}
