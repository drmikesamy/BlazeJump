using BlazeJump.Common.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace BlazeJump.Common.Models
{
	[JsonConverter(typeof(NEventConverter))]
	[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
	public class NEvent
    {
        public string Id { get; set; }
		[JsonProperty("pubkey", NullValueHandling = NullValueHandling.Ignore)]
		public string? UserId { get; set; }
		public long Created_At { get; set; }
		public KindEnum? Kind { get; set; }
        public List<EventTag>? Tags { get; set; } = new List<EventTag>();
        public string? Content { get; set; }
        public string? Sig { get; set; }
		[JsonIgnore]
        public User? User { get; set; }
		[JsonIgnore]
		public DateTime CreatedAtDateTime => Helpers.UnixTimeStampToDateTime(Created_At).ToLocalTime();
        [JsonIgnore]
        public virtual Dictionary<string, NEvent> Replies { get; set; } = new Dictionary<string, NEvent>();
        [JsonIgnore]
        public int ReplyCount => Replies.Count;
        [JsonIgnore]
        public int ReactionCount { get; set; } = 0;
        [JsonIgnore]
        public bool Verified { get; set; } = false;
        public NEvent GetSignableNEvent()
        {
			return new NEvent
            {
                UserId = this.UserId,
                Created_At = this.Created_At,
                Kind = this.Kind,
                Tags = this.Tags,
                Content = this.Content,
            };
        }
    }

}