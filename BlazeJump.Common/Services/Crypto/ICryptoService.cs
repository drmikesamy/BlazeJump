﻿using BlazeJump.Common.Models.Crypto;
using NBitcoin.Secp256k1;

namespace BlazeJump.Common.Services.Crypto
{
    public interface ICryptoService
    {
		ECPubKey EtherealPublicKey { get; }
		void CreateEtherealKeyPair();
		Secp256k1KeyPair GetNewSecp256k1KeyPair();
		Task<CipherIv> AesEncrypt(string plainText, string theirPublicKey, string? ivOverride = null, bool ethereal = true);
		Task<string> AesDecrypt(string base64CipherText, string theirPublicKey, string ivString, bool ethereal = true);
		string Sign(string message, bool ethereal = true);
		bool Verify(string signature, string message, string publicKey);
	}
}