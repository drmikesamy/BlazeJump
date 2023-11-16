﻿// Copied and modified from NethermindEth/secp256-bindings commit 03f5a8d2ce9e087df627a7db974ac025e6cd5ef4 under MIT License. Copyright (c) 2023 Nethermind.

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace BlazeJump.Common.Services.Crypto.Bindings;

public static class SecP256k1
{
	private const string LibraryName = "secp256k1";

	static SecP256k1()
	{
		Context = CreateContext(Secp256K1ContextSign | Secp256K1ContextVerify);
	}

#pragma warning disable CA1401 // P/Invokes should not be visible
	[DllImport(LibraryName)]
	public static extern IntPtr secp256k1_context_create(uint flags);

	[DllImport(LibraryName)]
	public static extern IntPtr secp256k1_context_destroy(IntPtr context);

	[DllImport(LibraryName)]
	public static extern int secp256k1_ec_seckey_verify(IntPtr context, byte[] seckey);

	[DllImport(LibraryName)]
	public static unsafe extern int secp256k1_ec_pubkey_create(IntPtr context, void* pubkey, byte[] seckey);

	[DllImport(LibraryName)]
	public static unsafe extern int secp256k1_ec_pubkey_serialize(IntPtr context, void* serializedPublicKey, ref uint outputSize, void* publicKey, uint flags);

	[DllImport(LibraryName)]
	public static extern int secp256k1_ecdsa_sign_recoverable(IntPtr context, byte[] signature, byte[] messageHash, byte[] privateKey, IntPtr nonceFunction, IntPtr nonceData);

	[DllImport(LibraryName)]
	public static extern int secp256k1_ecdsa_recoverable_signature_serialize_compact(IntPtr context, byte[] compactSignature, out int recoveryId, byte[] signature);
	[DllImport(LibraryName)]
	public static unsafe extern int secp256k1_xonly_pubkey_parse(IntPtr context, IntPtr serializedPublicKey, byte[] inputPubKey32);
	[DllImport(LibraryName)]
	public static extern int secp256k1_schnorrsig_sign(IntPtr ctx, byte[] sig, ref int nonce_is_negated, byte[] msg32, byte[] seckey, IntPtr noncefp, IntPtr ndata);
	[DllImport(LibraryName)]
	public static extern int secp256k1_schnorrsig_verify(IntPtr ctx, IntPtr sig, IntPtr msg32, int length, IntPtr pubkey);
	[DllImport(LibraryName)]
	public static unsafe extern int secp256k1_ecdsa_recoverable_signature_parse_compact(IntPtr context, void* signature, void* compactSignature, int recoveryId);

	[DllImport(LibraryName)]
	public static unsafe extern int secp256k1_ecdsa_recover(IntPtr context, void* publicKey, void* signature, byte[] message);

	[DllImport(LibraryName)]
	public static extern int secp256k1_ecdh(IntPtr context, byte[] output, byte[] publicKey, byte[] privateKey, IntPtr hashFunctionPointer, IntPtr data);

	[DllImport(LibraryName)]
	public static unsafe extern int secp256k1_ec_pubkey_parse(IntPtr ctx, void* pubkey, void* input, uint inputlen);
#pragma warning restore CA1401 // P/Invokes should not be visible

	/* constants from pycoin (https://github.com/richardkiss/pycoin)*/
	private const uint Secp256K1FlagsTypeMask = (1 << 8) - 1;

	private const uint Secp256K1FlagsTypeContext = 1 << 0;

	private const uint Secp256K1FlagsTypeCompression = 1 << 1;

	/* The higher bits contain the actual data. Do not use directly. */
	private const uint Secp256K1FlagsBitContextVerify = 1 << 8;

	private const uint Secp256K1FlagsBitContextSign = 1 << 9;
	private const uint Secp256K1FlagsBitCompression = 1 << 8;

	/* Flags to pass to secp256k1_context_create. */
	private const uint Secp256K1ContextVerify = Secp256K1FlagsTypeContext | Secp256K1FlagsBitContextVerify;

