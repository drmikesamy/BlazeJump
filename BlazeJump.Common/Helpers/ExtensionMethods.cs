using Newtonsoft.Json;

namespace BlazeJump.Helpers
{
	public static class ExtensionMethods
	{
		public static T Clone<T>(this T source)
		{
			var serialized = JsonConvert.SerializeObject(source);
			return JsonConvert.DeserializeObject<T>(serialized);
		}
	}
}
