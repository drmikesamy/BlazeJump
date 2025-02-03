using AutoMapper;
using BlazeJump.Common.Enums;
using BlazeJump.Common.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace BlazeJump.Common.Models
{
	[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
	public class NEvent
    {
		[JsonProperty(Order = 1)]
		public string Id { get; set; }
		[JsonProperty(Order = 2)]
		public string? Pubkey { get; set; }
		[JsonProperty(Order = 3)]
		public long Created_At { get; set; }
		[JsonProperty(Order = 4)]
		public KindEnum? Kind { get; set; }
		[JsonProperty(Order = 5)]
		public List<EventTag>? Tags { get; set; } = new List<EventTag>();
		[JsonProperty(Order = 6)]
		public string? Content { get; set; }
		[JsonProperty(Order = 7)]
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
	}

}