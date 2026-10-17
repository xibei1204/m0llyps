using MollyPS.Database;
using MollyPS.Database.Player;
using MollyPS.GameServer.Game.Characters;
using MollyPS.GameServer.Game.Persistence;
using MollyPS.Util;
using Proto;

namespace MollyPS.GameServer.Game;

 
public sealed class PlayerInstance
{
    private readonly IPlayerRepository _repository;

    public PlayerInstance(Server.EndfieldConnection connection, ulong uid, IPlayerRepository? repository = null)
    {
        Connection = connection;
        Uid = uid;
        _repository = repository ?? new PlayerRepository(DatabaseHelper.Instance ??
            throw new InvalidOperationException("Database is not initialized."));
        var saved = _repository.Load(uid);
        CreateTs = saved?.CreateTs ?? DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        Name = ConfigManager.Config.Game.DefaultPlayerName;
        ShortId = "1";
        Gender = GENDER.GenMale;
        Level = 40;
        SceneNumId = (int)DataTable.DefaultLevelNumId;
        ScenePosition = [.. DataTable.DefaultPosition];
        PlayerRoster.Populate(this);
        if (saved != null) PlayerRestore.Apply(this, saved);
        TeamService.EnsureValidTeams(this);
        SaveToDatabase();
    }

    public Server.EndfieldConnection Connection { get; }
    public ulong Uid { get; }
    public string Name { get; set; } = "";
    public string ShortId { get; set; } = "";
    public GENDER Gender { get; set; }
    public uint Level { get; set; }
    public long CreateTs { get; }
    public byte[]? ClientSetting { get; set; }
    public int SceneNumId { get; set; }
    public double[] ScenePosition { get; set; } = [0, 0, 0];
    public double[] SceneRotation { get; set; } = [0, 0, 0];
    public ulong LastCampId { get; set; }
    public int CheckpointSceneNumId { get; set; }
    public double[] CheckpointPosition { get; set; } = [0, 0, 0];
    public double[] CheckpointRotation { get; set; } = [0, 0, 0];
    public string? TrackingMarkInstId { get; set; }
    public int MapMarksSentForScene { get; set; } = -1;
    public int MapMarksSentCeiling { get; set; }
    public List<SCENE_DYNAMIC_MAP_MARK> CustomMapMarks { get; } = [];
    public List<CHAR_INFO> Chars { get; } = [];
    public Dictionary<string, ulong> ObjIdByTemplate { get; } = [];
    public Dictionary<int, CHAR_TEAM_INFO> Teams { get; } = [];
    public int CurTeamIndex { get; set; }
    public bool ReceivedWelcomeMail { get; set; }
    public List<MailData> Mails { get; } = [];

    public CHAR_INFO? GetChar(ulong objId) => Chars.FirstOrDefault(character => character.Objid == objId);
    public ulong NextMailId() => Math.Max((ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() * 1000UL,
        Mails.Select(mail => mail.Id).DefaultIfEmpty(0UL).Max() + 1);
    public void SaveToDatabase() => _repository.Save(PlayerSnapshot.Capture(this));
    public void OnLogout() => SaveToDatabase();
}
