using BlazeJump.Common.Enums;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace BlazeJump.Common.Services.Message
{
	public class RelationRegister
	{
		public ConcurrentDictionary<string, ConcurrentDictionary<RelationTypeEnum, ConcurrentDictionary<string, bool>>> Relationships { get; set; } = new();

		public void AddRelation(string parentEventId, RelationTypeEnum relationType, string childEventId)
		{
			var relationDict = Relationships.GetOrAdd(parentEventId, _ => new ConcurrentDictionary<RelationTypeEnum, ConcurrentDictionary<string, bool>>());
			var childDict = relationDict.GetOrAdd(relationType, _ => new ConcurrentDictionary<string, bool>());
			childDict.TryAdd(childEventId, true);
		}

		public bool TryGetRelation(string parentEventId, RelationTypeEnum relationType, out List<string> childEventIds)
		{
			childEventIds = null;
			if (Relationships.TryGetValue(parentEventId, out var kindsAndChildEventIds) &&
				kindsAndChildEventIds.TryGetValue(relationType, out var childEventIdDict))
			{
				childEventIds = childEventIdDict.Keys.ToList();
				return true;
			}
			childEventIds = new List<string>();
			return false;
		}

		public bool TryGetRelations(List<string> parentEventIds, RelationTypeEnum relationType, out List<string> childEventIds)
		{
			childEventIds = new List<string>();
			foreach (var parentEventId in parentEventIds)
			{
				if (Relationships.TryGetValue(parentEventId, out var kindsAndChildEventIds) &&
					kindsAndChildEventIds.TryGetValue(relationType, out var childIds))
				{
					childEventIds.AddRange(childIds.Keys);
				}
			}
			return childEventIds.Count > 0;
		}

		public bool RelationExists(string parentEventId, RelationTypeEnum relationType)
		{
			return Relationships.ContainsKey(parentEventId) && Relationships[parentEventId].ContainsKey(relationType);
		}

		public ConcurrentDictionary<string, ConcurrentDictionary<RelationTypeEnum, ConcurrentDictionary<string, bool>>> GetAll()
		{
			return Relationships;
		}
	}
}
