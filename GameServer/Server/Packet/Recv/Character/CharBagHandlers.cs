using MollyPS.GameServer.Game.Characters;
using MollyPS.GameServer.Server;
using MollyPS.GameServer.Server.Packet;
using Proto;

namespace MollyPS.GameServer.Server.Packet.Recv.Character;

 
 
[PacketOpcode((int)CSMessageID.CsCharBagSetTeam)]
public class CharBagSetTeamHandler : Handler<CS_CHAR_BAG_SET_TEAM>
{
    public override void OnHandle(EndfieldConnection connection, CS_CHAR_BAG_SET_TEAM cmd)
    {
        if (connection.Player is { } player)
            TeamService.CharBagSetTeam(player, cmd);
    }
}

 
[PacketOpcode((int)CSMessageID.CsCharBagSetCurrTeamIndex)]
public class CharBagSetCurrTeamIndexHandler : Handler<CS_CHAR_BAG_SET_CURR_TEAM_INDEX>
{
    public override void OnHandle(EndfieldConnection connection, CS_CHAR_BAG_SET_CURR_TEAM_INDEX cmd)
    {
        if (connection.Player is { } player)
            TeamService.CharBagSetCurrTeamIndex(player, cmd);
    }
}

 
[PacketOpcode((int)CSMessageID.CsCharBagSetTeamLeader)]
public class CharBagSetTeamLeaderHandler : Handler<CS_CHAR_BAG_SET_TEAM_LEADER>
{
    public override void OnHandle(EndfieldConnection connection, CS_CHAR_BAG_SET_TEAM_LEADER cmd)
    {
        if (connection.Player is { } player)
            TeamService.CharBagSetTeamLeader(player, cmd);
    }
}

 
[PacketOpcode((int)CSMessageID.CsCharBagSetTeamName)]
public class CharBagSetTeamNameHandler : Handler<CS_CHAR_BAG_SET_TEAM_NAME>
{
    public override void OnHandle(EndfieldConnection connection, CS_CHAR_BAG_SET_TEAM_NAME cmd)
    {
        if (connection.Player is { } player)
            TeamService.CharBagSetTeamName(player, cmd);
    }
}
