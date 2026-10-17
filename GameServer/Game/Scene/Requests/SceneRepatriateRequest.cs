using MollyPS.GameServer.Game;
using MollyPS.GameServer.Server;
using MollyPS.GameServer.Server.Packet;
using Proto;
using MollyPS.GameServer.Game.Scene;
using MollyPS.GameServer.Server.Packet.Send.Player;
using MollyPS.GameServer.Server.Packet.Send.Scene;

namespace MollyPS.GameServer.Game.Scene;
public static class SceneRepatriateRequest
{
    public static void Handle(EndfieldConnection connection, CS_SCENE_REPATRIATE cmd)
    {
        var player = connection.Player;
        if (player == null)
            return;
        if (player.SceneNumId != cmd.SceneNumId)
            return;
        var previousScene = player.SceneNumId;
        SceneRevivalRequest.MoveToCheckpoint(player);
        player.SaveToDatabase();
        SceneService.BeginSceneChange(player, previousScene, player.SceneNumId, new VECTOR { X = (float)player.ScenePosition[0], Y = (float)player.ScenePosition[1], Z = (float)player.ScenePosition[2], });
    }
}
