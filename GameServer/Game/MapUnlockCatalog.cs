using MollyPS.Util;
using Newtonsoft.Json.Linq;

namespace MollyPS.GameServer.Game;

 


public static class MapUnlockCatalog
{
    private static readonly Logger Logger = new("MapUnlock");
    private static long[] mistIds = [];

    public static IReadOnlyList<long> MistIds => mistIds;

    public static void Load(string resourceDirectory)
    {
        mistIds = [];
        var path = Path.Combine(resourceDirectory, "MapUnlockCatalog.json");
        if (!File.Exists(path))
        {
            Logger.Warn("MapUnlockCatalog.json missing; fog unlock sync is unavailable");
            return;
        }

        var catalog = JObject.Parse(File.ReadAllText(path));
        mistIds = (catalog["mistIds"] as JArray ?? [])
            .Values<long>()
            .Where(id => id > 0 && id <= 1_000_000)
            .Distinct()
            .OrderBy(id => id)
            .ToArray();
        Logger.Info($"Loaded {mistIds.Length} client fog ids");
    }
}
