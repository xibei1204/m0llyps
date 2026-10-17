using MollyPS.GameServer.Game;
using MollyPS.GameServer.Game.Scene;
using Proto;

namespace MollyPS.GameServer.Server.Packet.Send.Scene;


public static class TeleportPackets
{
    private static long _tpUuidCounter;
    public static SC_SCENE_TELEPORT BuildAck(PlayerInstance player, CS_SCENE_TELEPORT cmd, int sceneNumId, VECTOR pos, VECTOR rot)
    {
        var response = new SC_SCENE_TELEPORT
        {
            SceneNumId = sceneNumId,
            Position = pos,
            Rotation = rot,
             
             
             
            TpUuid = NextTpUuid(),
            ServerTime = (uint)(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() & 0xFFFFFFFF),
            TeleportReason = cmd.TeleportReason,
        };
         
         
        var team = player.Teams.GetValueOrDefault(player.CurTeamIndex);
        response.ObjIdList.AddRange(team?.CharTeam.Where(id => id != 0) ?? []);
         
        if (cmd.PassThroughData != null)
            response.PassThroughData = cmd.PassThroughData.Clone();
        return response;
    }

    public static VECTOR Position(PlayerInstance p) => new()
    {
        X = (float)p.ScenePosition[0],
        Y = (float)p.ScenePosition[1],
        Z = (float)p.ScenePosition[2],
    };
    public static VECTOR Rotation(PlayerInstance p) => new()
    {
        X = (float)p.SceneRotation[0],
        Y = (float)p.SceneRotation[1],
        Z = (float)p.SceneRotation[2],
    };
    public static ulong NextTpUuid() => unchecked((ulong)Interlocked.Increment(ref _tpUuidCounter));
}
