using MollyPS.Configuration;
using Newtonsoft.Json;

namespace MollyPS.Util;

public static class ConfigManager
{
    public static readonly Logger Logger = new("ConfigManager");
    public static string ConfigDirectory => Path.GetFullPath("Config");
    public static string ConfigFilePath => Path.Combine(ConfigDirectory, "Config.json");
    public static ConfigContainer Config { get; private set; } = new();

    public static void LoadConfig()
    {
        LoadConfigData();
    }

    private static void LoadConfigData()
    {
        Directory.CreateDirectory(ConfigDirectory);
         
        var legacy = Path.GetFullPath("Config.json");
        if (!File.Exists(ConfigFilePath) && File.Exists(legacy))
        {
            File.Move(legacy, ConfigFilePath);
            Logger.Info("Migrated Config.json to Config/Config.json");
        }
        var file = new FileInfo(ConfigFilePath);
        if (!file.Exists)
        {
            Config = new ConfigContainer();
            Logger.Info("Creating default Config/Config.json");
            SaveData(Config, ConfigFilePath);
        }

        using (var stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        using (var reader = new StreamReader(stream))
        {
            var json = reader.ReadToEnd();
            Config = JsonConvert.DeserializeObject<ConfigContainer>(json, new JsonSerializerSettings
            {
                ObjectCreationHandling = ObjectCreationHandling.Replace
            }) ?? throw new InvalidDataException("Config.json must contain a configuration object.");
        }
    }

    public static void SaveData<T>(T data, string path)
    {
        try
        {
            var dir = Path.GetDirectoryName(Path.GetFullPath(path));
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            using var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read);
            using var writer = new StreamWriter(stream)
            {
                AutoFlush = true
            };
            writer.Write(JsonConvert.SerializeObject(data, Formatting.Indented));
        }
        catch (Exception e)
        {
            Logger.Error("SaveData", e);
        }
    }
}
