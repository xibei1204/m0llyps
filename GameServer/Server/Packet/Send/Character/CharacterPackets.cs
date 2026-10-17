using MollyPS.GameServer.Game;
using Proto;

namespace MollyPS.GameServer.Server.Packet.Send.Character;


public static class CharacterPackets
{
    public static SC_SYNC_CHAR_BAG_INFO Build(PlayerInstance player)
    {
        var bag = new SC_SYNC_CHAR_BAG_INFO
        {
            CurrTeamIndex = player.CurTeamIndex,
            MaxCharTeamMemberCount = 4,
            ScopeName = 1,
            TeamType = CHAR_BAG_TEAM_TYPE.Main,
        };
        bag.CharInfo.AddRange(player.Chars);
         
         
        foreach (var kvp in player.Teams.OrderBy(t => t.Key))
        {
            bag.TeamInfo.Add(kvp.Value);
            bag.TeamRecords[kvp.Key] = kvp.Value;
        }

        return bag;
    }
}
