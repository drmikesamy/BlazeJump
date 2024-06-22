using BlazeJump.Common.Enums;
using Newtonsoft.Json;

namespace BlazeJump.Common.Models
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
		public Stats? Stats { get; set; }
		public MessageContextEnum Context { get; set; }
	}

}