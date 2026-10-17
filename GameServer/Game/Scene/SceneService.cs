using MollyPS.GameServer.Game;
using MollyPS.GameServer.Server.Packet.Send.Player;
using MollyPS.GameServer.Server.Packet.Send.Scene;
using MollyPS.Util;
using Proto;

namespace MollyPS.GameServer.Game.Scene;

 























public static class SceneService
{
    private static readonly Logger Logger = new("Scene");
     
     




    public static void BeginSceneChange(PlayerInstance player, int sceneNumId, VECTOR position) => BeginSceneChange(player, player.SceneNumId, sceneNumId, position);
     





    public static void BeginSceneChange(PlayerInstance player, int fromSceneNumId, int toSceneNumId, VECTOR position, PASS_THROUGH_DATA? passThroughData = null)
    {
        var c = player.Connection;
        if (fromSceneNumId != toSceneNumId)
            c.Send((int)SCMessageID.ScLeaveSceneNotify, new SC_LEAVE_SCENE_NOTIFY
            {
                RoleId = player.Uid,
                SceneNumId = fromSceneNumId,
            });
        c.Send((int)SCMessageID.ScChangeSceneBeginNotify, new SC_CHANGE_SCENE_BEGIN_NOTIFY
        {
            SceneNumId = toSceneNumId,
            Position = position,
            PassThroughData = passThroughData?.Clone(),
        });
         
        c.Send((int)SCMessageID.ScSceneClientIdInfo, new SC_SCENE_CLIENT_ID_INFO
        {
            RoleIdx = 1,
            LastMaxIdx = 0,
        });
        c.Send((int)SCMessageID.ScEnterSceneNotify, new SC_ENTER_SCENE_NOTIFY
        {
            RoleId = player.Uid,
            SceneNumId = toSceneNumId,
            Position = position,
            PassThroughData = passThroughData?.Clone(),
        });
        Logger.Info($"Scene change begin: {fromSceneNumId} -> {toSceneNumId} " + $"@ ({position.X:F1}, {position.Y:F1}, {position.Z:F1})");
    }

     
    public static void SendEnterScene(PlayerInstance player) => BeginSceneChange(player, player.SceneNumId, TeleportPackets.Position(player));
     
     
    public static void SendSelfSceneInfo(PlayerInstance player, SELF_INFO_REASON_TYPE reason = SELF_INFO_REASON_TYPE.SlrEnterScene, bool sendVisibleObjects = true)
    {
        var info = ScenePackets.BuildSelfInfo(player, reason);
        player.Connection.Send((int)SCMessageID.ScSelfSceneInfo, info);
        SendRoleScene(player);
        SendMapMarks(player);
        if (sendVisibleObjects)
            SceneObjectPackets.SendVisibleObjects(player);
    }

     
     













    public static void SendMapMarks(PlayerInstance player)
    {
         
         
         
         
         
        var ceiling = AutoUnlockMap.AnchorCount;
        if (ceiling <= 0)
        {
            if (player.MapMarksSentForScene != 0)
            {
                player.MapMarksSentForScene = 0;
                player.MapMarksSentCeiling = 0;
            }

            return;
        }

         
         
         
        if (player.MapMarksSentForScene == player.SceneNumId && player.MapMarksSentCeiling == ceiling && !string.IsNullOrEmpty(player.TrackingMarkInstId))
            return;
        var marks = ScenePackets.BuildMapMarks(player, ceiling);
        player.Connection.Send((int)SCMessageID.ScSceneMapMarkSync, marks);
        player.MapMarksSentForScene = player.SceneNumId;
        player.MapMarksSentCeiling = ceiling;
        Logger.Info($"MapMarkSync: revealed {marks.SceneStaticMapMarkList.Count} configured static marks");
    }

     






    private static void SendRoleScene(PlayerInstance player)
    {
        player.Connection.Send((int)SCMessageID.ScSyncAllRoleScene, PlayerSyncPackets.BuildRoleSceneSync(player));
    }
}
