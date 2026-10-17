using MollyPS.GameServer.Game;
using MollyPS.GameServer.Game.Mail;
using MollyPS.GameServer.Game.Scene;
using MollyPS.GameServer.Server.Packet.Send.Character;
using Proto;

namespace MollyPS.GameServer.Server.Packet.Send.Player;

 
public static class PlayerSyncPackets
{
     
     
    private static readonly int[] HudSystems = [0, 1, 2, 3, 4, 5, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, 62, 63, 64, 65, 66, 67, 69, 70, 71, 72, 75, 101, 102, 103, 104, 105, 106, 107, 108, 109, 110, 111, 112, 113, 114, 115, 116, 117, 118, 119, 120, 121, 122, 123, 124, 125, 126, 127, 128, 201, 202, 203, 204, 205, 251, 252, 253, 254, 301, 302, 303, 304, 305, 306, 307, 308, 309, 401, 402, 403, 404, 405, 406, 407, 501, 502, 503, 504, 511, 512, 601, 602, 603, 604, 605, 651, 652, 661, 670, 701, 801, 802, 1100, 1113, ];

    public static void SendFullSync(PlayerInstance player)
    {
        var connection = player.Connection;
        connection.Send((int)SCMessageID.ScSyncBaseData, new SC_SYNC_BASE_DATA
        {
            Roleid = player.Uid, RoleName = player.Name, Level = player.Level,
            Gender = player.Gender, ShortId = player.ShortId,
        });
        connection.Send((int)SCMessageID.ScSyncAllGameVar, BuildGameVars());
        connection.Send((int)SCMessageID.ScSyncAllUnlock,
            new SC_SYNC_ALL_UNLOCK { UnlockSystems = { HudSystems } });
        ClientInitializationPackets.Send(connection);
        connection.Send((int)SCMessageID.ScSyncCharBagInfo, CharacterPackets.Build(player));
        connection.Send((int)SCMessageID.ScSyncAllBitset, BuildBitsetSync());
        connection.Send((int)SCMessageID.ScBitsetAdd, new SC_BITSET_ADD
        {
            Type = 12, Value = { DataTable.GetAllLevelNumIds().Select(id => checked((uint)id)) },
        });
        connection.Send((int)SCMessageID.ScSyncAllRoleScene, BuildRoleSceneSync(player));
        connection.Send((int)SCMessageID.ScSyncAllMission, NavigationUnlocks.Build());
        MailService.CheckAndSendWelcomeMail(player);
        connection.Send((int)SCMessageID.ScSyncAllMail, MailService.BuildSyncAllMail(player));
         
        SceneService.SendEnterScene(player);
    }

    internal static SC_SYNC_ALL_ROLE_SCENE BuildRoleSceneSync(PlayerInstance player)
    {
        var sync = new SC_SYNC_ALL_ROLE_SCENE();
        foreach (var level in DataTable.GetAllMapLevelIds())
            sync.UnlockAreaInfo.Add(new AREA_UNLOCK_INFO
            {
                SceneId = level, UnlockAreaId = { DataTable.GetAllAreaIds() },
            });
        return sync;
    }

    public static SC_SYNC_ALL_BITSET BuildBitsetSync()
    {
        var sync = new SC_SYNC_ALL_BITSET();
        Add(12, DataTable.GetAllLevelNumIds());
        Add(13, DataTable.GetAllLevelNumIds());
        Add(6, DataTable.GetAllAreaNumIds());
        Add(62, MapUnlockCatalog.MistIds);
        Add(67, MapUnlockCatalog.MistIds);
        Add(39, AnchorCatalog.AnchorIds);
        return sync;

        void Add(int type, IEnumerable<long> ids) => sync.Bitset.Add(new BITSET_DATA
        {
            Type = type, Value = { PackBits(ids) },
        });
    }

    private static IEnumerable<ulong> PackBits(IEnumerable<long> ids)
    {
        var values = ids.Where(id => id >= 0).Distinct().ToArray();
        if (values.Length == 0) return [];
        var words = new ulong[checked((int)(values.Max() / 64) + 1)];
        foreach (var id in values) words[id / 64] |= 1UL << (int)(id % 64);
        return words;
    }

    private static SC_SYNC_ALL_GAME_VAR BuildGameVars()
    {
        var sync = new SC_SYNC_ALL_GAME_VAR();
        for (var id = 100000; id <= 100011; id++) sync.ServerVars[id] = 0;
        sync.ServerVars[100003] = 1;  
        sync.ServerVars[100006] = 100;  
        sync.ServerVars[100007] = 1;  
        sync.ServerVars[109999] = 0;
        return sync;
    }
}
