using MollyPS.GameServer.Server;
using MollyPS.GameServer.Server.Packet;
using MollyPS.Util;
using Proto;
using MollyPS.GameServer.Game;

namespace MollyPS.GameServer.Game.Scene;
public static class SceneTeleportFinishRequest
{
    private static readonly Logger Logger = new("SceneTeleport");
    public static void Handle(EndfieldConnection connection, CS_SCENE_TELEPORT_FINISH cmd)
    {
        Logger.Info($"Teleport finished: tpUuid={cmd.TpUuid} uid={connection.Player?.Uid}");
    }
}
