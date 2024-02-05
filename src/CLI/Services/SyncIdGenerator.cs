using System.Text.RegularExpressions;

namespace Kong.Portal.CLI.Services;

internal class SyncIdGenerator
{
    private readonly List<string> _existingIds = new();
    private readonly Dictionary<string, string> _idMap = new();

    public string Generate(string id, string name)
    {
        var syncId = Regex.Replace(name.ToLowerInvariant(), @"\s", "-");

        foreach (var invalidChar in Path.GetInvalidFileNameChars())
        {
            syncId = syncId.Replace(invalidChar, '-');
        }

        syncId = Regex.Replace(syncId, "-+", "-");

        if (!_existingIds.Contains(syncId))
        {
            _existingIds.Add(syncId);
            _idMap.Add(id, syncId);
            return syncId;
        }

        var index = 1;
        string newSyncId;
        do
        {
            newSyncId = $"{syncId}-{index++}";

            if (index > 100)
            {
                throw new Exception("Cannot generate ID");
            }
        } while (_existingIds.Contains(newSyncId));

        _existingIds.Add(newSyncId);
        _idMap.Add(id, newSyncId);
        return newSyncId;
    }

    public string GetSyncId(string id)
    {
        return _idMap[id];
    }
}
