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
        public Dictionary<string, List<string>> TopLevelFetchRegister { get; set; } = new();
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
            if (firstLoad)
            {
                UntilMarker = DateTime.Now.AddDays(1);
            }
            FilterBuilder filters = new();
            if (firstLoad && pageType == PageTypeEnum.Event)
            {
                filters
                    .AddFilter()
                    .AddKind(KindEnum.Text)
                    .AddEventId(hex);
                filters
                    .AddFilter()
                    .AddKind(KindEnum.Text)
                    .WithToDate(UntilMarker)
                    .WithLimit(100)
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
            if (filterList.All(f => f.TaggedEventIds?.Count() > 0 || f.Authors?.Count() > 0 || f.EventIds.Count() > 0))
            {
                var fetchId = Guid.NewGuid().ToString();
                if (!isRelatedData)
                    TopLevelFetchRegister.Add(fetchId, new List<string>());
                await Fetch(filterList, fetchId);
            }
        }

        public async Task Fetch(List<Filter> filters, string? subscriptionId = null,
            MessageTypeEnum? messageType = null)
        {
            await _relayManager.QueryRelays(subscriptionId ?? Guid.NewGuid().ToString(),
                messageType ?? MessageTypeEnum.Req, filters);
        }

        private void ProcessReceivedMessages(object o, EventArgs eventArgs)
        {
            while (_relayManager.ReceivedMessages.Count > 0)
            {
                Console.WriteLine($"{_relayManager.ReceivedMessages.Count} messages in receive queue.");
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

                Console.WriteLine($"Processing {message.MessageType} message with id {message.Event.Id}");
                if (message.Event.Kind == KindEnum.Metadata)
                {
                    MessageStore.TryAdd(message.Event.UserId, message);
                }
                else
                {
                    MessageStore.TryAdd(message.Event.Id, message);
                }

                if (TopLevelFetchRegister.TryGetValue(message.SubscriptionId, out var topLevelEventList)
                    && !topLevelEventList.Contains(message.Event.Id))
                {
                    topLevelEventList.Add(message.Event.Id);
                    if (UntilMarker > message.Event.CreatedAtDateTime)
                    {
                        UntilMarker = message.Event.CreatedAtDateTime.AddMilliseconds(-1);
                    }
                }

                ProcessRelations(message);
            }
        }

        private void ProcessRelations(NMessage message)
        {
            foreach (var tag in message.Event.Tags)
            {
                if (tag.Value != null)
                {
                    switch (tag.Key)
                    {
                        case TagEnum.e:
                            if (tag.Value3 == "reply")
                            {
                                RelationRegister.AddRelation(message.Event.Id, FetchTypeEnum.TaggedParentIds,
                                    tag.Value);
                            }
                            else if (tag.Value3 == "root")
                            {
                                RelationRegister.AddRelation(message.Event.Id, FetchTypeEnum.TaggedRootId, tag.Value);
                            }

                            RelationRegister.AddRelation(tag.Value, FetchTypeEnum.Replies, message.Event.Id);
                            break;
                        case TagEnum.p:
                            RelationRegister.AddRelation(message.Event.Id, FetchTypeEnum.TaggedMetadata, tag.Value);
                            break;
                        default:
                            break;
                    }
                }
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

        private void EndOfFetch(string subscriptionId)
        {
            if (TopLevelFetchRegister.TryGetValue(subscriptionId, out var eventIds))
            {
                var messages = eventIds.Select(id => MessageStore[id]).ToList();

                FilterBuilder filterBuilder = new();

                filterBuilder
                    .AddFilter()
                    .AddKind(KindEnum.Text)
                    .AddTaggedEventIds(eventIds.Distinct().ToList());

                filterBuilder
                    .AddFilter()
                    .AddKind(KindEnum.Metadata)
                    .AddAuthors(messages.Select(m => m.Event.UserId).Distinct().ToList());

                if (RelationRegister.TryGetRelations(eventIds, FetchTypeEnum.TaggedParentIds,
                        out var childEventIdsReply))
                {
                    var missingEventIdsReply = childEventIdsReply.Where(id => !MessageStore.ContainsKey(id)).ToList();
                    
                    filterBuilder
                        .AddFilter()
                        .AddKind(KindEnum.Text)
                        .AddEventIds(missingEventIdsReply.Distinct().ToList());
                }

                if (RelationRegister.TryGetRelations(eventIds, FetchTypeEnum.TaggedRootId, out var childEventIdsRoot))
                {
                    var missingEventIdsRoot = childEventIdsRoot.Where(id => !MessageStore.ContainsKey(id)).ToList();
                    
                    filterBuilder
                        .AddFilter()
                        .AddKind(KindEnum.Text)
                        .AddEventIds(missingEventIdsRoot.Distinct().ToList());
                }

                var filters = filterBuilder.Build();
                if (filters.All(f => f.TaggedEventIds != null || f.Authors != null || f.EventIds != null))
                {
                    var fetchId = Guid.NewGuid().ToString();
                    _ = Fetch(filters, fetchId);
                }
            }
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