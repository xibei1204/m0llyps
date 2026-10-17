using PlayerRecord = MollyPS.Database.Player.PlayerData;

namespace MollyPS.GameServer.Game.Persistence;

 
public static class PlayerSnapshot
{
    public static PlayerRecord Capture(PlayerInstance player)
    {
        var data = new PlayerRecord
        {
            Uid = unchecked((int)player.Uid), Name = player.Name, ShortId = player.ShortId,
            Gender = (int)player.Gender, Level = (int)player.Level, CreateTs = player.CreateTs,
            ClientSetting = player.ClientSetting?.ToArray(), SceneNumId = player.SceneNumId,
            ScenePosition = [.. player.ScenePosition], SceneRotation = [.. player.SceneRotation],
            CurTeamIndex = player.CurTeamIndex,
            TrackingMarkInstId = player.TrackingMarkInstId ?? "",
        };
        foreach (var (index, team) in player.Teams)
            data.Teams[index] = new PlayerRecord.TeamData
            {
                TeamName = team.TeamName, Leaderid = team.Leaderid, CharTeam = [.. team.CharTeam]
            };
        data.Characters.AddRange(player.Chars.Select(character => character.Clone()));
        data.CustomMapMarks.AddRange(player.CustomMapMarks.Select(mark => mark.Clone()));
        PlayerFeatureState.Capture(player, data);
        return data;
    }
}
