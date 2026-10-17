using Proto;
using SqlSugar;

namespace MollyPS.Database.Player;

[SugarTable("PlayerData")]
public sealed class PlayerData : BaseDatabaseDataHelper
{
    public string Name { get; set; } = "";
    public string ShortId { get; set; } = "";
    public int Gender { get; set; }
    public int Level { get; set; }
    public long CreateTs { get; set; }
    [SugarColumn(IsJson = true, IsNullable = true)]
    public byte[]? ClientSetting { get; set; }
    [SugarColumn(IsJson = true)]
    public Dictionary<int, TeamData> Teams { get; set; } = [];
    public int CurTeamIndex { get; set; }
    public int SceneNumId { get; set; }
    [SugarColumn(IsJson = true)]
    public double[] ScenePosition { get; set; } = [0, 0, 0];
    [SugarColumn(IsJson = true)]
    public double[] SceneRotation { get; set; } = [0, 0, 0];
     
    [SugarColumn(IsJson = true, IsNullable = true)]
    public List<CHAR_INFO> Characters { get; set; } = [];
    [SugarColumn(IsJson = true, IsNullable = true)]
    public List<SCENE_DYNAMIC_MAP_MARK> CustomMapMarks { get; set; } = [];
    public string TrackingMarkInstId { get; set; } = "";
    [SugarColumn(IsJson = true, IsNullable = true)]
    public CampCheckpointData? CampCheckpoint { get; set; }
    public bool ReceivedWelcomeMail { get; set; }
    [SugarColumn(IsJson = true, IsNullable = true)]
    public List<MailData> Mails { get; set; } = [];

    public sealed class TeamData
    {
        public string TeamName { get; set; } = "";
        public ulong Leaderid { get; set; }
        public List<ulong> CharTeam { get; set; } = [];
    }
}

 
public sealed class MailData
{
    public ulong Id { get; set; }
    public int MailSubType { get; set; }
    public string Title { get; set; } = "";
    public string Content { get; set; } = "";
    public string SenderName { get; set; } = "system";
    public long SendTime { get; set; }
    public long ExpireTime { get; set; }
    public bool IsRead { get; set; }
    public bool IsStar { get; set; }
    public bool RedDotRead { get; set; }
    public Dictionary<string, string> Params { get; set; } = [];
}
