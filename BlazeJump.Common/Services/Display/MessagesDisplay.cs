using BlazeJump.Common.Enums;
using BlazeJump.Common.Models;
using BlazeJump.Common.Services.Message;
using Newtonsoft.Json.Linq;
using BlazeJump.Common.Services.Connections.Events;
using System;
using System.Diagnostics;

namespace BlazeJump.Common.Services.Display
{
	public class MessagesDisplay : IMessagesDisplay
	{

		private readonly IMessageService _messageService;
		public PageTypeEnum? PageType { get; set; }
		public string Hex { get; set; }
		public Dictionary<string, List<NMessage>> MessageBuckets { get; set; } = new();
		public Dictionary<string, User> Users { get; set; } = new();
		public Dictionary<string, List<NMessage>> Replies { get; set; } = new();
		public List<Filter> Filters { get; set; } = new List<Filter>();
		private PriorityQueue<Tuple<MessageContextEnum, string>, Tuple<int, long>> _loadQueue { get; set; } = new();
		public event EventHandler StateUpdated;
		public MessagesDisplay(IMessageService messageService)
		{
			_messageService = messageService;
			_messageService.MessageReceived += ProcessMessage;
			_ = ProcessLoadQueue();
		}
		public async Task Init(PageTypeEnum pageType, string hex)
		{
			PageType = pageType;
			Hex = hex;

			Filters.Clear();
			SetFilters();
			await FetchPosts(true);
		}
		public async Task LoadMore()
		{
			Filters.Clear();
			SetBodyFilter();
			await FetchPosts(true);
		}
		private void SetFilters()
		{
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
			SetBodyFilter();
		}
		private void SetBodyFilter()
		{
			Filters.Add(new Filter
			{
				Kinds = new int[] { (int)KindEnum.Text },
				Since = DateTime.Now.AddYears(-20),
				Until = MessageBuckets.Any() ? MessageBuckets.Last().Value.LastOrDefault()?.Event.CreatedAtDateTime.AddMilliseconds(-1) ?? DateTime.Now.AddDays(1) : DateTime.Now.AddDays(1),
				Limit = 5,
				TaggedEventIds = PageType == PageTypeEnum.Event ? new List<string> { Hex } : null,
				Authors = PageType == PageTypeEnum.Event ? null : new List<string> { Hex }
			});
		}
		private async Task FetchPosts(bool isPrimaryPost)
		{
			if (Filters.All(f => f.TaggedEventIds?.Count() > 0 || f.Authors?.Count() > 0 || f.EventIds.Count() > 0))
			{
				var fetchId = Guid.NewGuid().ToString();
				if (isPrimaryPost)
					MessageBuckets.Add(fetchId, new List<NMessage>());
				await _messageService.Fetch(MessageTypeEnum.Req, Filters, fetchId);
			}
		}
		private void SetupUserFilter(List<string> pubkeys)
		{
			Filters.Add(new Filter
			{
				Kinds = new int[] { (int)KindEnum.Metadata },
				Since = DateTime.Now.AddYears(-20),
				Until = DateTime.Now.AddDays(1),
				Authors = pubkeys
			});
		}
		private void SetupReplyFilter(List<string> parentEventIds)
		{
			Filters.Add(new Filter
			{
				Kinds = new int[] { (int)KindEnum.Text },
				Since = DateTime.Now.AddYears(-20),
				Until = DateTime.Now.AddDays(1),
				TaggedEventIds = parentEventIds
			});
		}
		private void ProcessMessage(object sender, MessageReceivedEventArgs e)
		{
			switch (e.Message.MessageType)
			{
				case MessageTypeEnum.Event:
					switch (e.Message.Event.Kind)
					{
						case KindEnum.Text:
							if (MessageBuckets.TryGetValue(e.Message.SubscriptionId, out var messages))
							{
								messages.Add(e.Message);
								if (!Users.TryGetValue(e.Message.Event.UserId, out var pubkey))
								{
									_loadQueue.Enqueue(new Tuple<MessageContextEnum, string>(MessageContextEnum.User, e.Message.Event.UserId), new Tuple<int, long>(0, Stopwatch.GetTimestamp()));
								}
								if (MessageBuckets.TryGetValue(e.Message.SubscriptionId, out var message))
								{
									_loadQueue.Enqueue(new Tuple<MessageContextEnum, string>(MessageContextEnum.Reply, e.Message.Event.Id), new Tuple<int, long>(1, Stopwatch.GetTimestamp()));
								}
							}
							else
							{
								ProcessReply(e.Message);
							}
							break;
						case KindEnum.Metadata:
							ProcessUser(e.Message);
							break;
					}
					break;
				case MessageTypeEnum.Eose:
					_ = ProcessLoadQueue();
					break;
			}
			StateUpdated.Invoke(this, EventArgs.Empty);
		}
		private async Task ProcessLoadQueue()
		{
			await Task.Delay(3000);
			var pubkeys = new List<string>();
			var parentEventIds = new List<string>();
			while (_loadQueue.Count > 0)
			{
				var (context, id) = _loadQueue.Dequeue();
				switch (context)
				{
					case MessageContextEnum.User:
						pubkeys.Add(id);
						break;
					case MessageContextEnum.Reply:
						parentEventIds.Add(id);
						break;
				}
			}
			Filters.Clear();
			SetupUserFilter(pubkeys);
			SetupReplyFilter(parentEventIds);
			await FetchPosts(false);
		}
		private void ProcessReply(NMessage message)
		{
			if (MessageBuckets.ContainsKey(message.SubscriptionId))
				return;
			var parentEventId = message.Event.Tags.LastOrDefault(t => t.Key == TagEnum.e && t.Value3 == "root")?.Value;
			if (parentEventId == null)
				return;
			Replies.TryGetValue(parentEventId, out var replies);
			if (replies == null)
			{
				replies = new List<NMessage>();
				Replies.Add(parentEventId, replies);
			}
			else
			{
				replies.Add(message);
			}
			StateUpdated.Invoke(this, EventArgs.Empty);
		}
		private void ProcessUser(NMessage message)
		{
			try
			{
				if (Users.ContainsKey(message.Event.UserId))
					return;
				Users.Add(message.Event.UserId, message.Event.User);
			}
			catch
			{

			}
			StateUpdated.Invoke(this, EventArgs.Empty);
		}
	}
}
