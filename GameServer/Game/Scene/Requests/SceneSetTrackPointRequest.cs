using MollyPS.GameServer.Server;
using MollyPS.GameServer.Server.Packet;
using MollyPS.Util;
using Proto;
using MollyPS.GameServer.Game;

namespace MollyPS.GameServer.Game.Scene;
public static class SceneSetTrackPointRequest
{
    private static readonly Logger Logger = new("MapMark");
    public static void Handle(EndfieldConnection connection, CS_SCENE_SET_TRACK_POINT cmd)
    {
        var player = connection.Player;
        if (player == null)
            return;
        player.TrackingMarkInstId = cmd.TrackPoint?.InstId;
        player.SaveToDatabase();
        Logger.Info($"Track point set: {player.TrackingMarkInstId ?? "<none>"}");
        connection.Send((int)SCMessageID.ScSceneSetTrackPoint, new SC_SCENE_SET_TRACK_POINT { TrackPoint = cmd.TrackPoint != null ? new SCENE_TRACK_POINT { InstId = cmd.TrackPoint.InstId } : new SCENE_TRACK_POINT(), });
    }
}
