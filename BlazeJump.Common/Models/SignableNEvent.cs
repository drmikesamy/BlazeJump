using AutoMapper;
using BlazeJump.Common.Enums;
using BlazeJump.Common.JsonConverters;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace BlazeJump.Common.Models
{
	[JsonConverter(typeof(SignableNEventConverter))]
	[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
	public class SignableNEvent
    {
		[JsonProperty(Order = 1)]
		public int Id { get; } = 0;
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
	}
}