using MollyPS.GameServer.Game.Player;
using MollyPS.GameServer.Server;
using MollyPS.GameServer.Server.Packet;
using Proto;

namespace MollyPS.GameServer.Server.Packet.Recv.Common;


[PacketOpcode((int)CSMessageID.CsPing)]
public class PingHandler : Handler<CS_PING>
{
    public override void OnHandle(EndfieldConnection connection, CS_PING cmd)
    {
        connection.Send((int)SCMessageID.ScPing, new SC_PING
        {
            ClientTs = cmd.ClientTs,
            ServerTs = (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
        });
    }
}

[PacketOpcode((int)CSMessageID.CsFlushSync)]
public class FlushSyncHandler : Handler<CS_FLUSH_SYNC>
{
    public override void OnHandle(EndfieldConnection connection, CS_FLUSH_SYNC cmd)
    {
        connection.Send((int)SCMessageID.ScFlushSync, new SC_FLUSH_SYNC
        {
            ClientTs = cmd.ClientTs,
            ServerTs = (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
        });
    }
}

[PacketOpcode((int)CSMessageID.CsSyncLogicalTs)]
public class SyncLogicalTsHandler : Handler<CS_SYNC_LOGICAL_TS>
{
    public override void OnHandle(EndfieldConnection connection, CS_SYNC_LOGICAL_TS cmd)
    {
         
    }
}

[PacketOpcode((int)CSMessageID.CsSetClientSetting)]
public class SetClientSettingHandler : Handler<CS_SET_CLIENT_SETTING>
{
    public override void OnHandle(EndfieldConnection connection, CS_SET_CLIENT_SETTING cmd)
    {
        if (connection.Player is { } player)
            PlayerProfileService.SetClientSetting(player, cmd);
    }
}

[PacketOpcode((int)CSMessageID.CsLogout)]
public class LogoutHandler : Handler<CS_LOGOUT>
{
    public override void OnHandle(EndfieldConnection connection, CS_LOGOUT cmd)
    {
        connection.Stop();
    }
}
