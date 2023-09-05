using Newtonsoft.Json;

namespace BlazeJump.Client.Models.SubtleCrypto
{
    public class RSAKeyPair
    {
        [JsonProperty("privateKey", NullValueHandling = NullValueHandling.Ignore)]
        public PrivateKey PrivateKey { get; set; } = new PrivateKey();
		[JsonProperty("publicKey", NullValueHandling = NullValueHandling.Ignore)]
        public PublicKey PublicKey { get; set; } = new PublicKey();
	}
    public class PrivateKey
    {
        [JsonProperty("alg", NullValueHandling = NullValueHandling.Ignore)]
        public string Alg { get; set; }
        [JsonProperty("d", NullValueHandling = NullValueHandling.Ignore)]
        public string D { get; set; }
        [JsonProperty("qi", NullValueHandling = NullValueHandling.Ignore)]
        public string Qi { get; set; }
    }

    public class PublicKey
    {
        [JsonProperty("alg", NullValueHandling = NullValueHandling.Ignore)]
        public string Alg { get; set; }
        [JsonProperty("e", NullValueHandling = NullValueHandling.Ignore)]
        public string E { get; set; }
        [JsonProperty("ext", NullValueHandling = NullValueHandling.Ignore)]
        public bool Ext { get; set; }
        [JsonProperty("key_ops", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> KeyOps { get; set; }
        [JsonProperty("kty", NullValueHandling = NullValueHandling.Ignore)]
        public string Kty { get; set; }
        [JsonProperty("n", NullValueHandling = NullValueHandling.Ignore)]
        public string N { get; set; }
        [JsonProperty("qi", NullValueHandling = NullValueHandling.Ignore)]
        public string Qi { get; set; }
    }
}
