using BlazeJump.Client.Enums;
using Newtonsoft.Json;

namespace BlazeJump.Client.Models
{
	[JsonConverter(typeof(MessageConverter))]
	public class NMessage
	{
		public MessageTypeEnum MessageType { get; set; }
		public string? SubscriptionId { get; set; }
		public NEvent? Event { get; set; }
		public Filter? Filter { get; set; }
		public string? NoticeMessage { get; set; }
		public bool? IsEventDuplicate { get; set; }
		public bool? Success { get; set; }
		public string? NEventId { get; set; }
	}

}