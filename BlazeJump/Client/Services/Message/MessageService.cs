using BlazeJump.Client.Enums;
using BlazeJump.Client.Models;
using BlazeJump.Client.Services.Connections;
using BlazeJump.Client.Services.Database;
using AutoMapper;
using BlazeJump.Client.Services.Crypto;
using System.Linq;
using Newtonsoft.Json;

namespace BlazeJump.Client.Services.Message
{
	public class MessageService : IMessageService
	{
		private IRelayManager _relayManager;
		private IBlazeDbService _dbService;
		private ICryptoService _cryptoService;
		private IMapper _mapper;
		private Dictionary<string, NEvent> sendMessageQueue = new Dictionary<string, NEvent>();
		public List<NEvent> NEvents { get; set; }

		public List<string> Users { get; set; } = new List<string>();

		public MessageService(IBlazeDbService dbService, IRelayManager relayManager, ICryptoService cryptoService, IMapper mapper)
		{
			_dbService = dbService;
			_relayManager = relayManager;
			_cryptoService = cryptoService;
			_mapper = mapper;
		}

		public async Task<List<NEvent>> FetchNEventsByFilter(Filter filter, bool fullFetch = false)
		{
			var subscriptionHash = Guid.NewGuid().ToString();
			var rawMessages = await _relayManager.QueryRelays(new List<string> { "wss://relay.damus.io" }, subscriptionHash, filter);
			var nMessages = rawMessages.Select(rawMessage => JsonConvert.DeserializeObject<NMessage>(rawMessage));
			var nEvents = nMessages.Where(m => m?.MessageType == MessageTypeEnum.Event).Select(m => m?.Event).ToList();
			if(fullFetch)
			{
				foreach (var nEvent in nEvents)
				{
					filter = new Filter
					{
						Kinds = new int[] { (int)KindEnum.Text },
						Since = DateTime.Now.AddYears(-20),
						Until = DateTime.Now,
						EventId = new List<string> { nEvent.Id }
					};
					nEvent.ChildNEvents = await FetchNEventsByFilter(filter);
					nEvent.ReplyCount = nEvent.ChildNEvents.Count();
				}
			}
			else
			{
				foreach (var nEvent in nEvents)
				{
					nEvent.ReplyCount = await FetchStats(nEvent.Id);
				}
			}
			return nEvents;
		}
		private async Task<int> FetchStats(string nEventId)
		{
			var filter = new Filter
			{
				Kinds = new int[] { (int)KindEnum.Text },
				Since = DateTime.Now.AddYears(-20),
				Until = DateTime.Now,
				EventId = new List<string> { nEventId }
			};
			var subscriptionHash = Guid.NewGuid().ToString();
			var rawMessages = await _relayManager.QueryRelays(new List<string> { "wss://relay.damus.io" }, subscriptionHash, filter);
			return rawMessages.Count();
		}

		public async Task<NEvent?> FetchById(string nEventId)
		{
			var nEvent = (await FetchNEventsByFilter(new Filter
			{
				Since = DateTime.Now.AddYears(-20),
				Until = DateTime.Now,
				Ids = new List<string> { nEventId },
				Limit = 1
			})).FirstOrDefault();

			return nEvent;
		}

		public async Task<List<NEvent>> FetchNEventsByParentId(string nEventId)
		{
			var nEvents = await FetchNEventsByFilter(new Filter
			{
				Since = DateTime.Now.AddYears(-20),
				Until = DateTime.Now,
				EventId = new List<string> { nEventId },
				Limit = 11
			});

			return nEvents;
		}


		private async Task AddMessagesToDb(List<NEvent> rawMessages)
		{
			try
			{
				await _dbService.Context.Events.AddRangeAsync(rawMessages);
			}
			catch (Exception e)
			{
				var logMessageForException = rawMessages == null ? "null" : rawMessages.ToString();
				Console.WriteLine($"Incompatible message type in message: {logMessageForException}");
				Console.WriteLine($"Exception message: {e.Message}");
			}
		}

		public List<NEvent> FetchMessagesFromDb(Func<NEvent, bool> selector)
		{
			return _dbService.Context.Events.Where(selector).ToList();
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
