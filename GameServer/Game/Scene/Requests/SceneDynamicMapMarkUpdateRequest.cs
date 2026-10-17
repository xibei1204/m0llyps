using MollyPS.GameServer.Server;
using MollyPS.GameServer.Server.Packet;
using MollyPS.Util;
using Proto;
using MollyPS.GameServer.Game;

namespace MollyPS.GameServer.Game.Scene;
public static class SceneDynamicMapMarkUpdateRequest
{
    private static readonly Logger Logger = new("MapMark");
    public static void Handle(EndfieldConnection connection, CS_SCENE_DYNAMIC_MAP_MARK_UPDATE cmd)
    {
        var player = connection.Player;
        if (player == null)
            return;
        var mark = player.CustomMapMarks.FirstOrDefault(m => m.Id == cmd.Id);
        if (mark == null)
        {
            Logger.Warn($"Custom mark update for unknown id={cmd.Id}");
            return;
        }

        mark.Note = cmd.Note ?? mark.Note;
        mark.Typ = cmd.Typ != 0 ? cmd.Typ : mark.Typ;
        player.SaveToDatabase();
        Logger.Info($"Custom mark updated: id={cmd.Id} note={mark.Note}");
        connection.Send((int)SCMessageID.ScSceneDynamicMapMarkModify, new SC_SCENE_DYNAMIC_MAP_MARK_MODIFY { ModifiedMarks = { mark.Clone() }, });
    }
}
