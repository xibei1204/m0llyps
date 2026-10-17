using MollyPS.GameServer.Server;
using MollyPS.GameServer.Server.Packet;
using MollyPS.Util;
using Proto;
using MollyPS.GameServer.Game;

namespace MollyPS.GameServer.Game.Scene;
public static class SceneRestRequest
{
    public static void Handle(EndfieldConnection connection, CS_SCENE_REST cmd)
    {
        if (connection.Player is not { } player)
            return;
        if (cmd.SceneNumId != player.SceneNumId || AnchorCatalog.CampDestination(cmd.CampId)is not { Reason: 2 } || !AnchorCatalog.Entities.TryGetValue(cmd.CampId, out var entity) || entity.SceneId != DataTable.GetLevel(player.SceneNumId)?.Id || !AnchorCatalog.Anchors.Contains(cmd.CampId))
        {
            connection.Send((int)SCMessageID.ScNtfErrorCode, new SC_NTF_ERROR_CODE { ErrorCode = (int)CODE.ErrSceneNameNotExist });
            return;
        }

        var count = 0;
        foreach (var id in player.Teams.GetValueOrDefault(player.CurTeamIndex)?.CharTeam ?? [])
        {
            if (player.GetChar(id)is not { } character)
                continue;
            var(hp, sp) = DataTable.GetVitals(character.Templateid);
            character.BattleInfo ??= new BATTLE_INFO();
            character.BattleInfo.Hp = hp;
            character.BattleInfo.Ultimatesp = (float)sp;
            character.IsDead = false;
            connection.Send((int)SCMessageID.ScCharSyncStatus, new SC_CHAR_SYNC_STATUS { Objid = id, IsDead = false, BattleInfo = character.BattleInfo.Clone() });
            count++;
        }

        player.SaveToDatabase();
        new Logger("CampRest").Info($"Camp rest: uid={player.Uid} camp={cmd.CampId}, healed {count} team characters");
    }
}
