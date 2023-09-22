using BlazeJump.Client.Enums;
using BlazeJump.Client.Models;
using BlazeJump.Client.Services.Connections;
using BlazeJump.Client.Services.Database;
using AutoMapper;
using BlazeJump.Client.Services.Crypto;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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

		public async Task<List<NEvent>> FetchNEventsByFilter(Filter filter, bool fetchStats = false, bool fullFetch = false)
		{
			var subscriptionHash = Guid.NewGuid().ToString();
			var rawMessages = await _relayManager.QueryRelays(new List<string> { "wss://relay.damus.io" }, subscriptionHash, filter);
			var nMessages = rawMessages.Select(rawMessage => JsonConvert.DeserializeObject<NMessage>(rawMessage));
			var nEvents = nMessages.Where(m => m?.MessageType == MessageTypeEnum.Event).Select(m => m?.Event).ToList();

			if (fullFetch)
			{
				foreach (var nEvent in nEvents)
				{
					filter.EventId = new List<string> { nEvent.Id };
					filter.Ids = null;
					nEvent.ChildNEvents = await FetchNEventsByFilter(filter, true);
				}
			}
			foreach (var nEvent in nEvents)
			{
				nEvent.ReplyCount = await FetchStats(nEvent.Id);
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
		public async Task<List<User>> FetchProfiles(List<string> pubKeys)
		{
			var filter = new Filter
			{
				Kinds = new int[] { (int)KindEnum.Metadata },
				Since = DateTime.Now.AddYears(-20),
				Until = DateTime.Now,
				Authors = pubKeys.Distinct().ToList()
			};
			var events = await FetchNEventsByFilter(filter, false, false);
			var metaData = events.Where(e => e.Kind == KindEnum.Metadata && pubKeys.Contains(e.Pubkey));
			var profiles = new List<User>();
			foreach (var m in metaData)
			{
				if (!string.IsNullOrEmpty(m.Content))
				{
					try
					{
						var parsed = JObject.Parse(m.Content);
						var user = new User
						{
							Id = m.Pubkey,
							Username = parsed["name"]?.ToString(),
							Bio = parsed["about"]?.ToString(),
							ProfilePic = parsed["picture"]?.ToString(),
							Banner = parsed["banner"]?.ToString(),
						};
						profiles.Add(user);
					}
					catch
					{

					}
				}
			}
			return profiles;
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
