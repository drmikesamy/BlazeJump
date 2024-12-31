namespace BlazeJump.Common.Enums
{
	[System.Flags]
    public enum RelationTypeEnum
	{
		ETagToReferringEventId = 3,
		EventIdToReplyEventId = 5,
		RootEventIdToETags = 6,
		SearchToSubscriptionId = 10,
		SubscriptionGuidToIds = 11,
		RootLevelSubscription = 14,
		EventChildren = 15,
		EventsByUser = 16,
		UserByEvent = 17
	}
}
