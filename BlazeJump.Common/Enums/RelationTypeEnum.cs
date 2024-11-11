﻿namespace BlazeJump.Common.Enums
{
	[System.Flags]
    public enum RelationTypeEnum
	{
        Metadata = 0,
		TaggedMetadata = 1,
		Text = 2,
		Replies = 3,
		Reactions = 4,
		TaggedParentIds = 5,
		TaggedRootId = 6,
		UserTopLevelEvents = 7,
		TopLevelEvents = 8,
		TopLevelSubscription = 9
	}
}
