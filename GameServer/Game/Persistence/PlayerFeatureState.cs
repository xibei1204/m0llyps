using MollyPS.Database.Player;
using Newtonsoft.Json;
using Record = MollyPS.Database.Player.PlayerData;

namespace MollyPS.GameServer.Game.Persistence;

public static class PlayerFeatureState
{
    public static void Capture(PlayerInstance player, Record data)
    {
        data.ReceivedWelcomeMail = player.ReceivedWelcomeMail;
        data.Mails = Copy(player.Mails);
        data.CampCheckpoint = player.LastCampId == 0 ? null : new CampCheckpointData
        {
            CampId = player.LastCampId, SceneNumId = player.CheckpointSceneNumId,
            Position = [.. player.CheckpointPosition], Rotation = [.. player.CheckpointRotation],
        };
    }

    public static void Restore(PlayerInstance player, Record data)
    {
        player.ReceivedWelcomeMail = data.ReceivedWelcomeMail;
        player.Mails.AddRange(Copy(data.Mails ?? []));
        if (data.CampCheckpoint is { CampId: > 0 } camp &&
            AnchorCatalog.Anchors.Contains(camp.CampId) &&
            AnchorCatalog.Entities[camp.CampId].SceneId == DataTable.GetLevel(camp.SceneNumId)?.Id &&
            camp.Position is { Length: 3 } && camp.Rotation is { Length: 3 } &&
            camp.Position.All(double.IsFinite) && camp.Rotation.All(double.IsFinite))
        {
            player.LastCampId = camp.CampId;
            player.CheckpointSceneNumId = camp.SceneNumId;
            player.CheckpointPosition = [.. camp.Position];
            player.CheckpointRotation = [.. camp.Rotation];
        }
    }

    private static T Copy<T>(T value) => JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(value))
        ?? throw new InvalidDataException($"Cannot copy {typeof(T).Name}.");
}
