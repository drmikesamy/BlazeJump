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

        public async Task FetchPage(string hex)
        {
            FilterBuilder filters = new();
            filters
                .AddFilter()
                .AddKind(KindEnum.Text)
                .AddEventId(hex);
            filters
                .AddFilter()
                .AddKind(KindEnum.Text)
                .WithToDate(UntilMarker)
                .AddTaggedEventId(hex);
            filters
                .AddFilter()
                .AddKind(KindEnum.Text)
                .WithToDate(UntilMarker)
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
            RelationRegister.AddRelation(message.Event.UserId, RelationTypeEnum.EventsByUser, message.Event.Id);
            RelationRegister.AddRelation(message.Event.Id, RelationTypeEnum.UserByEvent, message.Event.UserId);

            if (RelationRegister.TryGetRelation(message.SubscriptionId, RelationTypeEnum.SubscriptionGuidToIds,
                    out var guid))
            {
                RelationRegister.AddRelation(message.SubscriptionId, RelationTypeEnum.SubscriptionGuidToIds, message.Event.UserId);
            }
            
            foreach (var tag in message.Event.Tags)
            {
                if (tag.Value != null)
                {
                    switch (tag.Key)
                    {
                        case TagEnum.e:
                            RelationRegister.AddRelation(tag.Value, RelationTypeEnum.EventChildren, message.Event.Id);
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