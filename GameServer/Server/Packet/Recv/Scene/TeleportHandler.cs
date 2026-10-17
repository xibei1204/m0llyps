using MollyPS.GameServer.Game.Scene;
using MollyPS.GameServer.Server;
using MollyPS.GameServer.Server.Packet;
using Proto;
using MollyPS.Util;

namespace MollyPS.GameServer.Server.Packet.Recv.Scene;
 


























[PacketOpcode((int)CSMessageID.CsSceneTeleport)]
public class SceneTeleportHandler : Handler<CS_SCENE_TELEPORT>
{
    public override void OnHandle(EndfieldConnection connection, CS_SCENE_TELEPORT cmd)
    {
        global::MollyPS.GameServer.Game.Scene.SceneTeleportRequest.Handle(connection, cmd);
    }
}
