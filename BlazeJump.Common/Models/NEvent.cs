using BlazeJump.Common.Enums;
using BlazeJump.Common.Helpers;
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
		public DateTime CreatedAtDateTime => GeneralHelpers.UnixTimeStampToDateTime(Created_At).ToLocalTime();
        [JsonIgnore]
        public string RootId => Tags?.FirstOrDefault(t => t.Key == TagEnum.e && t.Value3 == "root")?.Value;
        [JsonIgnore]
        public string ParentId => Tags?.FirstOrDefault(t => t.Key == TagEnum.e && t.Value3 == "reply")?.Value;
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