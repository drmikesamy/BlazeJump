using BlazeJump.Common.Enums;
using BlazeJump.Common.Models;
using BlazeJump.Common.Services.Connections.Events;
using System.Collections.Concurrent;

namespace BlazeJump.Common.Services.Message
{
	public interface IMessageService
	{
		Task LookupUser(string searchString);
		Task FetchPage(string hex, DateTime? untilMarker = null);
		RelationRegister RelationRegister { get; set; }
		ConcurrentDictionary<string, NMessage> MessageStore { get; set; }
		Task Fetch(List<Filter> filters, string? subscriptionId = null, MessageTypeEnum? messageType = null);
		bool Verify(NEvent nEvent);
		Task Send(KindEnum kind, NEvent nEvent, string encryptPubKey = null);
		NEvent CreateNEvent(KindEnum kind, string message, string parentId = null, string rootId = null, List<string> ptags = null);
	}
}