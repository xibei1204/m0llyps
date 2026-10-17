using System.Net;
using System.Net.Sockets;
using MollyPS.Util;

namespace MollyPS.GameServer.Server;

public class Listener
{
    private static readonly Logger Logger = new("GameServer");
    private static TcpListener? _listener;
    private static Thread? _acceptThread;
    private static volatile bool _running;
    private static readonly List<EndfieldConnection> Connections = [];

    public static void Start()
    {
        if (_running) return;
        var port = ConfigManager.Config.Host.GameServerPort;
        var ip = ConfigManager.Config.Host.GameServerIp;
        _listener = new TcpListener(IPAddress.Parse(ip), port);
        _listener.Start();
        Logger.Info($"Game server listening on {ip}:{port} (TCP)");
        _running = true;
        _acceptThread = new Thread(AcceptLoop) { IsBackground = true, Name = "EndfieldListener" };
        _acceptThread.Start();
    }

    private static void AcceptLoop()
    {
        while (_running)
        {
            TcpClient client;
            try
            {
                client = _listener!.AcceptTcpClient();
            }
            catch (Exception e)
            {
                if (!_running) break;
                Logger.Error("Accept failed", e);
                Thread.Sleep(50);
                continue;
            }
            if (!_running)
            {
                client.Dispose();
                break;
            }
            var connection = new EndfieldConnection(client);
            client.NoDelay = true;  
            lock (Connections)
            {
                Connections.Add(connection);
            }
            connection.Start();
        }
    }

    public static void UnregisterConnection(EndfieldConnection connection)
    {
        lock (Connections)
        {
            Connections.Remove(connection);
        }
    }

    public static void StopAll()
    {
        _running = false;
        _listener?.Stop();
        _acceptThread?.Join();
        EndfieldConnection[] connections;
        lock (Connections)
        {
            connections = [.. Connections];
        }
        foreach (var connection in connections) connection.Stop();
        List<Exception> errors = [];
        foreach (var connection in connections)
        {
            try { connection.WaitForStop(); }
            catch (Exception exception) { errors.Add(exception); }
        }
        _listener = null;
        if (errors.Count > 0) throw new AggregateException("Failed to save player sessions.", errors);
    }
}
