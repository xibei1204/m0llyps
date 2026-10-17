using PlayerRecord = MollyPS.Database.Player.PlayerData;

namespace MollyPS.GameServer.Game.Persistence;

public interface IPlayerRepository
{
    PlayerRecord? Load(ulong uid);
    void Save(PlayerRecord snapshot);
}
