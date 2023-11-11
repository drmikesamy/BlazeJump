using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazeJump.Common.Models.NostrConnect
{
	public class NostrConnectRequest
	{
		[JsonProperty("id")]
		public string Id { get; set; }

		[JsonProperty("method")]
		public string Method { get; set; }

		[JsonProperty("params")]
		public List<string> Params { get; set; } = new();
	}
}
