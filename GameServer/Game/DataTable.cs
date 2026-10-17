using MollyPS.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MollyPS.GameServer.Game.Scene;

namespace MollyPS.GameServer.Game;
 





public static class DataTable
{
    private static readonly Logger Logger = new("DataTable");
    private static readonly Dictionary<string, long> CharStrToNum = [];
     
    private static readonly List<string> AreaIds = [];
     








    public static List<long> GetAllAreaNumIds() => [..AreaNums];
    public static List<string> GetAllAreaIds() => AreaIds.Where(v => v.StartsWith("areaId", StringComparison.Ordinal)).Distinct(StringComparer.Ordinal).OrderBy(v => v, StringComparer.Ordinal).ToList();
    private static readonly List<long> AreaNums = [];
     
    private static readonly Dictionary<string, (string normal, string combo, string ultimate, string attack)> Skills = [];
     
    private static readonly Dictionary<string, (double hp, double sp)> CharVitals = [];
     
    private static readonly Dictionary<long, List<JObject>> WorldEntities = [];
    public const int AttrMaxHp = 1;
    public const int AttrMaxUltimateSp = 22;
    public const long DefaultLevelNumId = 21;  
     
     
    private static readonly double[] FallbackPosition = [225.763, 103.008, -338.223];
     
     
    public static double[] DefaultPosition => GetLevel(DefaultLevelNumId)is { } s ? [s.X, s.Y, s.Z] : FallbackPosition;

    public static readonly string[] DefaultTeam = ["chr_0026_lastrite"];
    public static IEnumerable<string> PlayableChars => Skills.Keys;

    public static long CharToNum(string id) => CharStrToNum.GetValueOrDefault(id, 0);
    public static (string normal, string combo, string ultimate, string attack)? GetSkills(string charId) => Skills.TryGetValue(charId, out var s) ? s : null;
    public static (double hp, double sp) GetVitals(string charId) => CharVitals.GetValueOrDefault(charId, (100, 100));

     

    public sealed record LevelDef(string Id, long NumId, string MapId, double X, double Y, double Z, double RotX, double RotY, double RotZ);
    private static readonly Dictionary<long, LevelDef> Levels = [];  
    private static readonly Dictionary<string, LevelDef> LevelsById = [];  
    public static LevelDef? GetLevel(long numId) => Levels.GetValueOrDefault(numId);
    public static LevelDef? GetLevelByName(string levelId) => string.IsNullOrEmpty(levelId) ? null : LevelsById.GetValueOrDefault(levelId);
     

    public static long GetLevelNumId(string levelId) => GetLevelByName(levelId)?.NumId ?? 0;
    public static List<JObject> GetWorldEntities(long levelNumId) => WorldEntities.TryGetValue(levelNumId, out var list) ? list : [];
     



    public static List<string> GetAllMapLevelIds() => Levels.Values.Where(v => v.Id.StartsWith("map", StringComparison.Ordinal)).Select(v => v.Id).OrderBy(v => v, StringComparer.Ordinal).ToList();
     







    public static List<long> GetAllLevelNumIds() => Levels.Values.Select(v => v.NumId).OrderBy(v => v).ToList();
     

    public static List<string> GetAllMapIds() => Levels.Values.Select(v => v.MapId).Where(v => !string.IsNullOrEmpty(v)).Distinct(StringComparer.Ordinal).OrderBy(v => v, StringComparer.Ordinal).ToList();
    public static void Load()
    {
        CharStrToNum.Clear(); AreaIds.Clear(); AreaNums.Clear();
        Skills.Clear(); CharVitals.Clear(); WorldEntities.Clear(); Levels.Clear(); LevelsById.Clear();
        var configured = ConfigManager.Config.Path.ResourcePath;
        var dir = Path.IsPathRooted(configured) ? configured : Path.Combine(AppContext.BaseDirectory, configured);
        LoadIdTables(Path.Combine(dir, "StrIdNumTable.json"));
        LoadCharacters(Path.Combine(dir, "CharacterTable.json"));
        LoadSkills(Path.Combine(dir, "SkillPatchTable.json"));
        LoadLevels(Path.Combine(dir, "LevelConfigTable.json"));
        LoadWorldEntities(Path.Combine(dir, "WorldEntityRegistry.json"));
        MapUnlockCatalog.Load(dir);
        AnchorCatalog.Load(dir);
        NavigationUnlocks.Load(dir);
        Mail.MailPresentation.Load(dir);
    }

    private static JObject LoadJson(string path)
    {
        try
        {
            using var reader = new StreamReader(path);
            using var json = new JsonTextReader(reader);
            var result = JObject.Load(json);
            while (json.Read())
                if (json.TokenType != JsonToken.Comment)
                    throw new JsonReaderException("Unexpected additional resource JSON content.");
            return result;
        }
        catch (Newtonsoft.Json.JsonReaderException exception)
        {
            throw new InvalidDataException($"Invalid resource JSON: {Path.GetFullPath(path)} " + $"(line {exception.LineNumber}, column {exception.LinePosition}). " + "The export may be incomplete or corrupt; restore a validated table before starting the server.", exception);
        }
    }

