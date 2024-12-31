using BlazeJump.Common.Enums;

namespace BlazeJump.Common.Services.Message;

public class RelationRegister
{
    public Dictionary<string, Dictionary<RelationTypeEnum, Dictionary<string, bool>>> Relationships { get; set; } = new();

    public void AddRelation(string parentEventId, RelationTypeEnum relationType, string childEventId)
    {
        if (!Relationships.ContainsKey(parentEventId))
        {
            Relationships.Add(parentEventId, new Dictionary<RelationTypeEnum, Dictionary<string, bool>>());
        }

        if (!Relationships[parentEventId].ContainsKey(relationType))
        {
            Relationships[parentEventId].Add(relationType, new Dictionary<string, bool>());
        }

        Relationships[parentEventId][relationType].TryAdd(childEventId, true);
    }

    public bool TryGetRelation(string parentEventId, RelationTypeEnum relationType, out List<string> childEventIds)
    {
        childEventIds = null;
        Dictionary<string, bool> childEventIdDict = null;
        Relationships.TryGetValue(parentEventId, out var kindsAndChildEventIds);
        var found = kindsAndChildEventIds?.TryGetValue(relationType, out childEventIdDict) ?? false;
        childEventIds = found ? childEventIdDict.Keys.ToList() : new List<string>();
        return found;
    }

    public bool TryGetRelations(List<string> parentEventIds, RelationTypeEnum relationType, out List<string> childEventIds)
    {
        childEventIds = new List<string>();
        foreach (var parentEventId in parentEventIds)
        {
            if (Relationships.TryGetValue(parentEventId, out var kindsAndChildEventIds)
                && kindsAndChildEventIds.TryGetValue(relationType, out var childIds))
            {
                childEventIds.AddRange(childIds.Keys);
            }
        }

        if (childEventIds.Count > 0)
        {
            return true;
        }

        return false;
    }

    public bool RelationExists(string parentEventId, RelationTypeEnum relationType)
    {
        return Relationships.ContainsKey(parentEventId)
            && Relationships[parentEventId].ContainsKey(relationType);
    }
    public Dictionary<string, Dictionary<RelationTypeEnum, Dictionary<string, bool>>> GetAll()
    {
        return Relationships;
    }
}