using System.Runtime.InteropServices;
using MollyPS.Program.Hosting;
using MollyPS.Util;

namespace MollyPS.Program.Program;

public static class EntryPoint
{
    private static readonly Logger Logger = new("Program");

    public static async Task<int> Main(string[] args)
    {
        IConsole.InitConsole();
        using var shutdown = new CancellationTokenSource();
        ConsoleCancelEventHandler cancel = (_, eventArgs) =>
        {
            eventArgs.Cancel = true;
            shutdown.Cancel();
        };
        Console.CancelKeyPress += cancel;
        using var termination = OperatingSystem.IsWindows() ? null :
            PosixSignalRegistration.Create(PosixSignal.SIGTERM, context =>
            {
                context.Cancel = true;
                shutdown.Cancel();
            });

        try
        {
            ConfigManager.LoadConfig();
            var directory = Directory.CreateDirectory(ConfigManager.Config.Path.LogPath);
            Logger.SetLogFile(new FileInfo(Path.Combine(directory.FullName, "server.log")));
            Logger.SetDebugLogFile(new FileInfo(Path.Combine(directory.FullName, "server-debug.log")));

            Logger.Info("MollyPS starting...");

            using var runtime = new ServerRuntime();
            runtime.Start();
            Logger.Info($"Server started: {ConfigManager.Config.Host.GameServerIp}:" +
                        $"{ConfigManager.Config.Host.GameServerPort} (TCP). Type stop to shut down.");
            _ = Task.Run(() => ListenConsole(shutdown));
            try { await Task.Delay(Timeout.Infinite, shutdown.Token); }
            catch (OperationCanceledException) when (shutdown.IsCancellationRequested) { }
            Logger.Info("Stopping services and saving player sessions...");
            return 0;
        }
        catch (Exception exception)
        {
            Logger.Error("Server failed", exception);
            return 1;
        }
        finally
        {
            Console.CancelKeyPress -= cancel;
        }
    }

    private static void ListenConsole(CancellationTokenSource shutdown)
    {
        void Execute(string command)
        {
            if (shutdown.IsCancellationRequested) return;
            if (command.Trim().ToLowerInvariant() is "stop" or "exit" or "shutdown")
            {
                shutdown.Cancel();
                return;
            }
            Logger.Info("Available commands: stop, exit, shutdown");
            IConsole.RedrawInput(IConsole.Input);
        }

        IConsole.OnConsoleExcuteCommand += Execute;
        try { IConsole.ListenConsole(); }
        catch (Exception exception) { Logger.Error("Console input failed", exception); }
        finally { IConsole.OnConsoleExcuteCommand -= Execute; }
    }
}
