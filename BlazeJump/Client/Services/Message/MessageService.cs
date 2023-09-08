using BlazeJump.Client.Enums;
using BlazeJump.Client.Models;
using BlazeJump.Client.Services.Connections;
using BlazeJump.Client.Services.Database;
using AutoMapper;
using BlazeJump.Client.Services.Crypto;
using System.Linq;

namespace BlazeJump.Client.Services.Message
{
	public class MessageService : IMessageService
	{
		private IRelayManager _relayManager;
		private IBlazeDbService _dbService;
		private ICryptoService _cryptoService;
		private IMapper _mapper;
		private Dictionary<string, List<NMessage>> activeSubscriptions = new Dictionary<string, List<NMessage>>();
		private Dictionary<string, NEvent> sendMessageQueue = new Dictionary<string, NEvent>();
		public event EventHandler<NMessage>? NewMessageProcessed;
		public bool FetchTaskComplete = false;
		public List<string> Users { get; set; } = new List<string>
		{

		};

		public MessageService(IBlazeDbService dbService, IRelayManager relayManager, ICryptoService cryptoService, IMapper mapper)
		{
			_dbService = dbService;
			_relayManager = relayManager;
			_cryptoService = cryptoService;
			_mapper = mapper;
			_relayManager.NewMessageReceived += AddMessageToBuffer;
		}

		public async Task FetchMessagesByFilter(Filter filter, int parentDepth = 1)
		{
			var subscriptionHash = Guid.NewGuid().ToString();
			activeSubscriptions.Add(subscriptionHash, new List<NMessage>());
			await _relayManager.QueryRelays(new List<string> { "wss://relay.damus.io" }, subscriptionHash, filter);
			while (!FetchTaskComplete)
			{
				await Task.Delay(25);
			}

			if (parentDepth > 0)
			{
				FetchTaskComplete = false;
				var allTags = activeSubscriptions[subscriptionHash].Where(m => m.Event?.Kind == KindEnum.Text).Select(m => m.Event.Tags.Select(t => new Tuple<TagEnum, string?>(t.Key, t.Value))).SelectMany(tu => tu.Select(tf => tf)).Distinct().ToList();
				var parentEventIds = allTags.Where(t => t.Item1 == TagEnum.e && t.Item2 != null && t.Item2.Count() == 64).Select(t => t.Item2).ToList();

				var since = DateTime.UtcNow.AddYears(-20);
				var until = DateTime.UtcNow;

				if (parentEventIds != null && parentEventIds.Count() > 0)
				{
					parentDepth--;
					await FetchMessagesByFilter(new Filter
					{
						Ids = parentEventIds,
						Since = since,
						Until = until,
					}, parentDepth);
				}
			}
			ProcessMessages();
		}

		private async void AddMessageToBuffer(object sender, NMessage newMessage)
		{
			if (newMessage.MessageType == MessageTypeEnum.Eose || (newMessage.NoticeMessage != null && newMessage.NoticeMessage.Contains("ERROR")))
			{
				FetchTaskComplete = true;
			}
			else
			{
				if (!String.IsNullOrEmpty(newMessage.SubscriptionId) && activeSubscriptions.ContainsKey(newMessage.SubscriptionId))
				{
					activeSubscriptions[newMessage.SubscriptionId].Add(newMessage);
				}

			}
		}

		private async void ProcessMessages()
		{
			foreach (var subscription in activeSubscriptions)
			{
				foreach (var textMessage in subscription.Value)
				{
					AddMessage(textMessage);
				}
			}
			activeSubscriptions.Clear();
		}
		public User GetOrCreateUser(string userId)
		{
			User? user = _dbService.Context.Set<User>().SingleOrDefault(u => u.Id == userId);
			if (user == null)
			{
				var random = new Random();
				var randomNumberForProfileImg = random.Next(1, 18);
				user = new User() { Id = userId, ProfilePic = $"{randomNumberForProfileImg}" };
				_dbService.Context.Add(user);
			}
			return user;
		}
		public NEvent? GetOrCreateNEvent(string nEventId)
		{
			if (nEventId == null)
				return null;

			var nEvent = _dbService.Context.Set<NEvent>().SingleOrDefault(e => e.Id == nEventId);
			if (nEvent == null)
			{
				nEvent = new NEvent
				{
					Id = nEventId,
				};
				_dbService.Context.Set<NEvent>().Add(nEvent);
			}
			return nEvent;
		}
		public async void AddMessage(NMessage nMessage)
		{
			var nEvent = nMessage.Event;


			var user = GetOrCreateUser(nEvent.Pubkey);
			//Get or create parent
			var parentNEventEntity = GetOrCreateNEvent(nEvent.ParentNEventId);
			//Get or create current
			var nEventEntity = GetOrCreateNEvent(nEvent.Id);

			_mapper.Map(nEvent, nEventEntity);

			if (nEventEntity.User == null)
			{
				nEventEntity.User = user;
				nEventEntity.ParentNEvent = parentNEventEntity;
			}
			else
			{
				_dbService.Context.Set<NEvent>().Update(nEventEntity);
			}

			_dbService.Context.SaveChanges();

			nMessage.Event = nEventEntity;
			NewMessageProcessed?.Invoke(this, nMessage);
		}

		public async Task SendNEvent(NEvent nEvent, string subscriptionHash)
		{
			var signedNEvent = await _cryptoService.SignEvent(nEvent);
			await _relayManager.SendNEvent(signedNEvent, new List<string> { "wss://relay.damus.io" }, subscriptionHash);
			sendMessageQueue.TryAdd(signedNEvent.Id, signedNEvent);
		}

		public async Task<NEvent> GetNewNEvent(KindEnum kind, string message, string? parentId = null)
		{
			return new NEvent
			{
				Id = "0",
				Pubkey = await _cryptoService.GetPublicKey(),
				Kind = kind,
				Content = message,
				ParentNEventId = null,
				Created_At = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
			};
		}
	}
}
