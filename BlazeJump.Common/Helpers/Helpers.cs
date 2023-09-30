namespace BlazeJump.Common.Models
{
	public static class Helpers
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
	}
}
