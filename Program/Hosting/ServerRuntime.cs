using MollyPS.Database;
using MollyPS.GameServer.Game;
using MollyPS.GameServer.Server;
using MollyPS.GameServer.Server.Packet;
using MollyPS.Util;
using MollyPS.WebServer;
using DispatchServer = MollyPS.WebServer.WebServer;

namespace MollyPS.Program.Hosting;

 
internal sealed class ServerRuntime : IDisposable
{
    private static readonly Logger Logger = new("Runtime");
    private readonly Stack<Action> _shutdown = new();

    public void Start()
    {
        var database = new DatabaseHelper();
        StartService(database.Initialize, database.Dispose);
        DataTable.Load();
        HandlerManager.Init();
        AutoUnlockMap.LogEffectiveSettings();

        StartService(Listener.Start, Listener.StopAll);

        var config = ConfigManager.Config;
        if (config.HttpServer.Enable)
        {
            var dispatch = new DispatchServer(config.HttpServer.Port);
            StartService(dispatch.Start, dispatch.Dispose);
            if (config.HttpServer.Port != 10001)
            {
                var legacyConfig = new DispatchServer(10001, enableHttps: false);
                StartService(legacyConfig.Start, legacyConfig.Dispose);
            }
        }
    }

    private void StartService(Action start, Action stop)
    {
        _shutdown.Push(stop);
        start();
    }

    public void Dispose()
    {
        List<Exception> errors = [];
        while (_shutdown.TryPop(out var stop))
        {
            try { stop(); }
            catch (Exception exception)
            {
                Logger.Error("Service shutdown failed", exception);
                errors.Add(exception);
            }
        }
        if (errors.Count > 0) throw new AggregateException("Service shutdown failed.", errors);
    }
}
