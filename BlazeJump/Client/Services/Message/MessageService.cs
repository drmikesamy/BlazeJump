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

		public List<string> Users { get; set; } = new List<string>
		{

		};

		public MessageService(IBlazeDbService dbService, IRelayManager relayManager, ICryptoService cryptoService, IMapper mapper)
		{
			_dbService = dbService;
			_relayManager = relayManager;
			_cryptoService = cryptoService;
			_mapper = mapper;
		}

		public async Task<List<NEvent>> FetchMessagesByFilter(Filter filter, int depth = 1, int currentDepth = 0)
		{
			var subscriptionHash = Guid.NewGuid().ToString();
			var rawMessages = await _relayManager.QueryRelays(new List<string> { "wss://relay.damus.io" }, subscriptionHash, filter);
			var nMessages = rawMessages.Select(rawMessage => JsonConvert.DeserializeObject<NMessage>(rawMessage));
			var textMessages = nMessages.Where(m => m?.MessageType == MessageTypeEnum.Event).Select(m => m?.Event).ToList();
			if(currentDepth == 0)
			{
				textMessages.ForEach(t => t.IsBaseMessage = true);
			}
			var parentIds = textMessages.Where(t => t.ParentNEventId != null && t.ParentNEventId.Count() ==  64).Select(m => m.ParentNEventId).ToList();

			
			if(currentDepth < depth)
			{
				currentDepth++;
				var parentMessages = await FetchMessagesByFilter(new Filter
				{
					Since = DateTime.Now.AddYears(-20),
					Until = DateTime.Now,
					EventId = parentIds
				}, depth, currentDepth);
                foreach (var t in textMessages)
                {
					var parentId = t.ParentNEventId;
					var parentMessage = parentMessages.Where(p => p.Id == parentId).First();
					t.ParentNEvent = parentMessage;
                }
			}
			

			await AddMessagesToDb(textMessages);



			return textMessages;
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
