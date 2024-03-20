using NBitcoin.Secp256k1;
namespace BlazeJump.Common.Models.Crypto
{
	public class Secp256k1KeyPair
	{
		public string PrivateKeyString
		{
			get
			{
				return Convert.ToHexString(PrivateKey.sec.ToBytes());
			}
			set
			{
				var privateKeyBytes = Convert.FromHexString(value);
				if(privateKeyBytes != null && privateKeyBytes.Length == 32) {
					PrivateKey = ECPrivKey.Create(privateKeyBytes);
					PublicKey = PrivateKey.CreatePubKey();
				}
			}
		}
		public string PublicKeyString
		{
			get
			{
				return Convert.ToHexString(PublicKey.ToBytes());
			}	
		}
		public string XOnlyPublicKeyString
		{
			get
			{
				return Convert.ToHexString(XOnlyPublicKey.ToBytes());
			}
		}
		public ECPrivKey PrivateKey { get; set; }
		public ECPubKey PublicKey { get; set; }
		public ECXOnlyPubKey XOnlyPublicKey { get; set; }
	}
}