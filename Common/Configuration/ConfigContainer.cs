using Newtonsoft.Json;

namespace MollyPS.Configuration;

public sealed class ConfigContainer
{
    [JsonProperty("host")]
    public HostOption Host { get; set; } = new();
    [JsonProperty("game")]
    public GameOption Game { get; set; } = new();
    [JsonProperty("log")]
    public PathOption Path { get; set; } = new();
    [JsonProperty("httpServer")]
    public HttpServerOption HttpServer { get; set; } = new();
    [JsonProperty("debug")]
    public DebugOption Debug { get; set; } = new();

    public sealed class HostOption
    {
        [JsonProperty("gameServerIp")]
        public string GameServerIp { get; set; } = "127.0.0.1";
        [JsonProperty("gameServerPort")]
        public int GameServerPort { get; set; } = 23301;
    }

    public sealed class GameOption
    {
        [JsonProperty("serverName")]
        public string ServerName { get; set; } = "MollyPS";
        [JsonProperty("defaultName")]
        public string DefaultPlayerName { get; set; } = "Molly";
        [JsonProperty("defaultUid")]
        public uint DefaultUid { get; set; } = 100000001;
        [JsonProperty("rsaPadding")]
        public string RsaPadding { get; set; } = "OaepSHA1";
        [JsonProperty("enableCrypto")]
        public bool EnableCrypto { get; set; }
        [JsonProperty("branchVersion")]
        public string BranchVersion { get; set; } = "";
        [JsonProperty("welcomeMail")]
        public WelcomeMailOption WelcomeMail { get; set; } = new();
    }

    public sealed class WelcomeMailOption
    {
        [JsonProperty("enable")]
        public bool Enable { get; set; } = true;
        [JsonProperty("title")]
        public string Title { get; set; } = "Welcome";
        [JsonProperty("content")]
        public string Content { get; set; } = "Welcome to MollyPS. This is a free, open-source project intended solely for reference and educational purposes. Project URL: https://github.com/xibei1204/m0llyps. If you paid to obtain this project, you have been scammed.";
        [JsonProperty("sender")]
        public string Sender { get; set; } = "system";
        [JsonProperty("expireDays")]
        public int ExpireDays { get; set; }
    }

    public sealed class PathOption
    {
        [JsonProperty("logPath")]
        public string LogPath { get; set; } = "Log";
        [JsonProperty("resourcePath")]
        public string ResourcePath { get; set; } = "Resources";
    }

    public sealed class DebugOption
    {
        [JsonProperty("enablePacketLog")]
        public bool EnablePacketLog { get; set; }
        [JsonProperty("logPacketToConsole")]
        public bool LogPacketToConsole { get; set; }
        [JsonProperty("logPacketPayload")]
        public bool LogPacketPayload { get; set; }
    }

    public sealed class HttpServerOption
    {
        [JsonProperty("enable")]
        public bool Enable { get; set; } = true;
        [JsonProperty("port")]
        public int Port { get; set; } = 23302;
        [JsonProperty("httpsPort")]
        public int HttpsPort { get; set; } = 443;
    }
}
