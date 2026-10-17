using MollyPS.GameServer.Game;
using MollyPS.GameServer.Game.Scene;
using MollyPS.Util;
using Newtonsoft.Json.Linq;
using Proto;

namespace MollyPS.GameServer.Server.Packet.Send.Scene;


public static class SceneObjectPackets
{
    private static readonly Logger Logger = new("SceneObjects");
    private const int ObjectsPerMessage = 128;
     
     












    public static void SendVisibleObjects(PlayerInstance player)
    {
        var c = player.Connection;
        var entities = DataTable.GetWorldEntities(player.SceneNumId);
        if (entities.Count == 0)
            return;
        var container = new SCENE_OBJECT_DETAIL_CONTAINER();
        var npcs = new List<SCENE_NPC>();
        var sent = 0;
        foreach (var entity in entities)
        {
            var common = BuildCommon(entity, player.SceneNumId);
            if (common == null)
                continue;
            switch ((SceneObjectType)common.Type)
            {
                case SceneObjectType.Interactive:
                    if (!DataTable.IsAlwaysResident(entity.Value<string>("detailId")))
                        continue;
                    container.InteractiveList.Add(new SCENE_INTERACTIVE
                    {
                        CommonInfo = common,
                        OriginId = common.Id,
                    });
                    break;
                case SceneObjectType.Npc:
                    npcs.Add(new SCENE_NPC
                    {
                        CommonInfo = common
                    });
                    break;
                default:
                     
                     
                     
                    continue;
            }

            sent++;
            if (container.InteractiveList.Count + container.MonsterList.Count >= ObjectsPerMessage)
                Flush(ref container);
        }

        Flush(ref container);
        for (var i = 0; i < npcs.Count; i += ObjectsPerMessage)
        {
            var batch = new SCENE_OBJECT_DETAIL_CONTAINER();
            foreach (var n in npcs.Skip(i).Take(ObjectsPerMessage))
                batch.NpcList.Add(n);
            c.Send((int)SCMessageID.ScObjectEnterView, new SC_OBJECT_ENTER_VIEW
            {
                Detail = batch
            });
        }

        Logger.Info($"VisibleObjects: scene={player.SceneNumId} " + $"objects sent={sent} of {entities.Count} candidates");
        void Flush(ref SCENE_OBJECT_DETAIL_CONTAINER box)
        {
            if (box.InteractiveList.Count == 0 && box.NpcList.Count == 0)
                return;
            c.Send((int)SCMessageID.ScObjectEnterView, new SC_OBJECT_ENTER_VIEW
            {
                Detail = box
            });
            box = new SCENE_OBJECT_DETAIL_CONTAINER();
        }
    }

    private static SCENE_OBJECT_COMMON_INFO? BuildCommon(JObject entity, long sceneNumId)
    {
        var position = entity["position"];
        if (position == null)
            return null;
        var rotation = entity["rotation"];
        return new SCENE_OBJECT_COMMON_INFO
        {
            Id = ulong.TryParse(entity.Value<string>("instId") ?? "", out var iid) ? iid : 0,
            Type = entity.Value<int?>("entityType") ?? (int)SceneObjectType.Interactive,
            Templateid = entity.Value<string>("detailId") ?? "",
            Position = ToVector(position),
            Rotation = rotation != null ? ToVector(rotation) : new VECTOR(),
            SceneNumId = (int)sceneNumId,
            Hp = 1,
        };
    }

    private static VECTOR ToVector(JToken t) => new()
    {
        X = (float)t.Value<double>("x"),
        Y = (float)t.Value<double>("y"),
        Z = (float)t.Value<double>("z"),
    };
}
