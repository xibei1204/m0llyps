using MollyPS.GameServer.Server;
using MollyPS.GameServer.Server.Packet;
using MollyPS.Util;
using Proto;
using MollyPS.GameServer.Game;

namespace MollyPS.GameServer.Game.Scene;
public static class SceneSetSafeZoneRequest
{
    public static void Handle(EndfieldConnection connection, CS_SCENE_SET_SAFE_ZONE cmd)
    {
        connection.Send((int)SCMessageID.ScSceneSetSafeZone, new SC_SCENE_SET_SAFE_ZONE { InZone = cmd.InZone, Id = cmd.Id, });
    }
}
