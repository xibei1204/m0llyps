using MollyPS.GameServer.Server;
using MollyPS.GameServer.Server.Packet;
using MollyPS.Util;
using Proto;
using MollyPS.GameServer.Game;

namespace MollyPS.GameServer.Game.Scene;
public static class SceneDynamicMapMarkDeleteRequest
{
    private static readonly Logger Logger = new("MapMark");
    public static void Handle(EndfieldConnection connection, CS_SCENE_DYNAMIC_MAP_MARK_DELETE cmd)
    {
        var player = connection.Player;
        if (player == null)
            return;
        if (cmd.Id.Count == 0)
            return;
         
         
         
        var modify = new SC_SCENE_DYNAMIC_MAP_MARK_MODIFY();
        foreach (var id in cmd.Id)
        {
            var mark = player.CustomMapMarks.FirstOrDefault(m => m.Id == id);
            if (mark == null)
                continue;
            player.CustomMapMarks.Remove(mark);
            modify.DeletedMarks.Add(id);
            Logger.Info($"Custom mark deleted: id={id}");
        }

        if (modify.DeletedMarks.Count > 0)
        {
            player.SaveToDatabase();
            connection.Send((int)SCMessageID.ScSceneDynamicMapMarkModify, modify);
        }
    }
}
