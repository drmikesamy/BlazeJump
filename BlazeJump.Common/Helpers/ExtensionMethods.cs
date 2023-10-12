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
				return BitConverter.ToString(inputBytes).Replace("-", "");
		}
		public static byte[] SHA256Hash(this string inputString)
		{
			using (SHA256 sha256 = SHA256.Create())
			{
				return sha256.ComputeHash(Encoding.UTF8.GetBytes(inputString));
			}
		}
	}
}
