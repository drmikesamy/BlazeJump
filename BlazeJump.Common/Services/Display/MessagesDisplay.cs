using BlazeJump.Common.Enums;
using BlazeJump.Common.Models;
using BlazeJump.Common.Services.Message;
using Newtonsoft.Json.Linq;
using BlazeJump.Common.Services.Connections.Events;
using System;

namespace BlazeJump.Common.Services.Display
{
	public class MessagesDisplay : IMessagesDisplay
	{

		private readonly IMessageService _messageService;
		public PageTypeEnum? PageType { get; set; }
		public string Hex { get; set; }
		public Dictionary<string, List<NMessage>> MessageBuckets { get; set; } = new();
		public Dictionary<string, User> Users { get; set; } = new();
		public List<Filter> Filters { get; set; } = new List<Filter>();
		public event EventHandler StateUpdated;
		public MessagesDisplay(IMessageService messageService)
		{

			_messageService = messageService;
			_messageService.NewMessageReceived += ProcessMessage;
		}
		public async Task Init(PageTypeEnum pageType, string hex)
		{
			PageType = pageType;
			Hex = hex;
			if (PageType == PageTypeEnum.Event)
			{
				Filters.Add(new Filter
				{
					Kinds = new int[] { (int)KindEnum.Text },
					Since = DateTime.Now.AddYears(-20),
					Until = DateTime.Now.AddDays(1),
					Limit = 1,
					EventIds = new List<string> { Hex },
				});
			}
			await LoadMessages();
		}
		public async Task LoadMessages()
		{
			if (MessageBuckets.Any())
			{
				Filters.Clear();
			}
			Filters.Add(new Filter
			{
				Kinds = new int[] { (int)KindEnum.Text },
				Since = DateTime.Now.AddYears(-20),
				Until = MessageBuckets.Any() ? MessageBuckets.Last().Value.LastOrDefault()?.Event.CreatedAtDateTime.AddMilliseconds(-1) ?? DateTime.Now.AddDays(1) : DateTime.Now.AddDays(1),
				Limit = 5,
				TaggedEventIds = PageType == PageTypeEnum.Event ? new List<string> { Hex } : null,
				Authors = PageType == PageTypeEnum.Event ? null : new List<string> { Hex }
			});
			var fetchId = Guid.NewGuid().ToString();
			MessageBuckets.Add(fetchId, new List<NMessage>());
			await _messageService.FetchNEventsByFilter(MessageTypeEnum.Req, Filters, $"{MessageContextEnum.Event}_{fetchId}");
		}
		private async Task LoadUsers(string subId)
		{
			MessageBuckets.TryGetValue(subId, out var messages);
			if (messages.Count() == 0)
				return;
			var usersToLoad = messages.Select(m => m.Event).Select(e => e.Pubkey).Except(Users.Keys);
			if (usersToLoad.Any())
			{
				var userFilter = new Filter
				{
					Kinds = new int[] { (int)KindEnum.Metadata },
					Since = DateTime.Now.AddYears(-20),
					Until = DateTime.Now.AddDays(1),
					Authors = usersToLoad.ToList()
				};
				await _messageService.FetchNEventsByFilter(MessageTypeEnum.Req, new List<Filter> { userFilter }, $"{MessageContextEnum.User}_{subId}");
			}
		}
		private async Task LoadReplies(string subId)
		{
			MessageBuckets.TryGetValue(subId, out var messages);
			if (messages.Count() == 0)
				return;
			var nEventIds = messages.Where(m => !m.Event.RepliesLoaded).Select(m => m.Event.Id);
			var filters = new List<Filter>();
			foreach (var parentEventId in nEventIds)
			{
				filters.Add(new Filter
				{
					Kinds = new int[] { (int)KindEnum.Text },
					Since = messages.Last().Event.CreatedAtDateTime,
					Until = DateTime.Now.AddDays(1),
					TaggedEventIds = new List<string> { parentEventId }
				});
			}
			await _messageService.FetchNEventsByFilter(MessageTypeEnum.Req, filters, $"{MessageContextEnum.Reply}_{subId}");
		}
		private void ProcessMessage(object sender, MessageReceivedEventArgs e)
		{
			switch (e.Message.MessageType)
			{
				case MessageTypeEnum.Event:
					switch (e.Message.Context)
					{
						case MessageContextEnum.Event:
							MessageBuckets.TryGetValue(e.Message.SubscriptionId, out var messages);
							messages.Add(e.Message);
							break;
						case MessageContextEnum.Reply:
							ProcessReply(e.Message);
							break;
						case MessageContextEnum.User:
							ProcessUser(e.Message);
							break;
					}
					break;
				case MessageTypeEnum.Eose:
					_ = LoadUsers(e.Message.SubscriptionId);
					_ = LoadReplies(e.Message.SubscriptionId);
					break;
			}
			StateUpdated.Invoke(this, EventArgs.Empty);		
		}
		private void ProcessReply(NMessage message)
		{
			MessageBuckets.TryGetValue(message.SubscriptionId, out var messages);
			if (messages.Count() == 0)
				return;
			var parent = messages.Where(m => message.Event.Tags.Select(t => t.Value).Contains(m.Event.Id)).Single();
			parent.Event.Replies.Add(message.Event);
			parent.Event.RepliesLoaded = true;
		}
		private void ProcessUser(NMessage message)
		{
			try
			{
				if (Users.ContainsKey(message.Event.Pubkey))
					return;
				var parsed = JObject.Parse(message.Event.Content);
				var user = new User
				{
					Id = message.Event.Pubkey,
					Username = parsed["name"]?.ToString(),
					Bio = parsed["about"]?.ToString(),
					ProfilePic = parsed["picture"]?.ToString(),
					Banner = parsed["banner"]?.ToString(),
				};
				Users.Add(user.Id, user);
			}
			catch
			{

			}
		}
	}
}
