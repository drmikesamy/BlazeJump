using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BlazeJump.Common.Models
{
	public class Filter
    {
		[JsonProperty("ids", NullValueHandling = NullValueHandling.Ignore)] 
		public List<string>? EventIds { get; set; }
		[JsonProperty("authors", NullValueHandling = NullValueHandling.Ignore)] 
		public List<string>? Authors { get; set; }
		[JsonProperty("kinds", NullValueHandling = NullValueHandling.Ignore)] 
		public int[]? Kinds { get; set; }
		[JsonProperty("#e", NullValueHandling = NullValueHandling.Ignore)] 
		public List<string>? TaggedEventIds { get; set; }
		[JsonProperty("#p", NullValueHandling = NullValueHandling.Ignore)] 
		public List<string>? TaggedPublicKeys { get; set; }
		[JsonProperty("#t", NullValueHandling = NullValueHandling.Ignore)]
		public List<string>? TaggedKeywords { get; set; }
		[JsonProperty("since", NullValueHandling = NullValueHandling.Ignore)]
		[JsonConverter(typeof(UnixDateTimeConverter))]
		public DateTime? Since { get; set; }
		[JsonProperty("until", NullValueHandling = NullValueHandling.Ignore)]
		[JsonConverter(typeof(UnixDateTimeConverter))]
		public DateTime? Until { get; set; }
		[JsonProperty("limit", NullValueHandling = NullValueHandling.Ignore)] 
		public int? Limit { get; set; }
		[JsonProperty("search", NullValueHandling = NullValueHandling.Ignore)]
		public string? Search { get; set; }
	}
}