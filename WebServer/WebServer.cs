using System.Net.Security;
using System.Security.Cryptography;
using System.Formats.Asn1;
using System.Net.Sockets;
using System.Collections.Concurrent;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using MollyPS.Util;
using Newtonsoft.Json;

namespace MollyPS.WebServer;

 






public class WebServer : IDisposable
{
    private static readonly Logger Logger = new("WebServer");
    private readonly TcpListener _httpListener;
    private TcpListener? _httpsListener;
    private X509Certificate2? _cert;
    private readonly bool _enableHttps;
    private readonly ConcurrentDictionary<TcpClient, byte> _clients = new();
    private Task? _httpTask;
    private Task? _httpsTask;

    public WebServer(int port, bool enableHttps = true)
    {
        Port = port;
        _enableHttps = enableHttps;
        _httpListener = new TcpListener(System.Net.IPAddress.Any, port);
    }

    public int Port { get; }

     
    public void Start()
    {
        _httpListener.Start();
        Logger.Info($"HTTP server listening on 0.0.0.0:{Port} (http)");
        _httpTask = HttpAcceptLoop();

        if (!_enableHttps) return;

        try
        {
            _cert = LoadOrCreateCertificate();
            var httpsPort = ConfigManager.Config.HttpServer.HttpsPort;
            _httpsListener = new TcpListener(System.Net.IPAddress.Any, httpsPort);
            _httpsListener.Start();
            Logger.Info($"HTTPS server listening on 0.0.0.0:{httpsPort} (tls)");
            _httpsTask = HttpsAcceptLoop();
        }
        catch (Exception e)
        {
            Logger.Warn($"HTTPS on {ConfigManager.Config.HttpServer.HttpsPort} unavailable: {e.Message}");
        }
    }

    private async Task HttpAcceptLoop()
    {
        while (true)
        {
            TcpClient client;
            try { client = await _httpListener.AcceptTcpClientAsync(); }
            catch { break; }
            _clients.TryAdd(client, 0);
            _ = HandlePlain(client);
        }
    }

    private async Task HttpsAcceptLoop()
    {
        while (true)
        {
            TcpClient client;
            try { client = await _httpsListener!.AcceptTcpClientAsync(); }
            catch { break; }
            _clients.TryAdd(client, 0);
            _ = HandleTls(client);
        }
    }

    private async Task HandleTls(TcpClient client)
    {
        try
        {
            using var ssl = new SslStream(client.GetStream());
            await ssl.AuthenticateAsServerAsync(new SslServerAuthenticationOptions
            {
                ServerCertificate = _cert,
            });
            await HandleRequest(client, ssl);
        }
        catch (Exception e)
        {
            Logger.Debug("WebServer tls error: " + (e.InnerException ?? e));
        }
        finally
        {
            _clients.TryRemove(client, out _);
            try { client.Close(); } catch {   }
        }
    }

    private async Task HandlePlain(TcpClient client)
    {
        try
        {
            await HandleRequest(client, client.GetStream());
        }
        catch (Exception e)
        {
            Logger.Debug("WebServer plain error: " + e.Message);
        }
        finally
        {
            _clients.TryRemove(client, out _);
            try { client.Close(); } catch {   }
        }
    }

