using BlazeJump.Common.Enums;
using BlazeJump.Common.Models;
using BlazeJump.Common.Services.Connections;
using BlazeJump.Common.Services.Crypto;
using Newtonsoft.Json;
using BlazeJump.Common.Services.UserProfile;
using Microsoft.Maui.Controls;
using System;
using System.Diagnostics;
using BlazeJump.Common.Builders;
using BlazeJump.Common.Helpers;
using BlazeJump.Common.Services.Notification;
using BlazeJump.Helpers;
using AutoMapper;

namespace BlazeJump.Common.Services.Message
{
    public class MessageService : IMessageService
    {
        private IRelayManager _relayManager;
        private ICryptoService _cryptoService;
        private IUserProfileService _userProfileService;
        private INotificationService _notificationService;
        private IMapper _mapper;
        public Dictionary<string, NMessage> MessageStore { get; set; } = new();
        public RelationRegister RelationRegister { get; set; } = new();

        public MessageService(IRelayManager relayManager, ICryptoService cryptoService,
            IUserProfileService userProfileService, INotificationService notificationService, IMapper mapper)
        {
            _relayManager = relayManager;
            _cryptoService = cryptoService;
            _userProfileService = userProfileService;
            _relayManager.ProcessMessageQueue += ProcessReceivedMessages;
            _notificationService = notificationService;
            _mapper = mapper;
        }

        public async Task LookupUser(string searchString)
        {
            if (RelationRegister.TryGetRelation(searchString, RelationTypeEnum.SearchToSubscriptionId, out var guid))
            {
                return;
            }

            var filterBuilder = new FilterBuilder();
            var filterList = filterBuilder
                .AddFilter()
                .AddKind(KindEnum.Metadata)
                .AddSearch(searchString)
                .WithLimit(10)
                .Build();
            var lookupGuid = Guid.NewGuid().ToString();
            RelationRegister.AddRelation(lookupGuid, RelationTypeEnum.SubscriptionGuidToIds, lookupGuid);
            RelationRegister.AddRelation(searchString, RelationTypeEnum.SearchToSubscriptionId, lookupGuid);
            await Fetch(filterList, lookupGuid);
        }

        public async Task FetchPage(string hex, DateTime? untilMarker = null)
        {
            var until = untilMarker ?? DateTime.Now.AddDays(1);
            
            FilterBuilder filters = new();
            filters
                .AddFilter()
                .AddKind(KindEnum.Text)
                .AddEventId(hex);
            filters
                .AddFilter()
                .AddKind(KindEnum.Text)
                .WithToDate(until)
                .AddTaggedEventId(hex);
            filters
                .AddFilter()
                .AddKind(KindEnum.Text)
                .WithToDate(until)
                .WithLimit(5)
                .AddAuthor(hex);
            var filterList = filters.Build();
            var subscriptionId = Guid.NewGuid().ToString();
            RelationRegister.AddRelation(subscriptionId, RelationTypeEnum.RootLevelSubscription, hex);
            await Fetch(filterList, subscriptionId);
        }

        public async Task Fetch(List<Filter> filters, string? subscriptionId = null,
            MessageTypeEnum? messageType = null)
        {
            await _relayManager.QueryRelays(subscriptionId ?? Guid.NewGuid().ToString(),
                messageType ?? MessageTypeEnum.Req, filters);
        }

        private void ProcessReceivedMessages(object o, EventArgs eventArgs)
        {
            Console.WriteLine($"Process queue called...");
            while (_relayManager.ReceivedMessages.Count > 0)
            {
                var message = _relayManager.ReceivedMessages.Dequeue();

                if (message.MessageType == MessageTypeEnum.Eose)
                {
                    Console.WriteLine($"EOSE message received. Refreshing view.");
                    EndOfFetch(message.SubscriptionId);
                    _notificationService.UpdateTheState();
                    continue;
                }

                if (message.Event == null)
                {
                    Console.WriteLine($"Message has no event. Skipping.");
                    continue;
                }

                Console.WriteLine(
                    $"Processing {message.MessageType} message with id {message.Event.Id} and Kind {message.Event.Kind}");
                switch (message.Event.Kind)
                {
                    case KindEnum.Metadata:
                        MessageStore.TryAdd(message.Event.Pubkey, message);
                        break;
                    case KindEnum.Text:
                        MessageStore.TryAdd(message.Event.Id, message);
                        break;
                }

                Console.WriteLine($"Processing relations for {message.MessageType} message with id {message.Event.Id}");
                ProcessRelations(message);
            }
        }