	private const uint Secp256K1ContextSign = Secp256K1FlagsTypeContext | Secp256K1FlagsBitContextSign;
	private const uint Secp256K1ContextNone = Secp256K1FlagsTypeContext;

	private const uint Secp256K1EcCompressed = Secp256K1FlagsTypeCompression | Secp256K1FlagsBitCompression;
	private const uint Secp256K1EcUncompressed = Secp256K1FlagsTypeCompression;

	private static readonly IntPtr Context;

	private static IntPtr CreateContext(uint flags)
	{
		return secp256k1_context_create(flags);
	}
	private static IntPtr DestroyContext()
	{
		return secp256k1_context_destroy(Context);
	}

	public static bool VerifyPrivateKey(byte[] privateKey)
	{
		return secp256k1_ec_seckey_verify(Context, privateKey) == 1;
	}

	public static unsafe byte[]? GetPublicKey(byte[] privateKey, bool compressed)
	{
		Span<byte> publicKey = stackalloc byte[64];
		Span<byte> serializedPublicKey = stackalloc byte[compressed ? 33 : 65];

		fixed (byte* serializedPtr = &MemoryMarshal.GetReference(serializedPublicKey), pubKeyPtr = &MemoryMarshal.GetReference(publicKey))
		{
			bool keyDerivationFailed = secp256k1_ec_pubkey_create(Context, pubKeyPtr, privateKey) == 0;

			if (keyDerivationFailed)
			{
				return null;
			}

			uint outputSize = (uint)serializedPublicKey.Length;
			uint flags = compressed ? Secp256K1EcCompressed : Secp256K1EcUncompressed;

			bool serializationFailed = secp256k1_ec_pubkey_serialize(Context, serializedPtr, ref outputSize, pubKeyPtr, flags) == 0;

			if (serializationFailed)
			{
				return null;
			}
		}

		return serializedPublicKey.ToArray();
	}

	public static byte[]? SignCompact(byte[] messageHash, byte[] privateKey, out int recoveryId)
	{
		byte[] recoverableSignature = new byte[65];
		recoveryId = 0;

		if (secp256k1_ecdsa_sign_recoverable(
			Context, recoverableSignature, messageHash, privateKey, IntPtr.Zero, IntPtr.Zero) == 0)
		{
			return null;
		}

		byte[] compactSignature = new byte[64];

		if (secp256k1_ecdsa_recoverable_signature_serialize_compact(
			Context, compactSignature, out recoveryId, recoverableSignature) == 0)
		{
			return null;
		}

		return compactSignature;
	}
	public static byte[]? SchnorrSign(byte[] messageHash, byte[] privateKey)
	{
		if (messageHash.Length != 32)
			throw new ArgumentException($"{nameof(messageHash)} must be 32 bytes");

		if (privateKey.Length != 32)
			throw new ArgumentException($"{nameof(privateKey)} must be 32 bytes");

		int nonce_is_negated = 0;
		var sigOut = new byte[64];
		return secp256k1_schnorrsig_sign(Context, sigOut, ref nonce_is_negated, messageHash, privateKey, IntPtr.Zero, (IntPtr)null) == 1 ? sigOut : null;
	}
	public static unsafe bool SchnorrVerify(byte[] signature, byte[] messageHash, byte[] publicKeyBytes)
	{
		if (signature.Length != 64)
			throw new ArgumentException($"{nameof(signature)} must be 64 bytes");

		if (messageHash.Length != 32)
			throw new ArgumentException($"{nameof(messageHash)} must be 32 bytes");

		if (publicKeyBytes.Length != 32)
			throw new ArgumentException($"{nameof(publicKeyBytes)} must be 32 bytes");

		IntPtr pubKeyPtr = Marshal.AllocHGlobal(32);
		Marshal.Copy(publicKeyBytes, 0, pubKeyPtr, 32);
		IntPtr sig64_ptr = Marshal.AllocHGlobal(64);
		Marshal.Copy(signature, 0, sig64_ptr, 64);
		IntPtr msg32_ptr = Marshal.AllocHGlobal(32);
		Marshal.Copy(messageHash, 0, msg32_ptr, 32);


		if (secp256k1_xonly_pubkey_parse(Context, pubKeyPtr, publicKeyBytes) != 1)
		{
			throw new ArgumentException($"{nameof(publicKeyBytes)} couldn't be parsed");
		}

		var v = secp256k1_schnorrsig_verify(Context, sig64_ptr, msg32_ptr, 32, pubKeyPtr);

		Marshal.FreeHGlobal(pubKeyPtr);
		Marshal.FreeHGlobal(sig64_ptr);
		Marshal.FreeHGlobal(msg32_ptr);

		return v == 1;

	}

