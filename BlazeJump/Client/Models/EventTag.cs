using BlazeJump.Client.Enums;
using Newtonsoft.Json;

namespace BlazeJump.Client.Models
{
	[JsonConverter(typeof(TagConverter))]
	public class EventTag
	{
		public long Id { get; set; }
		public string NEventId { get; set; }
		public TagEnum Key { get; set; }
		public string? Value { get; set; }
		public string? Value2 { get; set; }
		public string? Value3 { get; set; }
		public string? Value4 { get; set; }
	}

}