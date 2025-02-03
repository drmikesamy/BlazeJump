using System.Diagnostics;
using NSubstitute;
using BlazeJump.Common.Enums;
using BlazeJump.Common.Models;
using BlazeJump.Common.Services.Connections;
using BlazeJump.Common.Services.Crypto;
using BlazeJump.Common.Services.UserProfile;
using BlazeJump.Common.Services.Message;
using BlazeJump.Common.Services.Notification;
using AutoMapper;
using BlazeJump.Common.Mappers;
using NUnit.Framework.Internal.Execution;

namespace BlazeJump.Common.Tests.MapperTests
{
	[TestFixture]
	public class NEventProfileTests
	{
		[Test]
		public void Automapper_ConfigIsValid()
		{
			var config = new MapperConfiguration(cfg => cfg.AddProfile<NEventProfile>());
			config.AssertConfigurationIsValid();
		}
		[Test]
		public void Automapper_CreateSignableNEvent_MapsCorrectly()
		{
			var config = new MapperConfiguration(cfg => cfg.AddProfile<NEventProfile>());
			var mapper = config.CreateMapper();

			var nEvent = new NEvent
			{
				Kind = KindEnum.Text,
				Id = "firstEventId",
				Pubkey = "firstUserId",
				Tags = new List<EventTag>
						{
							new EventTag
							{
								Key = TagEnum.e,
								Value = "taggedParentEventId",
								Value3 = "reply"
							},
							new EventTag
							{
								Key = TagEnum.e,
								Value = "taggedRootEventId",
								Value3 = "root"
							},
						},
				Content = "TestContent",
				Created_At = 1234,
				Verified = true
			};

			var signableNEvent = mapper.Map<NEvent, SignableNEvent>(nEvent);

			Assert.That(signableNEvent.Id, Is.EqualTo(0));
			Assert.That(signableNEvent.Pubkey, Is.EqualTo("firstUserId"));
			Assert.That(signableNEvent.Created_At, Is.EqualTo(1234));
			Assert.That(signableNEvent.Tags[0].Value, Is.EqualTo("taggedParentEventId"));
			Assert.That(signableNEvent.Tags[1].Value, Is.EqualTo("taggedRootEventId"));
			Assert.That(signableNEvent.Content, Is.EqualTo("TestContent"));
		}
	}
}