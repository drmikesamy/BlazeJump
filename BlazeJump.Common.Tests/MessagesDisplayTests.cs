using BlazeJump.Common.Enums;
using BlazeJump.Common.Models;
using BlazeJump.Common.Services.Connections.Events;
using BlazeJump.Common.Services.Display;
using BlazeJump.Common.Services.Message;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities.Interfaces;
using NSubstitute;
using static QRCoder.PayloadGenerator;
using MessageReceivedEventArgs = BlazeJump.Common.Services.Connections.Events.MessageReceivedEventArgs;

namespace BlazeJump.Common.Tests
{
	public class MessagesDisplayTests
	{
		private IMessageService _mockMessageService;
		private string _hex;
		private MessagesDisplay _sut;
		private NMessage _nMessage;
		private MessageReceivedEventArgs _messageReceivedEventArgs;

		[SetUp]
		public void Setup()
		{
			_hex = Convert.ToHexString(Guid.NewGuid().ToByteArray());
			_mockMessageService = Substitute.For<IMessageService>();
			_mockMessageService.FetchNEventsByFilter(MessageTypeEnum.Req, Arg.Any<List<Filter>>(), Arg.Any<string>()).Returns(Task.FromResult(1));
			_sut = new MessagesDisplay(_mockMessageService);
			_nMessage = new NMessage
			{
				MessageType = MessageTypeEnum.Event,
				Event = new NEvent
				{
					Pubkey = "testPubKey",
					Content = "testMessage"
				},
				SubscriptionId = "testSubscriptionId"
			};
			_sut.MessageBuckets = new Dictionary<string, List<NMessage>>
			{
				{ _nMessage.SubscriptionId, new List<NMessage>() }
			};
			_messageReceivedEventArgs = new MessageReceivedEventArgs(
				"test.url",
				_nMessage
			);
			_sut.StateUpdated += UpdateTheState!;
		}
		[Test]
		public async Task OnInit_ForEventPage_SetsCorrectFilters()
		{
			await _sut.Init(PageTypeEnum.Event, _hex);

			Assert.That(_sut.Filters.Count, Is.EqualTo(2));

			Assert.That(_sut.Filters[0]?.EventIds?.SingleOrDefault(), Is.EqualTo(_hex));
			Assert.That(_sut.Filters[0]?.Kinds?.SingleOrDefault(), Is.EqualTo((int)KindEnum.Text));
			Assert.That(_sut.Filters[0]?.Limit, Is.EqualTo(1));

			Assert.That(_sut.Filters[1]?.TaggedEventIds?.SingleOrDefault(), Is.EqualTo(_hex));
			Assert.That(_sut.Filters[1]?.Kinds?.SingleOrDefault(), Is.EqualTo((int)KindEnum.Text));
			Assert.That(_sut.Filters[1]?.Limit, Is.EqualTo(5));
		}
		[Test]
		public async Task OnInit_ForUserPage_SetsCorrectFilters()
		{
			await _sut.Init(PageTypeEnum.User, _hex);

			Assert.That(_sut.Filters.Count, Is.EqualTo(1));

			Assert.That(_sut.Filters[0]?.Authors?.SingleOrDefault(), Is.EqualTo(_hex));
			Assert.That(_sut.Filters[0]?.Kinds?.SingleOrDefault(), Is.EqualTo((int)KindEnum.Text));
			Assert.That(_sut.Filters[0]?.Limit, Is.EqualTo(5));
		}
		[Test]
		public void OnEventMessageReceived_AddsToMessageBucket()
		{
			_mockMessageService.NewMessageReceived += Raise.EventWith(new object(), _messageReceivedEventArgs);
			Assert.That(_sut.MessageBuckets?.Single().Value?.Single().Event?.Content, Is.EqualTo(_nMessage?.Event?.Content));
		}
		[Test]
		public void OnReplyMessageReceived_AddsToParentMessage()
		{
			var subscriptionId = "reply_test";
			var parentEventId = "parentEventId";
			var childEventId = "childEventId";

			var existingParent = new NMessage
			{
				MessageType = MessageTypeEnum.Event,
				Event = new NEvent
				{
					Id = parentEventId
				},
				SubscriptionId = subscriptionId
			};

			_nMessage.Context = MessageContextEnum.Reply;
			_nMessage.Event!.Id = childEventId;
			_nMessage.Event!.Tags = new List<EventTag>
			{
				new EventTag
				{
					Key = TagEnum.e,
					Value = parentEventId
				}
			};

			_nMessage.SubscriptionId = subscriptionId;
			_sut.MessageBuckets.Add(subscriptionId, new List<NMessage> { existingParent });

			_mockMessageService.NewMessageReceived += Raise.EventWith(new object(), _messageReceivedEventArgs);

			Assert.That(_sut.MessageBuckets?[subscriptionId].Single().Event?.Replies?.Single().Id, Is.EqualTo(childEventId));
		}
		[Test]
		public void OnUserMessageReceived_AddsToUserCache()
		{
			var userString = $$"""
                {
                    "name": "testName",
                    "about": "testAbout",
                    "picture": "testPicture",
                    "banner": "testBanner",
                }
            """;

			_nMessage.Context = MessageContextEnum.User;
			_nMessage.Event!.Content = userString;

			_mockMessageService.NewMessageReceived += Raise.EventWith(new object(), _messageReceivedEventArgs);

			Assert.That(_sut.Users?.Single().Value?.Username, Is.EqualTo("testName"));
			Assert.That(_sut.Users?.Single().Value?.Bio, Is.EqualTo("testAbout"));
			Assert.That(_sut.Users?.Single().Value?.ProfilePic, Is.EqualTo("testPicture"));
			Assert.That(_sut.Users?.Single().Value?.Banner, Is.EqualTo("testBanner"));
		}
		[Test]
		public void OnEoseMessageReceived_TriggersRepliesAndUsersLoadWithCorrectFilter()
		{
			_nMessage.MessageType = MessageTypeEnum.Eose;
			_sut.MessageBuckets.Single().Value.Add(new NMessage
			{
				SubscriptionId = "testSubscriptionId",
				Event = new NEvent
				{
					Pubkey = "testUserKey",
					Id = "testParentEventId"
				}
			});

			List<Filter> userFetchFilter = new List<Filter>();
			List<Filter> replyFetchFilter = new List<Filter>();

			_mockMessageService.FetchNEventsByFilter(MessageTypeEnum.Req, Arg.Do<List<Filter>>(f => userFetchFilter = f), Arg.Is<string>(s => s.StartsWith("User_")));
			_mockMessageService.FetchNEventsByFilter(MessageTypeEnum.Req, Arg.Do<List<Filter>>(f => replyFetchFilter = f), Arg.Is<string>(s => s.StartsWith("Reply_")));

			_mockMessageService.NewMessageReceived += Raise.EventWith(new object(), _messageReceivedEventArgs);

			Assert.That(userFetchFilter?.Single().Authors?.Single(), Is.EqualTo("testUserKey"));
			Assert.That(replyFetchFilter?.Single().TaggedEventIds?.Single(), Is.EqualTo("testParentEventId"));
		}
		void UpdateTheState(object sender, EventArgs e) { }
	}
}