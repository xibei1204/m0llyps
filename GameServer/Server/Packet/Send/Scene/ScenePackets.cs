using MollyPS.GameServer.Game;
using MollyPS.GameServer.Game.Scene;
using MollyPS.Util;
using Proto;

namespace MollyPS.GameServer.Server.Packet.Send.Scene;


public static class ScenePackets
{
    private static readonly Logger Logger = new("ScenePackets");
     
     
    public static SC_SELF_SCENE_INFO BuildSelfInfo(PlayerInstance player, SELF_INFO_REASON_TYPE reason)
    {
        var team = player.Teams.GetValueOrDefault(player.CurTeamIndex);
        var info = new SC_SELF_SCENE_INFO
        {
            SceneNumId = player.SceneNumId,
            SelfInfoReason = (int)reason,
            Detail = new SCENE_OBJECT_DETAIL_CONTAINER(),
             
             
             
             
             
             
            Empty = new SCENE_IMPL_EMPTY(),
        };
        if (team != null)
        {
            info.TeamInfo = new SCD_SCENE_TEAM_INFO
            {
                TeamType = CHAR_BAG_TEAM_TYPE.Main,
                TeamIndex = player.CurTeamIndex,
                CurLeaderId = team.Leaderid,
            };
        }

        var pos = TeleportPackets.Position(player);
        var rot = TeleportPackets.Rotation(player);
        foreach (var objId in team?.CharTeam ?? [])
        {
            var charInfo = player.GetChar(objId);
            if (charInfo == null)
                continue;
            info.Detail.CharList.Add(new SCENE_CHARACTER
            {
                CommonInfo = new SCENE_OBJECT_COMMON_INFO
                {
                    Id = objId,
                    Type = (int)SceneObjectType.Character,
                    Templateid = charInfo.Templateid,
                    Position = pos,
                    Rotation = rot,
                    SceneNumId = player.SceneNumId,
                    Hp = (float)(charInfo.BattleInfo?.Hp ?? 100),
                },
                Level = charInfo.Level,
                BattleInfo = new BATTLE_MGR_INFO
                {
                    MsgGeneration = (uint)objId,
                    BattleInstId = (uint)objId,
                },
            });
        }

         
         
         
         
         
        var unlockLevels = AutoUnlockMap.Levels
            ? DataTable.GetAllMapLevelIds()
            : DataTable.GetLevel(player.SceneNumId) is { } only ? [only.Id] : [];
        info.UnlockArea.AddRange(unlockLevels);
        Logger.Info($"SelfSceneInfo: scene={player.SceneNumId} reason={reason} " + $"unlockArea=[{string.Join(",", unlockLevels)}]");
        return info;
    }

    public static SC_SCENE_MAP_MARK_SYNC BuildMapMarks(PlayerInstance player, int ceiling)
    {
        var marks = new SC_SCENE_MAP_MARK_SYNC();
        marks.SceneDynamicMapMarkList.AddRange(player.CustomMapMarks.Select(mark => mark.Clone()));
        foreach (var i in AnchorCatalog.MarkIndices.Take(Math.Max(0, ceiling)))
            marks.SceneStaticMapMarkList.Add(new SCENE_STATIC_MAP_MARK
            {
                Index = i
            });
         
        if (!string.IsNullOrEmpty(player.TrackingMarkInstId))
            marks.TrackPoint = new SCENE_TRACK_POINT
            {
                InstId = player.TrackingMarkInstId
            };
        return marks;
    }
}
