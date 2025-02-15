
using BlazeJump.Common.Enums;
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
		public static string HexToBech32(string hexString, Bech32PrefixEnum bechLabel)
		{
			var bytes = HexStringToByteArray(hexString);
			return Bech32Encoder.Encode(bechLabel.ToString(), bytes);
		}

		public static string Bech32ToHex(string bech32string, Bech32PrefixEnum bechLabel)
		{
			if (bech32string == null || !bech32string.Contains(bechLabel.ToString()))
			{
				return bech32string;
			}
			Bech32Encoder.Decode(bech32string, out var hrp, out var bytes);
			if (hrp != bechLabel.ToString() || bytes == null) throw new Exception("Invalid npub string");
			return ByteArrayToHexString(bytes);
		}

		public static Dictionary<TLVTypeEnum, string> Bech32ToTLVComponents(string bech32string, Bech32PrefixEnum bechLabel)
		{
			if (bech32string == null || !bech32string.Contains(bechLabel.ToString()))
			{
				return new Dictionary<TLVTypeEnum, string>();
			}
			Bech32Encoder.Decode(bech32string, out var hrp, out var bytes);
			if (hrp != bechLabel.ToString() || bytes == null) throw new Exception("Invalid npub string");
			var cursor = 0;
			var tlvDictionary = new Dictionary<TLVTypeEnum, string>();
			while (cursor < bytes.Length)
			{
				TLVTypeEnum type = (TLVTypeEnum)bytes[cursor++];
				int length = bytes[cursor++];
				byte[] value = new byte[length];
				Array.Copy(bytes, cursor, value, 0, length);
				cursor += length;
				tlvDictionary.Add(type, ByteArrayToHexString(value));
			}
			return tlvDictionary;
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
