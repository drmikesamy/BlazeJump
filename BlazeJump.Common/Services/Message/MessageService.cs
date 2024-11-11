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
using BlazeJump.Common.Services.Notification;

namespace BlazeJump.Common.Services.Message
{
    public class MessageService : IMessageService
    {
        private IRelayManager _relayManager;
        private ICryptoService _cryptoService;
        private IUserProfileService _userProfileService;
        private INotificationService _notificationService;
        public Dictionary<string, NMessage> MessageStore { get; set; } = new();
        public RelationRegister RelationRegister { get; set; } = new();
        public DateTime UntilMarker { get; set; } = DateTime.Now.AddDays(1);

        public MessageService(IRelayManager relayManager, ICryptoService cryptoService,
            IUserProfileService userProfileService, INotificationService notificationService)
        {
            _relayManager = relayManager;
            _cryptoService = cryptoService;
            _userProfileService = userProfileService;
            _relayManager.ProcessMessageQueue += ProcessReceivedMessages;
            _notificationService = notificationService;
        }

        public async Task FetchPage(string hex, PageTypeEnum pageType, bool firstLoad = false,
            bool isRelatedData = false)
        {
            if (firstLoad && MessageStore.ContainsKey(hex))
            {
                return;
            }

            if (firstLoad)
            {
                UntilMarker = DateTime.Now.AddDays(1);
            }

            FilterBuilder filters = new();
            if (firstLoad
                && pageType == PageTypeEnum.Event)
            {
                filters
                    .AddFilter()
                    .AddKind(KindEnum.Text)
                    .AddEventId(hex);
                filters
                    .AddFilter()
                    .AddKind(KindEnum.Text)
                    .WithToDate(UntilMarker)
                    .AddTaggedEventId(hex);
            }
            else if (pageType == PageTypeEnum.User)
            {
                filters
                    .AddFilter()
                    .AddKind(KindEnum.Text)
                    .WithToDate(UntilMarker)
                    .WithLimit(5)
                    .AddAuthor(hex);
            }

            var filterList = filters.Build();
            var subscriptionId = Guid.NewGuid().ToString();
            if (!isRelatedData)
            {
                RelationRegister.AddRelation(hex, RelationTypeEnum.TopLevelSubscription, subscriptionId);
                RelationRegister.AddRelation(subscriptionId, RelationTypeEnum.TopLevelEvents, hex);
            }

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
                Console.WriteLine($"Processing {message.MessageType} message with id {message.Event.Id} and Kind {message.Event.Kind}");
                switch (message.Event.Kind)
                {
                    case KindEnum.Metadata:
                        MessageStore.TryAdd(message.Event.UserId, message);
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
            Console.WriteLine($"Processing text message top level with Id {message.Event.Id}");
            if (message?.Event?.Kind == KindEnum.Text
                && RelationRegister.TryGetRelation(message.SubscriptionId, RelationTypeEnum.TopLevelEvents, out var topLevelEventList))
            {
                RelationRegister.AddRelation(message.SubscriptionId, RelationTypeEnum.TopLevelEvents, message.Event.Id);
                RelationRegister.AddRelation(message.Event.Id, RelationTypeEnum.Metadata, message.Event.UserId);
                
                if (UntilMarker > message.Event.CreatedAtDateTime)
                {
                    UntilMarker = message.Event.CreatedAtDateTime.AddMilliseconds(-1);
                }
            }

            foreach (var tag in message.Event.Tags)
            {
                if (tag.Value != null)
                {
                    switch (tag.Key)
                    {
                        case TagEnum.e:
                            if (tag.Value3 == "reply")
                            {
                                Console.WriteLine($"Processing reply tag for event with Id {message.Event.Id}");
                                RelationRegister.AddRelation(message.Event.Id, RelationTypeEnum.TaggedParentIds,
                                    tag.Value);
                            }
                            else if (tag.Value3 == "root")
                            {
                                Console.WriteLine($"Processing root tag for event with Id {message.Event.Id}");
                                RelationRegister.AddRelation(message.Event.Id, RelationTypeEnum.TaggedRootId,
                                    tag.Value);
                            }
                            Console.WriteLine($"Processing relation for message that {message.Event.Id} replies to.");
                            RelationRegister.AddRelation(tag.Value, RelationTypeEnum.Replies, message.Event.Id);
                            break;
                        case TagEnum.p:
                            Console.WriteLine($"Processing user meta tag for event with Id {message.Event.Id}");
                            RelationRegister.AddRelation(message.Event.Id, RelationTypeEnum.Metadata, tag.Value);
                            break;
                        default:
                            break;
                    }
                }
            }
        }
        private void EndOfFetch(string subscriptionId)
        {
            if (RelationRegister.TryGetRelation(subscriptionId, RelationTypeEnum.TopLevelEvents, out var eventIds))
            {
                FilterBuilder filterBuilder = new();

                filterBuilder
                    .AddFilter()
                    .AddKind(KindEnum.Text)
                    .AddTaggedEventIds(eventIds.Distinct().ToList());

                if (RelationRegister.TryGetRelations(eventIds, RelationTypeEnum.Metadata, out var userIds))
                {
                    filterBuilder
                        .AddFilter()
                        .AddKind(KindEnum.Metadata)
                        .AddAuthors(userIds.Where(id => !MessageStore.ContainsKey(id)).Distinct().ToList());
                }

                if (RelationRegister.TryGetRelations(eventIds, RelationTypeEnum.TaggedParentIds,
                        out var childEventIdsReply))
                {
                    var missingEventIdsReply = childEventIdsReply.Where(id => !MessageStore.ContainsKey(id)).ToList();

                    filterBuilder
                        .AddFilter()
                        .AddKind(KindEnum.Text)
                        .AddEventIds(missingEventIdsReply.Distinct().ToList());
                }

                if (RelationRegister.TryGetRelations(eventIds, RelationTypeEnum.TaggedRootId,
                        out var childEventIdsRoot))
                {
                    var missingEventIdsRoot = childEventIdsRoot.Where(id => !MessageStore.ContainsKey(id)).ToList();

                    filterBuilder
                        .AddFilter()
                        .AddKind(KindEnum.Text)
                        .AddEventIds(missingEventIdsRoot.Distinct().ToList());
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

            var signableEvent = nEvent.GetSignableNEvent();
            var serialisedNEvent = JsonConvert.SerializeObject(signableEvent);
            var signature = _cryptoService.Sign(serialisedNEvent);
            nEvent.Sig = signature;
            return true;
        }
        public bool Verify(NEvent nEvent)
        {
            var signableEvent = nEvent.GetSignableNEvent();
            var serialisedNEvent = JsonConvert.SerializeObject(signableEvent);
            var verified = _cryptoService.Verify(nEvent.Sig, serialisedNEvent, nEvent.UserId);
            return verified;
        }

        public async Task Send(KindEnum kind, string message)
        {
            var nEvent = CreateNEvent(kind, message);
            var subscriptionHash = Guid.NewGuid().ToString();
            Sign(ref nEvent);
            await _relayManager.SendNEvent(nEvent, subscriptionHash);
        }

        private NEvent CreateNEvent(KindEnum kind, string message, string parentId = null)
        {
            return new NEvent
            {
                Id = "0",
                UserId = _userProfileService.NPubKey,
                Kind = kind,
                Content = message,
                Created_At = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            };
        }
    }
}