using NUnit.Framework;
using NSubstitute;
using BlazeJump.Common.Enums;
using BlazeJump.Common.Services.Message;
using System.Collections.Generic;

namespace BlazeJump.Common.Tests.ServiceTests
{
    [TestFixture]
    public class RelationRegisterTests
    {
        private RelationRegister _relationRegister;

        [SetUp]
        public void SetUp()
        {
            _relationRegister = new RelationRegister();
        }

        [Test]
        public void AddRelation_ShouldAddRelationToRegister()
        {
            // Arrange
            string parentEventId = "parentEvent1";
            RelationTypeEnum relationType = RelationTypeEnum.Replies;
            string childEventId1 = "childEvent1";
            string childEventId2 = "childEvent2";

            // Act
            _relationRegister.AddRelation(parentEventId, relationType, childEventId1);
            _relationRegister.AddRelation(parentEventId, relationType, childEventId2);

            // Assert
            Assert.IsTrue(_relationRegister.TryGetRelations(new List<string> { parentEventId }, relationType, out var childEventIds));
            Assert.That(childEventIds[0], Is.EqualTo("childEvent1"));
            Assert.That(childEventIds[1], Is.EqualTo("childEvent2"));
        }

        [Test]
        public void TryGetRelations_ShouldReturnFalseWhenNoRelationsExist()
        {
            // Arrange
            string parentEventId = "parentEvent1";
            RelationTypeEnum relationType = RelationTypeEnum.Replies;

            // Act
            var result = _relationRegister.TryGetRelations(new List<string> { parentEventId }, relationType, out var childEventIds);

            // Assert
            Assert.IsFalse(result);
            Assert.IsEmpty(childEventIds);
        }

        [Test]
        public void TryGetRelations_ShouldReturnTrueWhenRelationsExist()
        {
            // Arrange
            string parentEventId = "parentEvent1";
            RelationTypeEnum relationType = RelationTypeEnum.Replies;
            string childEventId = "childEvent1";

            _relationRegister.AddRelation(parentEventId, relationType, childEventId);

            // Act
            var result = _relationRegister.TryGetRelations(new List<string> { parentEventId }, relationType, out var childEventIds);

            // Assert
            Assert.IsTrue(result);
            Assert.Contains(childEventId, childEventIds);
        }
    }
}
