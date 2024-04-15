using BlazeJump.Common.Enums;
using BlazeJump.Common.Models;
using BlazeJump.Common.Services.Connections;
using AutoMapper;
using BlazeJump.Common.Services.Crypto;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using BlazeJump.Common.Models.NostrConnect;
using BlazeJump.Common.Services.UserProfile;

namespace BlazeJump.Common.Services.Message
{
	public class MessageService : IMessageService
	{
		private IRelayManager _relayManager;
		private ICryptoService _cryptoService;
		private IUserProfileService _userProfileService;
		private IMapper _mapper;
		private Dictionary<string, NEvent> sendMessageQueue = new Dictionary<string, NEvent>();
		public List<NEvent> NEvents { get; set; }

		public List<string> Users { get; set; } = new List<string>();

		public MessageService(IRelayManager relayManager, ICryptoService cryptoService, IUserProfileService userProfileService, IMapper mapper)
		{
			_relayManager = relayManager;
			_cryptoService = cryptoService;
			_userProfileService = userProfileService;
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
		private bool SignNEvent(ref NEvent nEvent)
		{
			if(_cryptoService.EtherealPublicKey == null)
			{
				_cryptoService.CreateEtherealKeyPair();
			}
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
		public async Task SendNEvent(KindEnum kind, string message)
		{
			var nEvent = await GetNewNEvent(kind, message);
			var subscriptionHash = Guid.NewGuid().ToString();
			SignNEvent(ref nEvent);
			await _relayManager.SendNEvent(nEvent, new List<string> { "wss://relay.damus.io" }, subscriptionHash);
			sendMessageQueue.TryAdd(nEvent.Id, nEvent);
		}
		private async Task<NEvent> GetNewNEvent(KindEnum kind, string message, string? parentId = null)
		{
			return new NEvent
			{
				Id = "0",
				Pubkey = _userProfileService.NPubKey,
				Kind = kind,
				Content = message,
				ParentNEventId = null,
				Created_At = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
			};
		}
	}
}
