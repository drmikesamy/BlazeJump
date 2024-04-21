using BlazeJump.Common.Models;

namespace BlazeJump.Common.Services.Connections.Events
{
	public class MessageReceivedEventArgs
	{
		public MessageReceivedEventArgs(string url, NMessage message)
		{
			Url = url;
			Message = message;
		}
		public string Url { get; set; }
		public NMessage Message { get; set; }
	}
}