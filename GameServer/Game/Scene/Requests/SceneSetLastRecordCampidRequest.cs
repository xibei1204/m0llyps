using MollyPS.GameServer.Server;
using MollyPS.GameServer.Server.Packet;
using MollyPS.Util;
using Proto;
using MollyPS.GameServer.Game;

namespace MollyPS.GameServer.Game.Scene;
public static class SceneSetLastRecordCampidRequest
{
    private static readonly Logger Logger = new("Checkpoint");
    public static void Handle(EndfieldConnection connection, CS_SCENE_SET_LAST_RECORD_CAMPID cmd)
    {
        var player = connection.Player;
        if (player == null)
            return;
         
         
        if (cmd.SceneNumId == player.SceneNumId && AnchorCatalog.Anchors.Contains(cmd.LastCampId) && AnchorCatalog.Entities[cmd.LastCampId].SceneId == DataTable.GetLevel(player.SceneNumId)?.Id && (cmd.Position == null || float.IsFinite(cmd.Position.X) && float.IsFinite(cmd.Position.Y) && float.IsFinite(cmd.Position.Z)) && (cmd.Rotation == null || float.IsFinite(cmd.Rotation.X) && float.IsFinite(cmd.Rotation.Y) && float.IsFinite(cmd.Rotation.Z)))
        {
            player.LastCampId = cmd.LastCampId;
            player.CheckpointSceneNumId = cmd.SceneNumId;
            player.CheckpointPosition = cmd.Position != null ? [cmd.Position.X, cmd.Position.Y, cmd.Position.Z] : [..player.ScenePosition];
            player.CheckpointRotation = cmd.Rotation != null ? [cmd.Rotation.X, cmd.Rotation.Y, cmd.Rotation.Z] : [..player.SceneRotation];
            player.SaveToDatabase();
            Logger.Info($"Checkpoint set: camp={cmd.LastCampId} @ ({cmd.Position?.X:F1}, " + $"{cmd.Position?.Y:F1}, {cmd.Position?.Z:F1})");
        }
        else
        {
            Logger.Warn($"Invalid/stale campfire checkpoint ignored: camp={cmd.LastCampId} " + $"scene={cmd.SceneNumId} but player is in {player.SceneNumId}");
        }

         
        connection.Send((int)SCMessageID.ScSceneSetLastRecordCampid, new SC_SCENE_SET_LAST_RECORD_CAMPID { LastCampId = player.LastCampId, SceneNumId = player.SceneNumId, });
    }
}
