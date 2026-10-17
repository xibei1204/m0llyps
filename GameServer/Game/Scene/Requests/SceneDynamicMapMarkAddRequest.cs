using MollyPS.GameServer.Server;
using MollyPS.GameServer.Server.Packet;
using MollyPS.Util;
using Proto;
using MollyPS.GameServer.Game;

namespace MollyPS.GameServer.Game.Scene;
public static class SceneDynamicMapMarkAddRequest
{
    private static readonly Logger Logger = new("MapMark");
    public static void Handle(EndfieldConnection connection, CS_SCENE_DYNAMIC_MAP_MARK_ADD cmd)
    {
        var player = connection.Player;
        if (player == null)
            return;
        if (cmd.Position == null || DataTable.GetLevel(cmd.SceneNumId) == null ||
            !float.IsFinite(cmd.Position.X) || !float.IsFinite(cmd.Position.Y) || !float.IsFinite(cmd.Position.Z))
            return;
        var id = checked(player.CustomMapMarks.Select(mark => mark.Id).DefaultIfEmpty(0u).Max() + 1);
        var mark = new SCENE_DYNAMIC_MAP_MARK
        {
            Id = id,
            SceneNumId = cmd.SceneNumId,
            Pos = cmd.Position,
            Note = cmd.Note ?? "",
            Typ = cmd.Typ,
            TierIndex = cmd.TierIndex,
            TierId = cmd.TierId,
        };
        player.CustomMapMarks.Add(mark);
        player.SaveToDatabase();
        Logger.Info($"Custom mark added: id={id} note={mark.Note}");
        connection.Send((int)SCMessageID.ScSceneDynamicMapMarkModify, new SC_SCENE_DYNAMIC_MAP_MARK_MODIFY { ModifiedMarks = { mark }, });
    }
}
