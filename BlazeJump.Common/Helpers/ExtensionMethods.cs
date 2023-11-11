using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;

namespace BlazeJump.Helpers
{
	public static class ExtensionMethods
	{
		public static T Clone<T>(this T source)
		{
			var serialized = JsonConvert.SerializeObject(source);
			return JsonConvert.DeserializeObject<T>(serialized);
		}
		public static string ToHashString(this byte[] inputBytes)
		{
				return BitConverter.ToString(inputBytes).Replace("-", "").ToLower();
		}
		public static byte[] SHA256Hash(this string inputString)
		{
			using (SHA256 sha256 = SHA256.Create())
			{
				return sha256.ComputeHash(Encoding.UTF8.GetBytes(inputString));
			}
		}
		public static byte[] Pad(this byte[] data)
		{
			// Get the number of bytes needed to pad
			int padLength = 16 - (data.Length % 16);

			// Create a new array with the padded length
			byte[] paddedData = new byte[data.Length + padLength];

			// Copy the original data to the new array
			Array.Copy(data, paddedData, data.Length);

			// Fill the remaining bytes with the pad value
			for (int i = data.Length; i < paddedData.Length; i++)
			{
				paddedData[i] = (byte)padLength;
			}

			// Return the padded array
			return paddedData;
		}

		// A helper function to unpad a byte array using PKCS7
		public static byte[] Unpad(this byte[] data)
		{
			// Get the pad value from the last byte
			int padLength = data[data.Length - 1];

			// Create a new array with the unpadded length
			byte[] unpaddedData = new byte[data.Length - padLength];

			// Copy the original data without the padding to the new array
			Array.Copy(data, unpaddedData, unpaddedData.Length);

			// Return the unpadded array
			return unpaddedData;
		}
	}
}
