using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BlazeJump.Common.Services.Crypto.Bindings
{
	public static class TinyAes
	{
		private const string LibraryName = "tinyaes";

#pragma warning disable CA1401 // P/Invokes should not be visible
		[DllImport(LibraryName)]
		public static extern void AES_init_ctx(ref AES_ctx ctx, byte[] key);

		[DllImport(LibraryName)]
		public static extern void AES_init_ctx_iv(ref AES_ctx ctx, byte[] key, byte[] iv);

		[DllImport(LibraryName)]
		public static extern void AES_ctx_set_iv(ref AES_ctx ctx, byte[] iv);

		[DllImport(LibraryName)]
		public static extern void AES_ECB_encrypt(ref AES_ctx ctx, byte[] buf);

		[DllImport(LibraryName)]
		public static extern void AES_ECB_decrypt(ref AES_ctx ctx, byte[] buf);

		[DllImport(LibraryName)]
		public static extern void AES_CBC_encrypt_buffer(ref AES_ctx ctx, byte[] buf, uint length);

		[DllImport(LibraryName)]
		public static extern void AES_CBC_decrypt_buffer(ref AES_ctx ctx, byte[] buf, uint length);

		[DllImport(LibraryName)]
		public static extern void AES_CTR_xcrypt_buffer(ref AES_ctx ctx, byte[] buf, uint length);

#pragma warning restore CA1401 // P/Invokes should not be visible

		public static byte[]? Encrypt(string message, byte[] key, byte[] iv)
		{
			byte[] messageBytes = Encoding.UTF8.GetBytes(message);
			uint messageLength = ((uint)messageBytes.Length);

			var ctx = new AES_ctx();
			AES_init_ctx_iv(ref ctx, key, iv);
			AES_CBC_encrypt_buffer(ref ctx, messageBytes, messageLength);

			return messageBytes;
		}
		public static byte[]? Decrypt(string base64CiperText, byte[] key, byte[] iv)
		{
			byte[] cipherBytes = Convert.FromBase64String(base64CiperText);
			uint cipherLength = ((uint)cipherBytes.Length);

			var ctx = new AES_ctx();
			AES_init_ctx_iv(ref ctx, key, iv);
			AES_CBC_decrypt_buffer(ref ctx, cipherBytes, cipherLength);

			return cipherBytes;
		}
	}
	// The struct definition must match the native one
	[StructLayout(LayoutKind.Sequential)]
	public struct AES_ctx
	{
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4 * 4 * 15)]
		public uint[] RoundKey;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
		public byte[] Iv;
	}
}
