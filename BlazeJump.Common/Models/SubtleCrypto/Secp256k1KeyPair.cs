using Newtonsoft.Json;

namespace BlazeJump.Common.Models.SubtleCrypto
{
    public class Secp256k1KeyPair
    {
        public string PrivateKey { get; set; } = String.Empty;
        public string PublicKey { get; set; } = String.Empty;
	}
    public class PrivateKey
    {
		[JsonProperty("crv", NullValueHandling = NullValueHandling.Ignore)]
		public string Curve { get; set; }
		[JsonProperty("d", NullValueHandling = NullValueHandling.Ignore)]
		public string D { get; set; }
		[JsonProperty("ext", NullValueHandling = NullValueHandling.Ignore)]
		public bool Ext { get; set; }
		[JsonProperty("key_ops", NullValueHandling = NullValueHandling.Ignore)]
		public List<string> KeyOps { get; set; }
		[JsonProperty("kty", NullValueHandling = NullValueHandling.Ignore)]
		public string Kty { get; set; }
		[JsonProperty("x", NullValueHandling = NullValueHandling.Ignore)]
		public string X { get; set; }
		[JsonProperty("y", NullValueHandling = NullValueHandling.Ignore)]
		public string Y { get; set; }
	}

    public class PublicKey
    {
        [JsonProperty("crv", NullValueHandling = NullValueHandling.Ignore)]
        public string Curve { get; set; }
        [JsonProperty("ext", NullValueHandling = NullValueHandling.Ignore)]
        public bool Ext { get; set; }
        [JsonProperty("key_ops", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> KeyOps { get; set; }
        [JsonProperty("kty", NullValueHandling = NullValueHandling.Ignore)]
        public string Kty { get; set; }
        [JsonProperty("x", NullValueHandling = NullValueHandling.Ignore)]
        public string X { get; set; }
        [JsonProperty("y", NullValueHandling = NullValueHandling.Ignore)]
        public string Y { get; set; }
    }
}
