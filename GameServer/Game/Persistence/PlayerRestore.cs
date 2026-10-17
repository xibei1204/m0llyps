using Proto;
using PlayerRecord = MollyPS.Database.Player.PlayerData;

namespace MollyPS.GameServer.Game.Persistence;

public static class PlayerRestore
{
    public static void Apply(PlayerInstance player, PlayerRecord data)
    {
        player.Name = data.Name;
        player.ShortId = data.ShortId;
        player.Gender = (GENDER)data.Gender;
        player.ClientSetting = data.ClientSetting?.ToArray();
        if (DataTable.GetLevel(data.SceneNumId) != null && ValidVector(data.ScenePosition) && ValidVector(data.SceneRotation))
        {
            player.SceneNumId = data.SceneNumId;
            player.ScenePosition = [.. data.ScenePosition];
            player.SceneRotation = [.. data.SceneRotation];
        }
        player.CurTeamIndex = data.CurTeamIndex;
         
        var idMap = (data.Characters ?? [])
            .Where(character => character.Objid != 0 && !string.IsNullOrEmpty(character.Templateid))
            .GroupBy(character => character.Objid)
            .ToDictionary(group => group.Key,
                group => player.ObjIdByTemplate.GetValueOrDefault(group.First().Templateid));
        var validIds = player.Chars.Select(character => character.Objid).ToHashSet();
        foreach (var (index, saved) in data.Teams ?? [])
        {
            if (index is < 0 or >= Characters.TeamService.TeamCount) continue;
            var team = new CHAR_TEAM_INFO { TeamName = saved.TeamName };
            foreach (var member in saved.CharTeam.Take(4))
            {
                var id = idMap.GetValueOrDefault(member, member);
                if (validIds.Contains(id) && !team.CharTeam.Contains(id)) team.CharTeam.Add(id);
            }
            var leader = idMap.GetValueOrDefault(saved.Leaderid, saved.Leaderid);
            team.Leaderid = team.CharTeam.Contains(leader) ? leader : team.CharTeam.FirstOrDefault();
            player.Teams[index] = team;
        }
        player.CustomMapMarks.AddRange((data.CustomMapMarks ?? [])
            .Where(mark => DataTable.GetLevel(mark.SceneNumId) != null && mark.Pos != null &&
                float.IsFinite(mark.Pos.X) && float.IsFinite(mark.Pos.Y) && float.IsFinite(mark.Pos.Z))
            .GroupBy(mark => mark.Id).Select(group => group.First().Clone()));
        player.TrackingMarkInstId = data.TrackingMarkInstId;
        PlayerFeatureState.Restore(player, data);
    }

    private static bool ValidVector(double[]? value) => value is { Length: 3 } && value.All(double.IsFinite);
}
