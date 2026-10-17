using MollyPS.GameServer.Server;
using MollyPS.GameServer.Server.Packet;
using MollyPS.Util;
using Proto;
using MollyPS.GameServer.Game;
using MollyPS.GameServer.Game.Scene;
using MollyPS.GameServer.Server.Packet.Send.Player;
using MollyPS.GameServer.Server.Packet.Send.Scene;

namespace MollyPS.GameServer.Game.Scene;
public static class SceneTeleportRequest
{
    private static readonly Logger Logger = new("SceneTeleport");
    public static void Handle(EndfieldConnection connection, CS_SCENE_TELEPORT cmd)
    {
        var player = connection.Player;
        if (player == null)
            return;
        var sceneNumId = cmd.SceneNumId;
        var position = cmd.Position;
        var rotation = cmd.Rotation;
        if (cmd.SvrTpGotoCampDetail != null || !string.IsNullOrEmpty(cmd.TpPosId))
        {
            var campId = cmd.SvrTpGotoCampDetail?.CampId;
            var destination = campId.HasValue ? AnchorCatalog.CampDestination(campId.Value) : AnchorCatalog.Teleports.GetValueOrDefault(cmd.TpPosId);
            if (destination == null || destination.Reason != cmd.TeleportReason || campId.HasValue && !AnchorCatalog.Anchors.Contains(campId.Value) || !campId.HasValue && destination.Reason == 2 && (!cmd.TpPosId.StartsWith("TpForMap_ent_", StringComparison.Ordinal) || !ulong.TryParse(cmd.TpPosId[13..], out var namedCamp) || !AnchorCatalog.Anchors.Contains(namedCamp)))
            {
                Reject("unknown, inactive or mismatched teleport destination");
                return;
            }

            sceneNumId = checked((int)DataTable.GetLevelNumId(destination.SceneId));
            position = new VECTOR
            {
                X = destination.Position[0],
                Y = destination.Position[1],
                Z = destination.Position[2]
            };
            rotation = new VECTOR
            {
                X = destination.Rotation[0],
                Y = destination.Rotation[1],
                Z = destination.Rotation[2]
            };
            Logger.Info($"Resolved teleport: camp={campId} tpPosId={cmd.TpPosId} scene={sceneNumId} pos=({position.X:F3},{position.Y:F3},{position.Z:F3})");
        }
        else if (cmd.TeleportReason == 2)
        {
            Reject("ServerGotoCamp requires a known camp ID or tpPosId");
            return;
        }

        var target = DataTable.GetLevel(sceneNumId);
        if (target == null)
        {
             
             
             
            Reject($"unknown scene {sceneNumId}");
            return;
        }

        if (position != null && !new[]
        {
            position.X,
            position.Y,
            position.Z
        }.All(float.IsFinite) || rotation != null && !new[]
        {
            rotation.X,
            rotation.Y,
            rotation.Z
        }.All(float.IsFinite))
        {
            Reject("non-finite teleport coordinates");
            return;
        }

        if (position != null)
            player.ScenePosition = [position.X, position.Y, position.Z];
        else if (sceneNumId != player.SceneNumId)
            player.ScenePosition = [target.X, target.Y, target.Z];
        if (rotation != null)
            player.SceneRotation = [rotation.X, rotation.Y, rotation.Z];
        else if (sceneNumId != player.SceneNumId)
            player.SceneRotation = [target.RotX, target.RotY, target.RotZ];
        if (sceneNumId != player.SceneNumId)
        {
            var from = player.SceneNumId;
            Logger.Info($"Teleport crosses scenes {from} -> {sceneNumId}");
            player.SceneNumId = sceneNumId;
            player.SaveToDatabase();
             
             
            SceneService.BeginSceneChange(player, from, sceneNumId, new VECTOR { X = (float)player.ScenePosition[0], Y = (float)player.ScenePosition[1], Z = (float)player.ScenePosition[2] }, cmd.PassThroughData);
            return;
        }

        Logger.Info($"Teleport in scene {sceneNumId}: reason={cmd.TeleportReason} " + $"@ ({player.ScenePosition[0]:F1}, {player.ScenePosition[1]:F1}, {player.ScenePosition[2]:F1})");
        SendAck(connection, player, cmd, sceneNumId, PlayerPos(player), PlayerRot(player));
        player.SaveToDatabase();
        void Reject(string reason)
        {
            Logger.Warn($"Teleport rejected: {reason}; camp={cmd.SvrTpGotoCampDetail?.CampId} tpPosId={cmd.TpPosId}");
            connection.Send((int)SCMessageID.ScNtfErrorCode, new SC_NTF_ERROR_CODE { ErrorCode = (int)CODE.ErrSceneNameNotExist, Details = reason });
            SendAck(connection, player, cmd, player.SceneNumId, PlayerPos(player), PlayerRot(player));
        }
    }

    private static void SendAck(EndfieldConnection connection, PlayerInstance player, CS_SCENE_TELEPORT cmd, int sceneNumId, VECTOR pos, VECTOR rot)
    {
        var response = new SC_SCENE_TELEPORT
        {
            SceneNumId = sceneNumId,
            Position = pos,
            Rotation = rot,
             
             
             
            TpUuid = NextTpUuid(),
            ServerTime = (uint)(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() & 0xFFFFFFFF),
            TeleportReason = cmd.TeleportReason,
        };
         
         
        var team = player.Teams.GetValueOrDefault(player.CurTeamIndex);
        response.ObjIdList.AddRange(team?.CharTeam.Where(id => id != 0) ?? []);
         
        if (cmd.PassThroughData != null)
            response.PassThroughData = cmd.PassThroughData.Clone();
        connection.Send((int)SCMessageID.ScSceneTeleport, response);
    }

    private static VECTOR PlayerPos(PlayerInstance p) => new()
    {
        X = (float)p.ScenePosition[0],
        Y = (float)p.ScenePosition[1],
        Z = (float)p.ScenePosition[2],
    };
    private static VECTOR PlayerRot(PlayerInstance p) => new()
    {
        X = (float)p.SceneRotation[0],
        Y = (float)p.SceneRotation[1],
        Z = (float)p.SceneRotation[2],
    };
     


    public static ulong NextTpUuid() => TeleportPackets.NextTpUuid();
}
