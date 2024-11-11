using BlazeJump.Common.Enums;

namespace BlazeJump.Common.Services.Message;

public class RelationRegister
{
    private readonly Dictionary<string, Dictionary<RelationTypeEnum, Dictionary<string, bool>>> _relationRegister = new();

    public void AddRelation(string parentEventId, RelationTypeEnum relationType, string childEventId)
    {
        if (!_relationRegister.ContainsKey(parentEventId))
        {
            _relationRegister.Add(parentEventId, new Dictionary<RelationTypeEnum, Dictionary<string, bool>>());
        }

        if (!_relationRegister[parentEventId].ContainsKey(relationType))
        {
            _relationRegister[parentEventId].Add(relationType, new Dictionary<string, bool>());
        }

        _relationRegister[parentEventId][relationType].TryAdd(childEventId, true);
    }

    public bool TryGetRelation(string parentEventId, RelationTypeEnum relationType, out List<string> childEventIds)
    {
        childEventIds = null;
        Dictionary<string, bool> childEventIdDict = null;
        _relationRegister.TryGetValue(parentEventId, out var kindsAndChildEventIds);
        var found = kindsAndChildEventIds?.TryGetValue(relationType, out childEventIdDict) ?? false;
        childEventIds = found ? childEventIdDict.Keys.ToList() : new List<string>();
        return found;
    }

    public bool TryGetRelations(List<string> parentEventIds, RelationTypeEnum relationType, out List<string> childEventIds)
    {
        childEventIds = new List<string>();
        foreach (var parentEventId in parentEventIds)
        {
            if (_relationRegister.TryGetValue(parentEventId, out var kindsAndChildEventIds)
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
}