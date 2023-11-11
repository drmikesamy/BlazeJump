using Newtonsoft.Json;

namespace BlazeJump.Common.Models.NostrConnect
{
	public class NostrConnectResponse
	{
		[JsonProperty("id")]
		public string Id { get; set; }

		[JsonProperty("result")]
		public string Result { get; set; }

		[JsonProperty("error")]
		public string? Error { get; set; }
	}
}
