using MollyPS.GameServer.Server;
using Proto;

namespace MollyPS.GameServer.Server.Packet.Send.Player;

 
public static class FactoryPackets
{
    private const string CurrentChapter = "domain_2";
    private static readonly string[] Chapters = ["domain_1", CurrentChapter];

    public static SC_FACTORY_SYNC BuildSync() => new()
    {
         
        FormulaMan = new SCD_FACTORY_SYNC_FORMULA_MAN(),
        Stt = new SCD_FACTORY_SYNC_STT(),
        ProgressStatus = new SCD_FACTORY_SYNC_PROGRESS_STATUS
        {
            RiftMgr = new SCD_RIFT_MGR(),
            SkipChapter = new SCD_FACTORY_SKIP_CHAPTER(),
        },
        Decorate = new SCD_FACTORY_DECORATE(),
    };

    public static SC_FACTORY_SYNC_SCOPE BuildScope() => new()
    {
        ScopeName = 1,
        CurrentChapterId = CurrentChapter,
        ActiveChapterIds = { Chapters },
        TransportRoute = new SCD_FACTORY_SYNC_HUB_TRANSPORT_ROUTE_MANAGER(),
        BookMark = new SCD_FACTORY_STATISTIC_BOOKMARK(),
        PanelStore = new SCD_FACTORY_SYNC_PANEL_STORE(),
        SignMgr = new SCD_FACTORY_SYNC_SIGN_MGR(),
        SharedMgr = new SCD_FACTORY_SYNC_SHARED_MGR(),
    };

    public static SC_FACTORY_SYNC_CHAPTER BuildChapter(string chapter) => new()
    {
        ChapterId = chapter,
        Blackboard = new SCD_FACTORY_SYNC_BLACKBOARD
        {
            Power = new SCD_FACTORY_SYNC_BLACKBOARD_POWER { IsStopByPower = true },
        },
        PinBoard = new SCD_FACTORY_SYNC_PIN_BOARD(),
        Statistic = new SCD_FACTORY_SYNC_STATISTIC
        {
            Other = new SCD_FACTORY_STATISTIC_OTHER(),
            LastDay = new SCD_FACTORY_STATISTIC_LASTDAY(),
        },
        PendingPlace = new SCD_FACTORY_SYNC_PENDING_PLACE(),
        InvalidPlacedMgr = new SCD_FACTORY_INVALID_PLACED_MGR(),
    };

    public static SC_FACTORY_HS BuildHeartbeat(string chapter) => new()
    {
        ChapterId = chapter,
        Blackboard = new SCD_FACTORY_HS_BB
        {
            Power = new SCD_FACTORY_HS_BB_POWER { IsStopByPower = true },
        },
    };

    public static void SendInitial(EndfieldConnection connection)
    {
        connection.Send((int)SCMessageID.ScFactorySync, BuildSync());
        connection.Send((int)SCMessageID.ScFactorySyncScope, BuildScope());
        foreach (var chapter in Chapters)
        {
            connection.Send((int)SCMessageID.ScFactorySyncChapter, BuildChapter(chapter));
            connection.Send((int)SCMessageID.ScFactoryHs, BuildHeartbeat(chapter));
        }
    }
}