        private void ProcessRelations(NMessage message)
        {
            RelationRegister.AddRelation(message.Event.Pubkey, RelationTypeEnum.EventsByUser, message.Event.Id);
            RelationRegister.AddRelation(message.Event.Id, RelationTypeEnum.UserByEvent, message.Event.Pubkey);

            if (RelationRegister.TryGetRelation(message.SubscriptionId, RelationTypeEnum.SubscriptionGuidToIds,
                    out var guid))
            {
                RelationRegister.AddRelation(message.SubscriptionId, RelationTypeEnum.SubscriptionGuidToIds, message.Event.Pubkey);
            }
            
            foreach (var tag in message.Event.Tags)
            {
                if (tag.Value != null)
                {
                    switch (tag.Key)
                    {
                        case TagEnum.e:
                            RelationRegister.AddRelation(tag.Value, RelationTypeEnum.EventChildren, message.Event.Id);
                            if (tag.Value3 == "Root")
                            {
								RelationRegister.AddRelation(message.Event.Id, RelationTypeEnum.EventRoot, tag.Value);
							}
							if (tag.Value3 == "Reply")
							{
								RelationRegister.AddRelation(message.Event.Id, RelationTypeEnum.EventParent, tag.Value);
							}
							break;
                        default:
                            break;
                    }
                }
            }
        }

        private void EndOfFetch(string subscriptionId)
        {
            if (RelationRegister.TryGetRelation(subscriptionId, RelationTypeEnum.RootLevelSubscription, out var rootId)
                && (RelationRegister.TryGetRelations(rootId, RelationTypeEnum.EventChildren, out var topLevelEventIds)
                || RelationRegister.TryGetRelations(rootId, RelationTypeEnum.EventsByUser, out topLevelEventIds)))
            {
                    FilterBuilder filterBuilder = new();

                    filterBuilder
                        .AddFilter()
                        .AddKind(KindEnum.Text)
                        .AddTaggedEventIds(topLevelEventIds.Distinct().ToList());
                    
                    topLevelEventIds.Add(rootId.First());

                    if (RelationRegister.TryGetRelations(topLevelEventIds, RelationTypeEnum.UserByEvent,
                            out var userIds))
                    {
                        filterBuilder
                            .AddFilter()
                            .AddKind(KindEnum.Metadata)
                            .AddAuthors(userIds.Where(id => !MessageStore.ContainsKey(id)).Distinct().ToList());
                    }

                    var filters = filterBuilder.Build();

                    var fetchId = Guid.NewGuid().ToString();
                    _ = Fetch(filters, fetchId);
            }
        }

        private bool Sign(ref NEvent nEvent)
        {
            if (_cryptoService.EtherealPublicKey == null)
            {
                _cryptoService.CreateEtherealKeyPair();
            }
			nEvent.Pubkey = Convert.ToHexString(_cryptoService.EtherealPublicKey.ToXOnlyPubKey().ToBytes()).ToLower();
			var signableEvent = _mapper.Map<NEvent, SignableNEvent>(nEvent);
			var serialisedNEvent = JsonConvert.SerializeObject(signableEvent);
			nEvent.Id = Convert.ToHexString(serialisedNEvent.SHA256Hash()).ToLower();
			var signature = _cryptoService.Sign(serialisedNEvent);
			nEvent.Sig = signature.ToLower();
			return true;
        }

        public bool Verify(NEvent nEvent)
        {
            var signableEvent = _mapper.Map<NEvent, SignableNEvent>(nEvent);
			var serialisedNEvent = JsonConvert.SerializeObject(signableEvent);
            var verified = _cryptoService.Verify(nEvent.Sig, serialisedNEvent, nEvent.Pubkey);
            return verified;
        }

        public async Task Send(KindEnum kind, NEvent nEvent, string encryptPubKey = null)
        {
            if (!String.IsNullOrEmpty(encryptPubKey))
            {
                var encryptedContent = await _cryptoService.AesEncrypt(nEvent.Content, encryptPubKey);
                nEvent.Content = JsonConvert.SerializeObject(encryptedContent);
            }
			Sign(ref nEvent);
			var subscriptionHash = Guid.NewGuid().ToString();
			await _relayManager.SendNEvent(nEvent, subscriptionHash);
        }

        public NEvent CreateNEvent(KindEnum kind, string message, string parentId = null, string rootId = null, List<string> ptags = null)
        {
            var newEvent = new NEvent
			{
				Pubkey = _userProfileService.NPubKey,
				Kind = kind,
				Content = message,
				Created_At = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
			};
			if (parentId != null)
            {
                newEvent.Tags.Add(new EventTag
                {
                    Key = TagEnum.e,
                    Value = parentId,
                    Value3 = "reply"
                });
            }
            if (rootId != null)
            {
				newEvent.Tags.Add(new EventTag
				{
					Key = TagEnum.e,
					Value = rootId,
					Value3 = "root"
				});
			}
            if (ptags != null)
            {
				foreach (var ptag in ptags)
				{
					newEvent.Tags.Add(new EventTag
					{
						Key = TagEnum.p,
						Value = ptag
					});
				}
			}
            return newEvent;
        }
    }
}