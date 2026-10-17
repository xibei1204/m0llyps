using MollyPS.GameServer.Server;
using MollyPS.GameServer.Server.Packet;
using MollyPS.Util;
using Proto;

namespace MollyPS.GameServer.Server.Packet.Recv.Scene;
 
[PacketOpcode((int)CSMessageID.CsSceneRest)]
public sealed class SceneRestHandler : Handler<CS_SCENE_REST>
{
    public override void OnHandle(EndfieldConnection connection, CS_SCENE_REST cmd)
    {
        global::MollyPS.GameServer.Game.Scene.SceneRestRequest.Handle(connection, cmd);
    }
}
