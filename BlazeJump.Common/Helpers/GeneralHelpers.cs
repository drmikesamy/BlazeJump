using Nano.Bech32;

namespace BlazeJump.Common.Helpers
{
	public static class GeneralHelpers
	{
		public static DateTime UnixTimeStampToDateTime(long unixTimeStamp)
		{
			DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(unixTimeStamp);
			DateTime dateTime = dateTimeOffset.DateTime;
			return dateTime;
		}
		public static long DateTimeToUnixTimeStamp(DateTime dateTime)
		{
			return ((DateTimeOffset)dateTime).ToUnixTimeSeconds();
		}
		public static string HexToNpub(string hexString)
		{
			var bytes = HexStringToByteArray(hexString);
			return Bech32Encoder.Encode("npub", bytes);
		}

		public static string NpubToHex(string npubString)
		{
			if (npubString == null || !npubString.Contains("npub"))
			{
				return npubString;
			}
			Bech32Encoder.Decode(npubString, out var hrp, out var bytes);
			if (hrp != "npub" || bytes == null) throw new Exception("Invalid npub string");
			return ByteArrayToHexString(bytes);
		}

		public static string HexToNsec(string hexString)
		{
			var bytes = HexStringToByteArray(hexString);
			return Bech32Encoder.Encode("nsec", bytes);
		}

		public static string NsecToHex(string nsecString)
		{
			Bech32Encoder.Decode(nsecString, out var hrp, out var bytes);
			if (hrp != "nsec" || bytes == null) throw new Exception("Invalid nsec string");
			return ByteArrayToHexString(bytes);
		}

		private static byte[] HexStringToByteArray(string hex)
		{
			return new byte[hex.Length / 2].Select((val, idx) => Convert.ToByte(hex.Substring(idx * 2, 2), 16)).ToArray();
		}

		private static string ByteArrayToHexString(byte[] bytes)
		{
			return BitConverter.ToString(bytes).Replace("-", "").ToLower();
		}
	}
}
