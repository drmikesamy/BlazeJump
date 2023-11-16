namespace BlazeJump.Common.Models.Crypto
{
	public class Secp256k1KeyPair
	{
		private byte[] _privateKeyBytes { get; set; }
		private byte[] _publicKeyBytes { get; set; }
		private string _privateKey { get; set; }
		private string _publicKey { get; set; }
		public byte[] PrivateKeyBytes
		{
			get
			{
				return _privateKeyBytes;
			}
			set
			{
				_privateKeyBytes = value;
				_privateKey = Convert.ToHexString(value);
			}
		}
		public byte[] PublicKeyBytes
		{
			get
			{
				return _publicKeyBytes;
			}
			set
			{
				_publicKeyBytes = value;
				_publicKey = Convert.ToHexString(value);
			}
		}
		public string PrivateKey
		{
			get
			{
				return _privateKey;
			}
			set
			{
				_privateKey = value;
				_privateKeyBytes = Convert.FromHexString(value);
			}
		}
		public string PublicKey
		{
			get
			{
				return _publicKey;
			}
			set
			{
				_publicKey = value;
				_publicKeyBytes = Convert.FromHexString(value);
			}
		}
	}
}