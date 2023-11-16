using BlazeJump.Common.Enums;
using BlazeJump.Common.Models;
using BlazeJump.Common.Services.Connections;
using BlazeJump.Common.Services.Database;
using AutoMapper;
using BlazeJump.Common.Services.Crypto;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using BlazeJump.Common.Models.NostrConnect;
using BlazeJump.Common.Services.UserProfile;
using BlazeJump.Common.Models.Crypto;
using System.Reflection.Metadata;

namespace BlazeJump.Common.Services.Message
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
			var rawMessages = await _relayManager.QueryRelays(new List<string> { "wss://relay.damus.io" }, subscriptionHash, filter, 15000);
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
		public bool SignNEvent(ref NEvent nEvent)
		{
#if ANDROID
			_cryptoService.GetUserKeyPair();
#else
			_cryptoService.GenerateKeyPair();
#endif
			var signableEvent = nEvent.GetSignableNEvent();
			var serialisedNEvent = JsonConvert.SerializeObject(signableEvent);
			var signature = _cryptoService.Sign(serialisedNEvent);
			nEvent.Sig = signature;
			return true;
		}
		public bool VerifyNEvent(NEvent nEvent)
		{
			var signableEvent = nEvent.GetSignableNEvent();
			var serialisedNEvent = JsonConvert.SerializeObject(signableEvent);
			var verified = _cryptoService.Verify(nEvent.Sig, serialisedNEvent, nEvent.Pubkey);
			return verified;
		}
#if ANDROID
		public async Task SendNEvent(NEvent nEvent)
		{
			var subscriptionHash = Guid.NewGuid().ToString();
			SignNEvent(ref nEvent);
			await _relayManager.SendNEvent(nEvent, new List<string> { "wss://relay.damus.io" }, subscriptionHash);
			sendMessageQueue.TryAdd(nEvent.Id, nEvent);
		}
		public async Task SendNostrConnectReply(string theirPubKey)
		{
			var subscriptionHash = Guid.NewGuid().ToString();
			var message = new NostrConnectResponse
			{
				Id = subscriptionHash,
				Result = "Connected"
			};
			var nEvent = await GetNewNEvent(KindEnum.NostrConnect, JsonConvert.SerializeObject(message));
			SignNEvent(ref nEvent);
			await _relayManager.SendNEvent(nEvent, new List<string> { "wss://relay.damus.io" }, subscriptionHash);
			sendMessageQueue.TryAdd(nEvent.Id, nEvent);
		}
		public async Task<NEvent> GetNewNEvent(KindEnum kind, string message, string? parentId = null)
		{
			return new NEvent
			{
				Id = "0",
				Pubkey = _cryptoService.PublicKey,
				Kind = kind,
				Content = message,
				ParentNEventId = null,
				Created_At = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
			};
		}
#endif
	}
}
