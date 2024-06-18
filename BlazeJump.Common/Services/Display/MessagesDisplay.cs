using BlazeJump.Common.Enums;
using BlazeJump.Common.Models;
using BlazeJump.Common.Services.Message;
using Newtonsoft.Json.Linq;
using BlazeJump.Common.Services.Connections.Events;

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
		public event EventHandler<MessageReceivedEventArgs> NewMessageReceived;
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
				Until = MessageBuckets.Any() ? MessageBuckets.Last().Value.Last().Event.CreatedAtDateTime.AddMilliseconds(-1) : DateTime.Now.AddDays(1),
				Limit = 5,
				TaggedEventIds = PageType == PageTypeEnum.Event ? new List<string> { Hex } : null,
				Authors = PageType == PageTypeEnum.Event ? null : new List<string> { Hex }
			});
			var fetchId = Guid.NewGuid().ToString();
			MessageBuckets.Add(fetchId, new List<NMessage>());
			await _messageService.FetchNEventsByFilter(MessageTypeEnum.Req, Filters, fetchId);
		}
		private void LoadUsers()
		{
			var usersToLoad = MessageBuckets.Last().Value.Select(m => m.Event).Select(e => e.Pubkey);
			if (usersToLoad.Any())
			{
				var userFilter = new Filter
				{
					Kinds = new int[] { (int)KindEnum.Metadata },
					Since = DateTime.Now.AddYears(-20),
					Until = DateTime.Now.AddDays(1),
					Authors = usersToLoad.ToList()
				};
				_ = _messageService.FetchNEventsByFilter(MessageTypeEnum.Req, new List<Filter> { userFilter }, $"{TemplateAreaEnum.User}_{MessageBuckets.Last().Key}");
			}
		}
		private void LoadReplies()
		{
			var nEventIds = MessageBuckets.Last().Value.Where(m => !m.Event.RepliesLoaded).Select(m => m.Event.Id);

			var filters = new List<Filter>();
			foreach (var parentEventId in nEventIds)
			{
				filters.Add(new Filter
				{
					Kinds = new int[] { (int)KindEnum.Text },
					Since = MessageBuckets.Last().Value.Last().Event.CreatedAtDateTime,
					Until = DateTime.Now.AddDays(1),
					TaggedEventIds = new List<string> { parentEventId }
				});
			}
			if (filters.Count > 0)
			{
				_ = _messageService.FetchNEventsByFilter(MessageTypeEnum.Req, filters, $"{TemplateAreaEnum.Replies}_{MessageBuckets.Last().Key}");
			}
		}
		private void ProcessMessage(object sender, MessageReceivedEventArgs e)
		{
			if (e.Message.MessageType == MessageTypeEnum.Eose
				&& !e.Message.SubscriptionId.StartsWith($"{TemplateAreaEnum.User}_"))
			{
				LoadUsers();
				LoadReplies();
				return;
			}
			else if (e.Message.SubscriptionId.StartsWith($"{TemplateAreaEnum.User}_"))
			{
				ProcessUser(e.Message);
			}
			else if (e.Message.SubscriptionId.StartsWith($"{TemplateAreaEnum.Replies}_"))
			{
				ProcessReply(e.Message);
			}
			else if (e.Message.MessageType == MessageTypeEnum.Event)
			{
				MessageBuckets.Last().Value.Add(e.Message);
			}
			StateUpdated.Invoke(this, EventArgs.Empty);
		}
		private void ProcessReply(NMessage message)
		{
			var parent = MessageBuckets.Last().Value.Where(m => message.Event.Tags.Select(t => t.Value).Contains(m.Event.Id)).Single();
			parent.Event.Replies.Add(message.Event);
			parent.Event.RepliesLoaded = true;
		}
		private void ProcessUser(NMessage message)
		{
			try
			{
				var parsed = JObject.Parse(message.Event.Content);
				var user = new User
				{
					Id = message.Event.Pubkey,
					Username = parsed["name"]?.ToString(),
					Bio = parsed["about"]?.ToString(),
					ProfilePic = parsed["picture"]?.ToString(),
					Banner = parsed["banner"]?.ToString(),
				};
				if (Users.ContainsKey(user.Id))
				{
					Users[user.Id] = user;
				}
				else
				{
					Users.Add(user.Id, user);
				}

			}
			catch
			{

			}
		}
	}
}
