using NBitcoin.Secp256k1;
namespace BlazeJump.Common.Models.Crypto
{
	public class Secp256k1KeyPair
	{
		public Secp256k1KeyPair(ECPrivKey privateKey, ECPubKey publicKey) { 
			PrivateKey = privateKey;
			PublicKey = publicKey;
		}
		public ECPrivKey PrivateKey { get; private set; }
		public ECPubKey PublicKey { get; private set; }
	}
}