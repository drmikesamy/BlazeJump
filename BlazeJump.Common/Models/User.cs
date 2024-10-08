using BlazeJump.Common.Enums;
using BlazeJump.Common.Models.Crypto;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace BlazeJump.Common.Models
{
	public class User
	{
		[Key]
		[JsonIgnore]
		public string Id { get; set; }
		[JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
		public string? Username { get; set; }
		[JsonProperty("about", NullValueHandling = NullValueHandling.Ignore)]
		public string? Bio { get; set; }
		[JsonIgnore]
		public string? Email { get; set; }
		[JsonIgnore]
		public string? Password { get; set; }
		[JsonIgnore]
		public string? RepeatPassword { get; set; }
		[JsonProperty("picture", NullValueHandling = NullValueHandling.Ignore)]
		public string? ProfilePic { get; set; }
		[JsonProperty("banner", NullValueHandling = NullValueHandling.Ignore)]
		public string? Banner { get; set; }
		[JsonIgnore]
		public ICollection<NEvent> Events { get; set; } = new List<NEvent>();
	}

}