	public static unsafe bool RecoverKeyFromCompact(Span<byte> output, byte[] messageHash, Span<byte> compactSignature, int recoveryId, bool compressed)
	{
		Span<byte> recoverableSignature = stackalloc byte[65];
		Span<byte> publicKey = stackalloc byte[64];
		int expectedLength = compressed ? 33 : 65;
		if (output.Length != expectedLength)
		{
			throw new ArgumentException($"{nameof(output)} length should be {expectedLength}");
		}

		fixed (byte*
			compactSigPtr = &MemoryMarshal.GetReference(compactSignature),
			pubKeyPtr = &MemoryMarshal.GetReference(publicKey),
			recoverableSignaturePtr = &MemoryMarshal.GetReference(recoverableSignature),
			serializedPublicKeyPtr = &MemoryMarshal.GetReference(output))
		{
			if (secp256k1_ecdsa_recoverable_signature_parse_compact(
				Context, recoverableSignaturePtr, compactSigPtr, recoveryId) == 0)
			{
				return false;
			}

			if (secp256k1_ecdsa_recover(Context, pubKeyPtr, recoverableSignaturePtr, messageHash) == 0)
			{
				return false;
			}

			uint flags = compressed ? Secp256K1EcCompressed : Secp256K1EcUncompressed;
			uint outputSize = (uint)output.Length;

			if (secp256k1_ec_pubkey_serialize(
				Context, serializedPublicKeyPtr, ref outputSize, pubKeyPtr, flags) == 0)
			{
				return false;
			}

			return true;
		}
	}

	unsafe delegate int secp256k1_ecdh_hash_function(void* output, void* x, void* y, IntPtr data);

	public static unsafe bool Ecdh(byte[] agreement, byte[] publicKey, byte[] privateKey)
	{
		int outputLength = agreement.Length;

		// TODO: should probably do that only once
		secp256k1_ecdh_hash_function hashFunctionPtr = (void* output, void* x, void* y, IntPtr d) =>
		{
			Span<byte> outputSpan = new(output, outputLength);
			Span<byte> xSpan = new(x, 32);
			if (xSpan.Length < 32)
			{
				return 0;
			}

			xSpan.CopyTo(outputSpan);
			return 1;
		};

		GCHandle gch = GCHandle.Alloc(hashFunctionPtr);
		try
		{
			IntPtr fp = Marshal.GetFunctionPointerForDelegate(hashFunctionPtr);
			{
				return secp256k1_ecdh(Context, agreement, publicKey, privateKey, fp, IntPtr.Zero) == 1;
			}
		}
		finally
		{
			gch.Free();
		}
	}

	public static byte[] EcdhSerialized(byte[] publicKey, byte[] privateKey)
	{
		Span<byte> serializedKey = stackalloc byte[65];
		ToPublicKeyArray(serializedKey, publicKey);
		byte[] key = new byte[64];
		PublicKeyParse(key, serializedKey);
		byte[] result = new byte[32];
		Ecdh(result, key, privateKey);
		return result;
	}

	public static byte[] Decompress(Span<byte> compressed)
	{
		Span<byte> serializedKey = stackalloc byte[65];
		byte[] publicKey = new byte[64];
		PublicKeyParse(publicKey, compressed);

		if (!PublicKeySerialize(serializedKey, publicKey))
		{
			throw new CryptographicException("Failed to serialize public key");
		}

		return serializedKey.ToArray();
	}

