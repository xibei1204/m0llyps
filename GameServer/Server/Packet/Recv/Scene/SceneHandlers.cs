using MollyPS.GameServer.Game.Scene;
using MollyPS.GameServer.Server;
using MollyPS.GameServer.Server.Packet;
using Proto;
using MollyPS.GameServer.Game;

namespace MollyPS.GameServer.Server.Packet.Recv.Scene;
 



[PacketOpcode((int)CSMessageID.CsSceneLoadFinish)]
public class SceneLoadFinishHandler : Handler<CS_SCENE_LOAD_FINISH>
{
    public override void OnHandle(EndfieldConnection connection, CS_SCENE_LOAD_FINISH cmd)
    {
        if (connection.Player == null)
            return;
        SceneService.SendSelfSceneInfo(connection.Player);
        connection.Player.SaveToDatabase();
    }
}

 



[PacketOpcode((int)CSMessageID.CsSceneRepatriate)]
public class SceneRepatriateHandler : Handler<CS_SCENE_REPATRIATE>
{
    public override void OnHandle(EndfieldConnection connection, CS_SCENE_REPATRIATE cmd)
    {
        global::MollyPS.GameServer.Game.Scene.SceneRepatriateRequest.Handle(connection, cmd);
    }
}
