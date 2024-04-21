using BlazeJump.Common.Models.Crypto;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace BlazeJump.Common.Models
{
	public class Stats
	{
		[JsonProperty("count", NullValueHandling = NullValueHandling.Ignore)]
		public int Count { get; set; }
		[JsonProperty("approximate", NullValueHandling = NullValueHandling.Ignore)]
		public bool Approximate { get; set; }
	}

}