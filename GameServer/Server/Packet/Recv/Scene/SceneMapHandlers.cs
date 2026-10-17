using MollyPS.GameServer.Game.Scene;
using MollyPS.GameServer.Server;
using MollyPS.GameServer.Server.Packet;
using MollyPS.Util;
using Proto;

namespace MollyPS.GameServer.Server.Packet.Recv.Scene;
 
 
 
 
 
 
 
 
 
 
 


[PacketOpcode((int)CSMessageID.CsSceneTeleportFinish)]
public class SceneTeleportFinishHandler : Handler<CS_SCENE_TELEPORT_FINISH>
{
    public override void OnHandle(EndfieldConnection connection, CS_SCENE_TELEPORT_FINISH cmd)
    {
        global::MollyPS.GameServer.Game.Scene.SceneTeleportFinishRequest.Handle(connection, cmd);
    }
}

 


[PacketOpcode((int)CSMessageID.CsSceneSetTrackPoint)]
public class SceneSetTrackPointHandler : Handler<CS_SCENE_SET_TRACK_POINT>
{
    public override void OnHandle(EndfieldConnection connection, CS_SCENE_SET_TRACK_POINT cmd)
    {
        global::MollyPS.GameServer.Game.Scene.SceneSetTrackPointRequest.Handle(connection, cmd);
    }
}

 


[PacketOpcode((int)CSMessageID.CsSceneDynamicMapMarkAdd)]
public class SceneDynamicMapMarkAddHandler : Handler<CS_SCENE_DYNAMIC_MAP_MARK_ADD>
{
    public override void OnHandle(EndfieldConnection connection, CS_SCENE_DYNAMIC_MAP_MARK_ADD cmd)
    {
        global::MollyPS.GameServer.Game.Scene.SceneDynamicMapMarkAddRequest.Handle(connection, cmd);
    }
}

[PacketOpcode((int)CSMessageID.CsSceneDynamicMapMarkUpdate)]
public class SceneDynamicMapMarkUpdateHandler : Handler<CS_SCENE_DYNAMIC_MAP_MARK_UPDATE>
{
    public override void OnHandle(EndfieldConnection connection, CS_SCENE_DYNAMIC_MAP_MARK_UPDATE cmd)
    {
        global::MollyPS.GameServer.Game.Scene.SceneDynamicMapMarkUpdateRequest.Handle(connection, cmd);
    }
}

[PacketOpcode((int)CSMessageID.CsSceneDynamicMapMarkDelete)]
public class SceneDynamicMapMarkDeleteHandler : Handler<CS_SCENE_DYNAMIC_MAP_MARK_DELETE>
{
    public override void OnHandle(EndfieldConnection connection, CS_SCENE_DYNAMIC_MAP_MARK_DELETE cmd)
    {
        global::MollyPS.GameServer.Game.Scene.SceneDynamicMapMarkDeleteRequest.Handle(connection, cmd);
    }
}

 

[PacketOpcode((int)CSMessageID.CsSceneSetLastRecordCampid)]
public class SceneSetLastRecordCampidHandler : Handler<CS_SCENE_SET_LAST_RECORD_CAMPID>
{
    public override void OnHandle(EndfieldConnection connection, CS_SCENE_SET_LAST_RECORD_CAMPID cmd)
    {
        global::MollyPS.GameServer.Game.Scene.SceneSetLastRecordCampidRequest.Handle(connection, cmd);
    }
}

 

[PacketOpcode((int)CSMessageID.CsSceneSetSafeZone)]
public class SceneSetSafeZoneHandler : Handler<CS_SCENE_SET_SAFE_ZONE>
{
    public override void OnHandle(EndfieldConnection connection, CS_SCENE_SET_SAFE_ZONE cmd)
    {
        global::MollyPS.GameServer.Game.Scene.SceneSetSafeZoneRequest.Handle(connection, cmd);
    }
}

 





[PacketOpcode((int)CSMessageID.CsSceneRevival)]
public class SceneRevivalHandler : Handler<CS_SCENE_REVIVAL>
{
    public override void OnHandle(EndfieldConnection connection, CS_SCENE_REVIVAL cmd)
    {
        global::MollyPS.GameServer.Game.Scene.SceneRevivalRequest.Handle(connection, cmd);
    }
}
