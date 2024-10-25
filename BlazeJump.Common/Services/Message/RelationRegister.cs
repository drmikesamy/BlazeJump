using BlazeJump.Common.Enums;

namespace BlazeJump.Common.Services.Message;

public class RelationRegister
{
    private Dictionary<string, Dictionary<FetchTypeEnum, List<string>>> _relationRegister = new();
    public void AddRelation(string parentEventId, FetchTypeEnum fetchType, string childEventId)
    {
        if (!_relationRegister.ContainsKey(parentEventId))
        {
            _relationRegister.Add(parentEventId, new Dictionary<FetchTypeEnum, List<string>>());
        }
        if (!_relationRegister[parentEventId].ContainsKey(fetchType))
        {
            _relationRegister[parentEventId].Add(fetchType, new List<string>());
        }
        _relationRegister[parentEventId][fetchType].Add(childEventId);
    }
    public bool TryGetRelations(List<string> parentEventIds, FetchTypeEnum fetchType, out List<string> childEventIds)
    {
        childEventIds = new List<string>();
        foreach (var parentEventId in parentEventIds)
        {
            if (_relationRegister.TryGetValue(parentEventId, out var kindsAndChildEventIds)
                && kindsAndChildEventIds.TryGetValue(fetchType, out var childIds))
            {
                childEventIds.AddRange(childIds);
            }
        }
        if (childEventIds.Count > 0)
        {
            return true;
        }
        return false;
    }
}