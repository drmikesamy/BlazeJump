using NBitcoin.Secp256k1;
namespace BlazeJump.Common.Models.Crypto
{
	public class Secp256k1KeyPair
	{
		public Secp256k1KeyPair(ECPrivKey privateKey, ECXOnlyPubKey publicKey) { 
			PrivateKey = privateKey;
			XOnlyPublicKey = publicKey;
		}
		public ECPrivKey PrivateKey { get; private set; }
		public ECXOnlyPubKey XOnlyPublicKey { get; private set; }
	}
}