using MollyPS.Util;

namespace MollyPS.GameServer.Game;

 
public static class AutoUnlockMap
{
    public static bool Levels => true;
    public static int AnchorCount => AnchorCatalog.MarkIndices.Count;
    public static void LogEffectiveSettings() => new Logger("Scenic").Info(
        $"All maps unlocked: {DataTable.GetAllMapLevelIds().Count} levels, " +
        $"{MapUnlockCatalog.MistIds.Count} fog regions, {AnchorCatalog.AnchorIds.Count} anchors");
}
