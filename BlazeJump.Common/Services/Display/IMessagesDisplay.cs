using BlazeJump.Common.Enums;
using BlazeJump.Common.Models;
using BlazeJump.Common.Services.Message;
using Newtonsoft.Json.Linq;
using BlazeJump.Common.Services.Connections.Events;

namespace BlazeJump.Common.Services.Display
{
	public interface IMessagesDisplay
	{
		Dictionary<string, List<NMessage>> Replies { get; set; }
		PageTypeEnum? PageType { get; set; }
		string Hex { get; set; }
		Dictionary<string, List<NMessage>> MessageBuckets { get; set; }
		Dictionary<string, User> Users { get; set; }
		List<Filter> Filters { get; set; }
		event EventHandler StateUpdated;
		Task Init(PageTypeEnum pageType, string hex);
		Task LoadMore();
	}
}
