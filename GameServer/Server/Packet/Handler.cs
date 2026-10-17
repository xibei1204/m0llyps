using System.Reflection;
using Google.Protobuf;

namespace MollyPS.GameServer.Server.Packet;

public abstract class Handler
{
    public virtual int CmdId => GetType().GetCustomAttribute<PacketOpcode>()?.CmdId ?? 0;

    public abstract void OnHandle(EndfieldConnection connection, byte[] data);
}

public abstract class Handler<T> : Handler where T : IMessage<T>, new()
{
    private static readonly MessageParser<T> Parser = new(() => new T());

    public override void OnHandle(EndfieldConnection connection, byte[] data)
    {
        var cmd = Parser.ParseFrom(data);
        OnHandle(connection, cmd);
    }

    public abstract void OnHandle(EndfieldConnection connection, T cmd);
}