	/// <summary>
	/// Parse a variable-length public key into the pubkey object.
	/// This function supports parsing compressed (33 bytes, header byte 0x02 or
	/// 0x03), uncompressed(65 bytes, header byte 0x04), or hybrid(65 bytes, header
	/// byte 0x06 or 0x07) format public keys.
	/// </summary>
	/// <param name="publicKeyOutput">(Output) pointer to a pubkey object. If 1 is returned, it is set to a parsed version of input. If not, its value is undefined.</param>
	/// <param name="serializedPublicKey">Serialized public key.</param>
	/// <returns>True if the public key was fully valid, false if the public key could not be parsed or is invalid.</returns>
	private static unsafe bool PublicKeyParse(Span<byte> publicKeyOutput, Span<byte> serializedPublicKey)
	{
		int inputLen = serializedPublicKey.Length;
		if (inputLen != 33 && inputLen != 65)
		{
			throw new ArgumentException($"{nameof(serializedPublicKey)} must be 33 or 65 bytes");
		}

		if (publicKeyOutput.Length < 64)
		{
			throw new ArgumentException($"{nameof(publicKeyOutput)} must be {64} bytes");
		}

		fixed (byte* pubKeyPtr = &MemoryMarshal.GetReference(publicKeyOutput), serializedPtr = &MemoryMarshal.GetReference(serializedPublicKey))
		{
			return secp256k1_ec_pubkey_parse(
				Context, pubKeyPtr, serializedPtr, (uint)inputLen) == 1;
		}
	}

	/// <summary>
	/// Serialize a pubkey object into a serialized byte sequence.
	/// </summary>
	/// <param name="serializedPublicKeyOutput">65-byte (if compressed==0) or 33-byte (if compressed==1) output to place the serialized key in.</param>
	/// <param name="publicKey">The secp256k1_pubkey initialized public key.</param>
	/// <param name="flags">SECP256K1_EC_COMPRESSED if serialization should be in compressed format, otherwise SECP256K1_EC_UNCOMPRESSED.</param>
	private static unsafe bool PublicKeySerialize(Span<byte> serializedPublicKeyOutput, Span<byte> publicKey, uint flags = Secp256K1EcUncompressed)
	{
		bool compressed = (flags & Secp256K1EcCompressed) == Secp256K1EcCompressed;
		int serializedPubKeyLength = compressed ? 33 : 65;

		if (serializedPublicKeyOutput.Length < serializedPubKeyLength)
		{
			string compressedStr = compressed ? "compressed" : "uncompressed";
			throw new ArgumentException($"{nameof(serializedPublicKeyOutput)} ({compressedStr}) must be {serializedPubKeyLength} bytes");
		}

		int expectedInputLength = flags == Secp256K1EcCompressed ? 33 : 64;

		if (publicKey.Length != expectedInputLength)
		{
			throw new ArgumentException($"{nameof(publicKey)} must be {expectedInputLength} bytes");
		}

		uint newLength = (uint)serializedPubKeyLength;

		fixed (byte* serializedPtr = &MemoryMarshal.GetReference(serializedPublicKeyOutput), pubKeyPtr = &MemoryMarshal.GetReference(publicKey))
		{
			bool success = secp256k1_ec_pubkey_serialize(
				Context, serializedPtr, ref newLength, pubKeyPtr, flags) == 1;

			return success && newLength == serializedPubKeyLength;
		}
	}

	private static void ToPublicKeyArray(Span<byte> serializedKey, byte[] unmanaged)
	{
		// Define the public key array
		Span<byte> publicKey = stackalloc byte[64];

		// Add our uncompressed prefix to our key.
		Span<byte> uncompressedPrefixedPublicKey = stackalloc byte[65];
		uncompressedPrefixedPublicKey[0] = 4;
		unmanaged.AsSpan().CopyTo(uncompressedPrefixedPublicKey[1..]);

		// Parse our public key from the serialized data.
		if (!PublicKeyParse(publicKey, uncompressedPrefixedPublicKey))
		{
			throw new CryptographicException("Failed parsing public key");
		}

		// Serialize the public key
		if (!PublicKeySerialize(serializedKey, publicKey, Secp256K1EcUncompressed))
		{
			throw new CryptographicException("Failed serializing public key");
		}
	}
}