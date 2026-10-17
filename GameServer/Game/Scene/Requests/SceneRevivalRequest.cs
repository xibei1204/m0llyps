using MollyPS.GameServer.Server;
using MollyPS.GameServer.Server.Packet;
using MollyPS.Util;
using Proto;
using MollyPS.GameServer.Game;
using MollyPS.GameServer.Game.Scene;
using MollyPS.GameServer.Server.Packet.Send.Player;
using MollyPS.GameServer.Server.Packet.Send.Scene;

namespace MollyPS.GameServer.Game.Scene;
public static class SceneRevivalRequest
{
    private static readonly Logger Logger = new("Revival");
    public static void Handle(EndfieldConnection connection, CS_SCENE_REVIVAL cmd)
    {
        var player = connection.Player;
        if (player == null)
            return;
        var c = connection;
        var team = player.Teams.GetValueOrDefault(player.CurTeamIndex);
        var reviveIds = new List<ulong>();
        foreach (var objId in team?.CharTeam ?? [])
        {
            var ch = player.GetChar(objId);
            if (ch == null || !ch.IsDead)
                continue;
            var(maxHp, maxSp) = DataTable.GetVitals(ch.Templateid);
            ch.IsDead = false;
            if (ch.BattleInfo != null)
            {
                ch.BattleInfo.Hp = maxHp / 2.0;  
                ch.BattleInfo.Ultimatesp = 0;
            }

            reviveIds.Add(objId);
            c.Send((int)SCMessageID.ScCharSyncStatus, new SC_CHAR_SYNC_STATUS { Objid = objId, IsDead = false, BattleInfo = new BATTLE_INFO { Hp = maxHp / 2.0, Ultimatesp = 0 }, });
        }

         
        MoveToCheckpoint(player);
        Logger.Info($"Revival: {reviveIds.Count} character(s) revived at checkpoint");
        SceneService.SendSelfSceneInfo(player, SELF_INFO_REASON_TYPE.SlrReviveDead);
        c.Send((int)SCMessageID.ScSceneRevival, new SC_SCENE_REVIVAL());
    }

     

    internal static void MoveToCheckpoint(PlayerInstance player)
    {
        if (player.CheckpointSceneNumId != 0 && player.CheckpointSceneNumId != player.SceneNumId)
        {
            player.SceneNumId = player.CheckpointSceneNumId;
        }

        if (player.LastCampId == 0)
        {
             
            if (DataTable.GetLevel(player.SceneNumId)is { } lvl)
            {
                player.ScenePosition = [lvl.X, lvl.Y, lvl.Z];
                player.SceneRotation = [lvl.RotX, lvl.RotY, lvl.RotZ];
            }
        }
        else
        {
            player.ScenePosition = [..player.CheckpointPosition];
            player.SceneRotation = [..player.CheckpointRotation];
        }
    }
}
