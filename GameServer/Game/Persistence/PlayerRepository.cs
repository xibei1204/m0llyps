using MollyPS.Database;
using PlayerRecord = MollyPS.Database.Player.PlayerData;

namespace MollyPS.GameServer.Game.Persistence;

public sealed class PlayerRepository(DatabaseHelper database) : IPlayerRepository
{
    public PlayerRecord? Load(ulong uid) => database.GetInstance<PlayerRecord>(unchecked((int)uid));

    public void Save(PlayerRecord snapshot) => database.InsertOrUpdate(snapshot);


}
