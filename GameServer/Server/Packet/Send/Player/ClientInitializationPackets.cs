using MollyPS.GameServer.Server;
using Proto;

namespace MollyPS.GameServer.Server.Packet.Send.Player;

 
internal static class ClientInitializationPackets
{
    public static void Send(EndfieldConnection connection)
    {
        var bag = new SC_ITEM_BAG_SCOPE_SYNC { ScopeName = 1, CurrentChapterId = "domain_2" };
        for (var depot = 1; depot <= 11; depot++)
            if (depot != 6) bag.Depot[depot] = new SCD_ITEM_DEPOT();
        bag.FactoryDepot["domain_2"] = new SCD_ITEM_DEPOT();
        connection.Send((int)SCMessageID.ScItemBagScopeSync, bag);
        connection.Send((int)SCMessageID.ScSyncWallet, new SC_SYNC_WALLET());
        connection.Send((int)SCMessageID.ScSyncFullDungeonStatus,
            new SC_SYNC_FULL_DUNGEON_STATUS { CurStamina = 200, MaxStamina = 200 });
        connection.Send((int)SCMessageID.ScDomainDevelopmentSystemSync, new SC_DOMAIN_DEVELOPMENT_SYSTEM_SYNC
        {
            Domains = { new DOMAIN_DEVELOPMENT { ChapterId = "domain_2", DevDegree = new DOMAIN_DEVELOPMENT_DEGREE { Level = 1 } } },
        });
        FactoryPackets.SendInitial(connection);
        connection.Send((int)SCMessageID.ScAdventureSyncAll,
            new SC_ADVENTURE_SYNC_ALL { Level = 60, WorldLevel = 7, UnlockWorldLevel = 7 });
        connection.Send((int)SCMessageID.ScAdventureBookSync, new SC_ADVENTURE_BOOK_SYNC { AdventureBookStage = 1 });
        connection.Send((int)SCMessageID.ScSyncAllMiniGame, new SC_SYNC_ALL_MINI_GAME());
        connection.Send((int)SCMessageID.ScKiteStationSyncAll, new SC_KITE_STATION_SYNC_ALL());
        connection.Send((int)SCMessageID.ScSyncAllGuide, new SC_SYNC_ALL_GUIDE());
        connection.Send((int)SCMessageID.ScGlobalEffectSyncAll, new SC_GLOBAL_EFFECT_SYNC_ALL());
        connection.Send((int)SCMessageID.ScSyncAllDoodadGroup, new SC_SYNC_ALL_DOODAD_GROUP());
        connection.Send((int)SCMessageID.ScSettlementSyncAll, new SC_SETTLEMENT_SYNC_ALL());
        connection.Send((int)SCMessageID.ScDomainDepotSyncAllInfo, new SC_DOMAIN_DEPOT_SYNC_ALL_INFO());
        connection.Send((int)SCMessageID.ScSyncAllDialog, new SC_SYNC_ALL_DIALOG());
        connection.Send((int)SCMessageID.ScSyncAllWiki, new SC_SYNC_ALL_WIKI());
        connection.Send((int)SCMessageID.ScRecycleBinSystemSyncAll, new SC_RECYCLE_BIN_SYSTEM_SYNC_ALL());
        connection.Send((int)SCMessageID.ScSyncAllStat, new SC_SYNC_ALL_STAT());
        connection.Send((int)SCMessageID.ScBpSyncAll, new SC_BP_SYNC_ALL());
        connection.Send((int)SCMessageID.ScSpaceshipSync, new SC_SPACESHIP_SYNC());
        connection.Send((int)SCMessageID.ScAchieveSync, new SC_ACHIEVE_SYNC());
        connection.Send((int)SCMessageID.ScFriendPersonalDataSync,
            new SC_FRIEND_PERSONAL_DATA_SYNC { Data = new FRIEND_PERSONAL_DATA() });
    }
}
