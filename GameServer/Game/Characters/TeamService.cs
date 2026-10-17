using MollyPS.GameServer.Game;
using MollyPS.GameServer.Game.Scene;
using MollyPS.Util;
using Proto;

namespace MollyPS.GameServer.Game.Characters;


public static class TeamService
{
    public const int TeamCount = 4;

    public static void EnsureValidTeams(PlayerInstance player)
    {
        for (var index = 0; index < TeamCount; index++)
            player.Teams.TryAdd(index, new CHAR_TEAM_INFO { TeamName = $"Team {index + 1}" });
        if (player.CurTeamIndex is < 0 or >= TeamCount) player.CurTeamIndex = 0;
        var team = player.Teams[player.CurTeamIndex];
        if (team.CharTeam.Count == 0)
        {
            var defaults = DataTable.DefaultTeam.Select(template => player.ObjIdByTemplate.GetValueOrDefault(template))
                .Where(id => id != 0).Distinct().Take(4).ToArray();
            team.CharTeam.AddRange(defaults.Length > 0 ? defaults : player.Chars.Take(4).Select(character => character.Objid));
        }
        if (!team.CharTeam.Contains(team.Leaderid)) team.Leaderid = team.CharTeam.FirstOrDefault();
    }

    private static bool ValidateIndex(PlayerInstance player, int index)
    {
        if (index is >= 0 and < TeamCount) return true;
        player.Connection.Send((int)SCMessageID.ScNtfErrorCode,
            new SC_NTF_ERROR_CODE { ErrorCode = (int)CODE.ErrSceneNameNotExist, Details = "Invalid team index" });
        return false;
    }

    private static readonly Logger Logger = new("TeamService");
    public static void CharBagSetTeam(PlayerInstance player, CS_CHAR_BAG_SET_TEAM cmd)
    {
        if (!ValidateIndex(player, cmd.TeamIndex)) return;
        var connection = player.Connection;
        var team = player.Teams.TryGetValue(cmd.TeamIndex, out var t) ? t : player.Teams[cmd.TeamIndex] = new CHAR_TEAM_INFO();
         
        if (cmd.UseEditBuffer) team = team.Clone();
         
         
         
         
        var validObjIds = player.Chars.Select(c => c.Objid).ToHashSet();
        team.CharTeam.Clear();
        var dropped = 0;
        foreach (var objId in cmd.CharTeam.Take(4))
        {
            if (!validObjIds.Contains(objId))
            {
                dropped++;
                continue;
            }

            if (!team.CharTeam.Contains(objId))
                team.CharTeam.Add(objId);
        }

        if (dropped > 0)
            Logger.Warn($"CS_CHAR_BAG_SET_TEAM idx={cmd.TeamIndex}: dropped {dropped} unknown char id(s) " + $"from {cmd.CharTeam.Count} submitted");
        team.Leaderid = cmd.LeaderId != 0 && team.CharTeam.Contains(cmd.LeaderId) ? cmd.LeaderId : (team.CharTeam.Count > 0 ? team.CharTeam[0] : 0);
        var resp = new SC_CHAR_BAG_SET_TEAM
        {
            TeamType = CHAR_BAG_TEAM_TYPE.Main,
            TeamIndex = cmd.TeamIndex,
            ScopeName = 1,
            LeaderId = team.Leaderid,
            UseEditBuffer = cmd.UseEditBuffer,  
        };
        resp.CharTeam.AddRange(team.CharTeam);
        connection.Send((int)SCMessageID.ScCharBagSetTeam, resp);
         
         
        if (!cmd.UseEditBuffer && cmd.TeamIndex == player.CurTeamIndex)
            SceneService.SendSelfSceneInfo(player, SELF_INFO_REASON_TYPE.SlrChangeTeam, sendVisibleObjects: false);
         
        if (!cmd.UseEditBuffer)
            player.SaveToDatabase();
    }

    public static void CharBagSetCurrTeamIndex(PlayerInstance player, CS_CHAR_BAG_SET_CURR_TEAM_INDEX cmd)
    {
        if (!ValidateIndex(player, cmd.TeamIndex)) return;
        var connection = player.Connection;
        player.CurTeamIndex = cmd.TeamIndex;
        EnsureValidTeams(player);
        connection.Send((int)SCMessageID.ScCharBagSetCurrTeamIndex, new SC_CHAR_BAG_SET_CURR_TEAM_INDEX
        {
            CurrTeamIndex = cmd.TeamIndex,
            ScopeName = 1,
        });
        SceneService.SendSelfSceneInfo(player, SELF_INFO_REASON_TYPE.SlrChangeTeam, sendVisibleObjects: false);
         
        player.SaveToDatabase();
    }

    public static void CharBagSetTeamLeader(PlayerInstance player, CS_CHAR_BAG_SET_TEAM_LEADER cmd)
    {
        if (!ValidateIndex(player, cmd.TeamIndex)) return;
        var connection = player.Connection;
        var leader = cmd.Leaderid;
        if (player.Teams.TryGetValue(cmd.TeamIndex, out var team))
        {
             
             
             
            if (team.CharTeam.Contains(leader))
            {
                team.Leaderid = leader;
            }
            else
            {
                Logger.Warn($"CS_CHAR_BAG_SET_TEAM_LEADER idx={cmd.TeamIndex}: leader {leader} is not a team member, keeping {team.Leaderid}");
                leader = team.Leaderid;
            }
        }

        connection.Send((int)SCMessageID.ScCharBagSetTeamLeader, new SC_CHAR_BAG_SET_TEAM_LEADER
        {
            TeamType = CHAR_BAG_TEAM_TYPE.Main,
            TeamIndex = cmd.TeamIndex,
            Leaderid = leader,
            ScopeName = 1,
        });
         
        player.SaveToDatabase();
    }

    public static void CharBagSetTeamName(PlayerInstance player, CS_CHAR_BAG_SET_TEAM_NAME cmd)
    {
        if (!ValidateIndex(player, cmd.TeamIndex)) return;
        var connection = player.Connection;
        if (player.Teams.TryGetValue(cmd.TeamIndex, out var team))
            team.TeamName = cmd.TeamName;
        connection.Send((int)SCMessageID.ScCharBagSetTeamName, new SC_CHAR_BAG_SET_TEAM_NAME
        {
            TeamIndex = cmd.TeamIndex,
            TeamName = cmd.TeamName,
            ScopeName = 1,
        });
         
        player.SaveToDatabase();
    }
}
