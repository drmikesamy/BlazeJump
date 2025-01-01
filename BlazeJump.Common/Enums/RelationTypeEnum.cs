namespace BlazeJump.Common.Enums
{
	[System.Flags]
    public enum RelationTypeEnum
	{
		SearchToSubscriptionId = 10,
		SubscriptionGuidToIds = 11,
		RootLevelSubscription = 14,
		EventChildren = 15,
		EventsByUser = 16,
		UserByEvent = 17
	}
}
