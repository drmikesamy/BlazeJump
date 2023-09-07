using BlazeJump.Client.Enums;
using BlazeJump.Client.Models;
using BlazeJump.Client.Services.Connections;
using BlazeJump.Client.Services.Database;
using AutoMapper;
using BlazeJump.Client.Services.Crypto;

namespace BlazeJump.Client.Services.Message
{
    public class MessageService : IMessageService
    {
        private IRelayManager _relayManager;
        private IBlazeDbService _dbService;
        private ICryptoService _cryptoService;
        private IMapper _mapper;
        private Dictionary<string, bool> activeSubscriptions = new Dictionary<string, bool>();
        private Dictionary<string, NEvent> sendMessageQueue = new Dictionary<string, NEvent>();
        private List<string> _relatedMessageIds = new List<string>();
        public event EventHandler<NMessage>? NewMessageProcessed;
        public bool FetchTaskComplete = false;
        public List<string> Users { get; set; } = new List<string>
        {

        };
        private int FetchCount { get; set; } = 0;

        public MessageService(IBlazeDbService dbService, IRelayManager relayManager, ICryptoService cryptoService, IMapper mapper)
        {
            _dbService = dbService;
            _relayManager = relayManager;
            _cryptoService = cryptoService;
            _mapper = mapper;
            _relayManager.NewMessageReceived += ProcessMessage;
        }

        public List<NEvent> GetAllMessages()
        {
            var events = _dbService.Context.Set<NEvent>().ToList();
            Console.WriteLine(events?.Count().ToString());
            return events;
        }

        public async Task FetchMessagesByFilter(Filter filter)
        {
            var subscriptionHash = Guid.NewGuid().ToString();
            activeSubscriptions.TryAdd(subscriptionHash, true);
            await _relayManager.QueryRelays(new List<string> { "wss://relay.damus.io" }, subscriptionHash, filter);
            FetchCount++;
            while (!FetchTaskComplete)
            {
                await Task.Delay(25);
            }
        }

        private async void ProcessMessage(object sender, NMessage newMessage)
        {
            switch (newMessage.MessageType)
            {
                case MessageTypeEnum.Event:
                    if (newMessage.Event != null && newMessage.Event.Kind == KindEnum.Text)
                        if (newMessage.Event.Pubkey != null)
                        {
                            var relatedMessageIds = newMessage.Event.Tags.Where(e => e.Key == TagEnum.e && e.Value?.Count() == 64).Select(e => e.Value ?? "").ToList();
                            _relatedMessageIds.AddRange(relatedMessageIds);
                            AddMessage(newMessage);
                        }
                    break;
                case MessageTypeEnum.Notice:
                    if (newMessage.NoticeMessage != null)
                    {
						Console.WriteLine(newMessage.NoticeMessage);
                        if (newMessage.NoticeMessage.Contains("ERROR"))
                        {
							FetchTaskComplete = true;
						}
					}  
                    break;
                case MessageTypeEnum.Eose:
                    Console.WriteLine($"End of stored events: {newMessage.SubscriptionId}");
                    if (_relatedMessageIds != null && _relatedMessageIds.Count() > 0 && FetchCount <= 1)
                    {
                        var since = DateTime.UtcNow.AddYears(-20);
                        var until = DateTime.UtcNow;

                        await FetchMessagesByFilter(new Filter
                        {
                            Ids = _relatedMessageIds,
                            Since = since,
                            Until = until,
                        });

                        _relatedMessageIds.Clear();
                    }
                    else
                    {
						FetchTaskComplete = true;
					}
                    break;
                case MessageTypeEnum Ok:
                    Console.WriteLine($"Event received OK. Event Id {newMessage.NEventId}");
                    sendMessageQueue.TryGetValue(newMessage.NEventId, out var nEvent);
                    newMessage.Event = nEvent;
					AddMessage(newMessage);
                    break;
            }
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
        public void AddMessage(NMessage nMessage)
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
            return new NEvent { 
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