    private static X509Certificate2 LoadOrCreateCertificate()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "certificate.pfx");
        if (File.Exists(path))
            return X509CertificateLoader.LoadPkcs12FromFile(path, "endfield");

        using var rsa = RSA.Create(2048);
        var req = new CertificateRequest(new X500DistinguishedName("CN=molly-ps"), rsa,
            HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
         
         
         
        req.CertificateExtensions.Add(BuildDnsSanExtension(new[]
        {
            "beyond-tools-nonprod.hypergryph.net",
            "game-config.hypergryph.com",
            "game-config.gryphline.com",
            "u8.hypergryph.com",
            "as.gryphline.com",
        }));
        var now = DateTimeOffset.UtcNow.AddDays(-1);
        using var cert = req.CreateSelfSigned(now, now.AddYears(10));
        var pfx = cert.Export(X509ContentType.Pfx, "endfield");
        File.WriteAllBytes(path, pfx);
        File.WriteAllBytes(Path.Combine(AppContext.BaseDirectory, "certificate.cer"),
            cert.Export(X509ContentType.Cert));
        var pem = "-----BEGIN CERTIFICATE-----" + Environment.NewLine
            + Convert.ToBase64String(cert.Export(X509ContentType.Cert), Base64FormattingOptions.InsertLineBreaks)
            + Environment.NewLine + "-----END CERTIFICATE-----" + Environment.NewLine;
        File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "certificate.pem"), pem);
        Logger.Info($"Generated self-signed certificate: {path} (+ .pem)");
        Logger.Info("Import it into the Windows trusted root store so the client accepts HTTPS:");
        Logger.Info($"  certutil -f -p endfield -user -addstore Root \"{path}\"");
        return X509CertificateLoader.LoadPkcs12(pfx, "endfield");
    }

    public void Dispose()
    {
        _httpListener.Stop();
        _httpsListener?.Stop();
         
        _httpTask?.GetAwaiter().GetResult();
        _httpsTask?.GetAwaiter().GetResult();
        foreach (var client in _clients.Keys) client.Dispose();
        _cert?.Dispose();
    }

     
    private static X509SubjectAlternativeNameExtension BuildDnsSanExtension(string[] dnsNames)
    {
        var writer = new AsnWriter(AsnEncodingRules.DER);
        using (writer.PushSequence())
        {
            foreach (var name in dnsNames)
                writer.WriteCharacterString(UniversalTagNumber.IA5String, name,
                    new Asn1Tag(TagClass.ContextSpecific, 2));
        }

        return new X509SubjectAlternativeNameExtension(writer.Encode());
    }

    private static async Task HandleRequest(TcpClient client, System.IO.Stream stream)
    {
        var buffer = new byte[8192];
        var total = 0;
        var headEnd = -1;
        while (total < buffer.Length)
        {
            var n = await stream.ReadAsync(buffer.AsMemory(total, buffer.Length - total));
            if (n <= 0) return;
            total += n;
            headEnd = IndexOfHeaderEnd(buffer, total);
            if (headEnd >= 0) break;
        }
        if (headEnd < 0) return;

        var headText = Encoding.ASCII.GetString(buffer, 0, headEnd);
        var lines = headText.Split("\r\n");
        var requestLine = lines[0].Split(' ');
        if (requestLine.Length < 2) return;
        var method = requestLine[0];
        var rawUrl = requestLine[1];

        var contentLength = 0;
        foreach (var line in lines.Skip(1))
        {
            var idx = line.IndexOf(':');
            if (idx <= 0) continue;
            var name = line[..idx].Trim().ToLowerInvariant();
            var value = line[(idx + 1)..].Trim();
            if (name == "content-length")
                _ = int.TryParse(value, out contentLength);
        }

        var bodyLen = Math.Min(contentLength, total - headEnd - 4);
        while (total < headEnd + 4 + contentLength && total < buffer.Length)
        {
            var n = await stream.ReadAsync(buffer.AsMemory(total, buffer.Length - total));
            if (n <= 0) break;
            total += n;
            bodyLen = total - headEnd - 4;
        }
        var body = Encoding.UTF8.GetString(buffer, headEnd + 4, Math.Max(0, Math.Min(contentLength, total - headEnd - 4)));

        Logger.Info($"{method} {rawUrl}  host={GetHeader(lines, "host")} body={Truncate(body, 600)}");

        var response = Route(method, rawUrl, body);
        var payload = Encoding.UTF8.GetBytes(response);
        var sb = new StringBuilder();
        sb.Append("HTTP/1.1 200 OK\r\n");
        sb.Append("Content-Type: application/json; charset=utf-8\r\n");
        sb.Append($"Content-Length: {payload.Length}\r\n");
        sb.Append("Connection: close\r\n\r\n");
        var head = Encoding.UTF8.GetBytes(sb.ToString());
        await stream.WriteAsync(head);
        await stream.WriteAsync(payload);
        await stream.FlushAsync();
    }

    private static string Route(string method, string rawUrl, string body)
    {
        var path = rawUrl.Split('?')[0];
        var query = ParseQuery(rawUrl);

         
        if (path.EndsWith("/get_server_list", StringComparison.OrdinalIgnoreCase))
        {
            var addr = ConfigManager.Config.Host.GameServerIp;
            var port = ConfigManager.Config.Host.GameServerPort;
            var name = ConfigManager.Config.Game.ServerName;
            return JsonConvert.SerializeObject(new
            {
                servers = new[]
                {
                    new { name, addr, port },
                },
            });
        }

         
        if (path.EndsWith("/api/game/get_latest", StringComparison.OrdinalIgnoreCase))
        {
            return JsonConvert.SerializeObject(new
            {
                action = 0,
                version = "",
                request_version = "",
                pkg = new
                {
                    packs = Array.Empty<object>(),
                    total_size = 0,
                    file_path = "",
                    url = "",
                    md5 = "",
                    package_size = "0",
                    file_id = "0",
                    sub_channel = "6",
                    game_files_md5 = "",
                },
                patch = (object?)null,
                state = 0,
                launcher_action = 0,
            });
        }

         
        if (path.EndsWith("/api/game/get_latest_resources", StringComparison.OrdinalIgnoreCase))
        {
            return JsonConvert.SerializeObject(new
            {
                resources = Array.Empty<object>(),
                configs = "{\"kick_flag\":false}",
                res_version = "",
                patch_index_path = "",
                domain = "",
            });
        }

         
         
        if (path.Contains("/api/remote_config/", StringComparison.OrdinalIgnoreCase) &&
            path.EndsWith("/game_config", StringComparison.OrdinalIgnoreCase))
        {
            return GameConfigPayload();
        }

         
         
        if (path.Contains("/api/remote_config/", StringComparison.OrdinalIgnoreCase) &&
            path.EndsWith("/network_config", StringComparison.OrdinalIgnoreCase))
        {
            return NetworkConfigPayload();
        }

         
         
         
         
        if (path.EndsWith("/api/remote_config/v2/canary", StringComparison.OrdinalIgnoreCase))
        {
            switch (query.GetValueOrDefault("configName"))
            {
                case "game_config": return GameConfigPayload();
                case "network_config": return NetworkConfigPayload();
            }
        }

         
         
        return "{}";
    }

    private static string GameConfigPayload()
    {
        const string content = "{\"enableHotUpdate\":false,\"mockLogin\":true,\"selectSrv\":false}";
        return RemoteConfigText(content);
    }

    private static string NetworkConfigPayload()
    {
        var self = "http://127.0.0.1:" + ConfigManager.Config.HttpServer.Port;
        var content = JsonConvert.SerializeObject(new Dictionary<string, object>
        {
            ["hgage"] = self + "/age",
            ["hggov"] = self + "/gov",
            ["u8root"] = self + "/u8",
            ["launcherurl"] = self + "/launcher",
            ["netlogurl"] = self + "/netlog",
            ["enableAceWin"] = false,
            ["enableAceIOS"] = false,
            ["enableAceAndroid"] = false,
            ["enableAntiWin"] = false,
            ["enableAntiIOS"] = false,
            ["enableAntiAndroid"] = false,
            ["enableDevOnlyLog"] = true,
            ["enableLoginDevLog"] = true,
            ["enableReleaseLog"] = true,
            ["loginWaitRetryTimeMin"] = 30,
            ["loginWaitRetryTimeMax"] = 60,
            ["qualityDeviceJson"] = "",
            ["vfsLowMemoryMode"] = false,
        });
        return RemoteConfigText(content);
    }

    private static Dictionary<string, string> ParseQuery(string rawUrl)
    {
        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var parts = rawUrl.Split('?', 2);
        if (parts.Length < 2) return dict;
        foreach (var kv in parts[1].Split('&'))
        {
            var i = kv.IndexOf('=');
            if (i <= 0) continue;
            dict[Uri.UnescapeDataString(kv[..i])] = Uri.UnescapeDataString(kv[(i + 1)..]);
        }
        return dict;
    }

    private static string RemoteConfigText(string content)
    {
        var key = Convert.FromHexString("7199BCE947C3A7F9202776A02B1F8764");
        return Convert.ToBase64String(Crypto.AesCbcEncrypt(key, System.Text.Encoding.UTF8.GetBytes(content)));
    }

    private static string Truncate(string s, int len) => s.Length <= len ? s : s[..len] + "...";

    private static string GetHeader(string[] lines, string name)
    {
        foreach (var line in lines.Skip(1))
        {
            var idx = line.IndexOf(':');
            if (idx <= 0) continue;
            if (line[..idx].Trim().Equals(name, StringComparison.OrdinalIgnoreCase))
                return line[(idx + 1)..].Trim();
        }
        return "";
    }

    private static int IndexOfHeaderEnd(byte[] buffer, int length)
    {
        for (var i = 0; i + 3 < length; i++)
            if (buffer[i] == 0x0D && buffer[i + 1] == 0x0A && buffer[i + 2] == 0x0D && buffer[i + 3] == 0x0A)
                return i;
        return -1;
    }
}
