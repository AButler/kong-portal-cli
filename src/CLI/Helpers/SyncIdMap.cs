namespace Kong.Portal.CLI;

public class SyncIdMap
{
    private readonly Dictionary<string, string> _syncIdToIdMap = new();
    private readonly Dictionary<string, string> _idToSyncIdMap = new();

    public SyncIdMap() { }

    public SyncIdMap(IDictionary<string, string> dictionary)
    {
        _syncIdToIdMap = dictionary.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        _idToSyncIdMap = dictionary.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
    }

    public void Add(string syncId, string id)
    {
        _syncIdToIdMap.Add(syncId, id);
        _idToSyncIdMap.Add(id, syncId);
    }

    public string GetId(string syncId)
    {
        return _syncIdToIdMap[syncId];
    }

    public string GetSyncId(string id)
    {
        return _idToSyncIdMap[id];
    }
}
