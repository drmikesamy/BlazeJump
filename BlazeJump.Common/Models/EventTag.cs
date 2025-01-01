using BlazeJump.Common.Enums;
using Newtonsoft.Json;

namespace BlazeJump.Common.Models
{
	[JsonConverter(typeof(TagConverter))]
	public class EventTag
	{
		public TagEnum Key { get; set; }
		public string? Value { get; set; }
		public string? Value2 { get; set; }
		public string? Value3 { get; set; }
		public string? Value4 { get; set; }
	}

}