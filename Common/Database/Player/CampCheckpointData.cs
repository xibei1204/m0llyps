namespace MollyPS.Database.Player;

public sealed class CampCheckpointData
{
    public ulong CampId { get; set; }
    public int SceneNumId { get; set; }
    public double[] Position { get; set; } = [0, 0, 0];
    public double[] Rotation { get; set; } = [0, 0, 0];
}
