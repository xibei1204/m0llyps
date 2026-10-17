using MollyPS.GameServer.Game.Player;
using MollyPS.GameServer.Server;
using MollyPS.GameServer.Server.Packet;
using Proto;

namespace MollyPS.GameServer.Server.Packet.Recv.Player;


[PacketOpcode((int)CSMessageID.CsCheckName)]
public class CheckNameHandler : Handler<CS_CHECK_NAME>
{
    public override void OnHandle(EndfieldConnection connection, CS_CHECK_NAME cmd)
    {
        connection.Send((int)SCMessageID.ScCheckName, new SC_CHECK_NAME
        {
            Name = cmd.Name,
            Pass = !string.IsNullOrWhiteSpace(cmd.Name),
        });
    }
}

[PacketOpcode((int)CSMessageID.CsCheckSensitive)]
public class CheckSensitiveHandler : Handler<CS_CHECK_SENSITIVE>
{
    public override void OnHandle(EndfieldConnection connection, CS_CHECK_SENSITIVE cmd)
    {
         
    }
}

[PacketOpcode((int)CSMessageID.CsSetName)]
public class SetNameHandler : Handler<CS_SET_NAME>
{
    public override void OnHandle(EndfieldConnection connection, CS_SET_NAME cmd)
    {
        if (connection.Player is { } player)
            PlayerProfileService.SetName(player, cmd);
    }
}

[PacketOpcode((int)CSMessageID.CsSetGender)]
public class SetGenderHandler : Handler<CS_SET_GENDER>
{
    public override void OnHandle(EndfieldConnection connection, CS_SET_GENDER cmd)
    {
        if (connection.Player is { } player)
            PlayerProfileService.SetGender(player, cmd);
    }
}

[PacketOpcode((int)CSMessageID.CsCreateRole)]
public class CreateRoleHandler : Handler<CS_CREATE_ROLE>
{
    public override void OnHandle(EndfieldConnection connection, CS_CREATE_ROLE cmd)
    {
        if (connection.Player is { } player)
            PlayerProfileService.CreateRole(player, cmd);
    }
}