    private static void LoadIdTables(string path)
    {
        var obj = LoadJson(path);
        foreach (var(key, value)in obj)
        {
            if (value?["dic"] is not JObject dic)
                continue;
            foreach (var(k, token)in dic)
            {
                var v = token?.Value<long>() ?? 0;
                switch (key)
                {
                    case "char_id":
                        CharStrToNum[k] = v;
                        break;
                    case "area_id":
                        AreaIds.Add(k);
                        AreaNums.Add(v);
                        break;
                }
            }
        }
    }

    private static void LoadCharacters(string path)
    {
        var obj = LoadJson(path);
        foreach (var(id, value)in obj)
        {
            if (value == null)
                continue;
            var attrs = value["attributes"]?[0]?["Attribute"]?["attrs"] as JArray;
            double hp = 100, sp = 100;
            if (attrs != null)
            {
                foreach (var attr in attrs)
                {
                    var type = attr.Value<long>("attrType");
                    var val = attr.Value<double>("attrValue");
                    if (type == AttrMaxHp)
                        hp = val;
                    else if (type == AttrMaxUltimateSp)
                        sp = val;
                }
            }

            CharVitals[id] = (hp, sp);
        }
    }

    private static void LoadSkills(string path)
    {
        var obj = LoadJson(path);
        var skillIds = obj.Properties().Select(p => p.Name).ToList();
        foreach (var charId in CharVitals.Keys)
        {
            var mine = skillIds.Where(s => s.StartsWith(charId, StringComparison.Ordinal)).ToList();
            if (mine.Count == 0)
                continue;
            var normal = mine.FirstOrDefault(s => s.Contains("normal_skill"));
            var combo = mine.FirstOrDefault(s => s.Contains("combo_skill"));
            var ultimate = mine.FirstOrDefault(s => s.Contains("ultimate_skill"));
            var attack = mine.FirstOrDefault(s => s.Contains("_attack1"));
            if (normal == null || combo == null || attack == null)
                continue;  
            Skills[charId] = (normal, combo, ultimate ?? "", attack);
        }
    }

    private static void LoadLevels(string path)
    {
        var obj = LoadJson(path);
        foreach (var(id, value)in obj)
        {
            if (value == null)
                continue;
            var num = value.Value<long?>("idNum");
            var pos = value["playerInitPos"];
            if (num == null || pos == null)
                continue;
             
             
            var rot = value["playerInitRot"];
            var mapId = value.Value<string>("mapIdStr");
            if (string.IsNullOrEmpty(mapId))
            {
                var m = System.Text.RegularExpressions.Regex.Match(id, @"^(map\d+|base\d+)_lv\d+$");
                mapId = m.Success ? m.Groups[1].Value : id;
            }

            var def = new LevelDef(id, num.Value, mapId, pos.Value<double>("x"), pos.Value<double>("y"), pos.Value<double>("z"), rot?.Value<double>("x") ?? 0, rot?.Value<double>("y") ?? 0, rot?.Value<double>("z") ?? 0);
            Levels[num.Value] = def;
            LevelsById[id] = def;
        }

        Logger.Info($"Loaded {Levels.Count} levels ({LevelsById.Count} ids, " + $"{GetAllMapLevelIds().Count} open-world map levels, " + $"{AreaNums.Count} area ids)");
    }

    private static void LoadWorldEntities(string path)
    {
        var root = LoadJson(path);
        var map = root["worldEntityBriefInfos"] as JObject;
        if (map == null)
            return;
        foreach (var(key, value)in map)
        {
            if (value is not JObject entity)
                continue;
            if (!long.TryParse(key, out var entityId))
                continue;
            var levelId = Math.DivRem(entityId, 100_000_000, out _);
            entity["instId"] = key;  
            if (!WorldEntities.TryGetValue(levelId, out var list))
                WorldEntities[levelId] = list = [];
            list.Add(entity);
        }
    }

     






    private static readonly string[] AlwaysResidentPatterns = ["campfire",  
 "teleport",  
 "_tp_",  
 "save_point",  
 "save_group",  
 "checkpoint",  
 "repatriate", "portal",  
 "barrierwall", "blockage", "levelgate", "locked_door", "_edoor", ];
     

    public static bool IsAlwaysResident(string? detailId)
    {
        if (string.IsNullOrEmpty(detailId))
            return false;
        var lower = detailId.ToLowerInvariant();
        return AlwaysResidentPatterns.Any(p => lower.Contains(p));
    }


}
