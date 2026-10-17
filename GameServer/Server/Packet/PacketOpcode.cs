namespace MollyPS.GameServer.Server.Packet;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class PacketOpcode(int cmdId) : Attribute
{
    public int CmdId { get; } = cmdId;
}
