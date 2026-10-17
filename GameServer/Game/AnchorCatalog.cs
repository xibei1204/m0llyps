using MollyPS.Util;
using Newtonsoft.Json.Linq;

namespace MollyPS.GameServer.Game;

public static class AnchorCatalog
{
    public sealed record Entity(string SceneId, long ShortId);
    public sealed record Destination(string SceneId, float[] Position, float[] Rotation, int Reason);
    public static IReadOnlyList<int> MarkIndices { get; private set; } = [];
    public static IReadOnlyList<long> AnchorIds { get; private set; } = [];
    public static Dictionary<ulong, Entity> Entities { get; } = [];
    public static HashSet<ulong> Anchors { get; } = [];
    public static Dictionary<string, Destination> Teleports { get; } = [];
    public static Destination? CampDestination(ulong id) => Teleports.GetValueOrDefault("TpForMap_ent_" + id);

    public static void Load(string directory)
    {
        var data = JObject.Parse(File.ReadAllText(Path.Combine(directory, "AnchorCatalog.json")));
        ResourceCatalogIdentity.Validate(data, directory);
        MarkIndices = ((JArray)data["markIndices"]!).Select(t => t.Value<int>()).Distinct().Order().ToArray();
        if (MarkIndices.Any(id => id < 0)) throw new InvalidDataException("Invalid map index");
        Entities.Clear(); Anchors.Clear();
        foreach (var (key, value) in (JObject)data["entities"]!)
            Entities.Add(ulong.Parse(key), new Entity(value!.Value<string>("sceneId")!, value.Value<long>("shortId")));
        foreach (var (key, _) in (JObject)data["anchors"]!) Anchors.Add(ulong.Parse(key));
        Teleports.Clear();
        foreach (var (key, value) in (JObject)data["teleports"]!)
        {
            var pos = ((JArray)value!["position"]!).Select(v => v.Value<float>()).ToArray();
            var rot = ((JArray)value["rotation"]!).Select(v => v.Value<float>()).ToArray();
            var scene = value.Value<string>("sceneId")!;
            if (pos.Length != 3 || rot.Length != 3 || !pos.Concat(rot).All(float.IsFinite))
                throw new InvalidDataException("Invalid teleport destination " + key);
            if (DataTable.GetLevelByName(scene) == null)
            {
                if (Anchors.Any(id => key == "TpForMap_ent_" + id))
                    throw new InvalidDataException("Anchor points to an unavailable scene: " + key);
                 
                new Logger("Anchors").Warn($"Ignoring non-playable teleport destination {key} ({scene})");
                continue;
            }
            Teleports.Add(key, new Destination(scene, pos, rot, value.Value<int>("reason")));
        }
        AnchorIds = Anchors.Select(id => Entities[id].ShortId).Distinct().Order().ToArray();
        if (Entities.Values.Any(e => e.ShortId <= 0) || Anchors.Any(id => DataTable.GetLevelByName(Entities[id].SceneId) == null) ||
            Entities.Values.Select(e => e.ShortId).Distinct().Count() != Entities.Count)
            throw new InvalidDataException("Invalid activation short-ID catalog");
        new Logger("Anchors").Info($"Loaded {AnchorIds.Count} anchors and {MarkIndices.Count} map indices");
    }
}
