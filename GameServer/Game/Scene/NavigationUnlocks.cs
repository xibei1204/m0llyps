using Newtonsoft.Json.Linq;
using Proto;

namespace MollyPS.GameServer.Game.Scene;

 

public static class NavigationUnlocks
{
    private static SC_SYNC_ALL_MISSION _sync = new();

    public static void Load(string directory)
    {
        var root = JObject.Parse(File.ReadAllText(Path.Combine(directory, "NavigationUnlockCatalog.json")));
        ResourceCatalogIdentity.Validate(root, directory);
        var sync = root["sync"]?.ToObject<SC_SYNC_ALL_MISSION>()
            ?? throw new InvalidDataException("Missing navigation unlock flags.");
        if (sync.Missions.Values.Any(mission => mission.MissionState != 3 || mission.AcceptTime == 0) ||
            sync.CurQuests.Values.Any(quest => quest.QuestState != 3 ||
                quest.QuestObjectives.Concat(quest.DisplayObjectives).Any(objective => !objective.IsComplete)) ||
            sync.CalculateSize() > ushort.MaxValue)
            throw new InvalidDataException("Invalid navigation unlock flags.");
        _sync = sync;
    }

    public static SC_SYNC_ALL_MISSION Build() => _sync.Clone();
}
