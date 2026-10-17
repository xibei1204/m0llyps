using MollyPS.GameServer.Game;
using MollyPS.GameServer.Server.Packet.Send.Player;
using MollyPS.Util;
using Proto;

namespace MollyPS.GameServer.Game.Player;


public static class PlayerProfileService
{
    private static readonly Logger Logger = new("PlayerProfileService");
    public static void SetClientSetting(PlayerInstance player, CS_SET_CLIENT_SETTING cmd)
    {
        if (cmd.ClientSettingModify != null)
            player.ClientSetting = cmd.ClientSettingModify.ToByteArray();
    }

    public static void SetName(PlayerInstance player, CS_SET_NAME cmd)
    {
        var connection = player.Connection;
        player.Name = cmd.Name;
        connection.Send((int)SCMessageID.ScSetName, new SC_SET_NAME
        {
            Name = player.Name,
            ShortId = player.ShortId,
        });
    }

    public static void SetGender(PlayerInstance player, CS_SET_GENDER cmd)
    {
        var connection = player.Connection;
        player.Gender = cmd.Gender;
        connection.Send((int)SCMessageID.ScSetGender, new SC_SET_GENDER
        {
            Gender = player.Gender,
            FirstSet = cmd.FirstSet,
        });
    }

    public static void CreateRole(PlayerInstance player, CS_CREATE_ROLE cmd)
    {
        var connection = player.Connection;
        if (!string.IsNullOrWhiteSpace(cmd.RoleName))
            player.Name = cmd.RoleName;
        if (cmd.Gender != GENDER.GenInvalid)
            player.Gender = cmd.Gender;
        Logger.Info($"Role created: {player.Name} gender={player.Gender}");
        connection.Send((int)SCMessageID.ScCreateRole, new SC_CREATE_ROLE());
        PlayerSyncPackets.SendFullSync(player);
    }
}
