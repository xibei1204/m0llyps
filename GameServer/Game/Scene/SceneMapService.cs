using MollyPS.GameServer.Game;
using MollyPS.Util;
using Proto;

namespace MollyPS.GameServer.Game.Scene;


public static class SceneMapService
{
    private static readonly Logger Logger = new("SceneMapService");
    public static void MoveObjectMove(PlayerInstance player, CS_MOVE_OBJECT_MOVE cmd)
    {
        var connection = player.Connection;
        var team = player.Teams.GetValueOrDefault(player.CurTeamIndex);
        var leaderId = team?.Leaderid ?? 0;
        foreach (var mi in cmd.MoveInfo)
        {
            var motion = mi.MotionInfo;
            if (motion?.Position == null || mi.Objid != leaderId ||
                !float.IsFinite(motion.Position.X) || !float.IsFinite(motion.Position.Y) || !float.IsFinite(motion.Position.Z) ||
                motion.Rotation != null && (!float.IsFinite(motion.Rotation.X) || !float.IsFinite(motion.Rotation.Y) || !float.IsFinite(motion.Rotation.Z)))
                continue;
            player.ScenePosition = [motion.Position.X, motion.Position.Y, motion.Position.Z];
            if (motion.Rotation != null)
                player.SceneRotation = [motion.Rotation.X, motion.Rotation.Y, motion.Rotation.Z];
        }
    }

}
