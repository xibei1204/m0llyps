using MollyPS.GameServer.Game.Player;
using MollyPS.GameServer.Game.Scene;
using MollyPS.GameServer.Server;
using MollyPS.GameServer.Server.Packet;
using MollyPS.Util;
using Proto;

namespace MollyPS.GameServer.Server.Packet.Recv.Common;
 
[PacketOpcode((int)CSMessageID.CsMoveObjectMove)]
public class MoveObjectMoveHandler : Handler<CS_MOVE_OBJECT_MOVE>
{
    public override void OnHandle(EndfieldConnection connection, CS_MOVE_OBJECT_MOVE cmd)
    {
        if (connection.Player is { } player)
            SceneMapService.MoveObjectMove(player, cmd);
    }
}

 

[PacketOpcode((int)CSMessageID.CsMergeMsg)]
public class MergeMsgHandler : Handler<CS_MERGE_MSG>
{
    private static readonly Logger Logger = new("MergeMsg");
    public override void OnHandle(EndfieldConnection connection, CS_MERGE_MSG cmd)
    {
        var data = cmd.Msg.ToByteArray();
        var off = 0;
        var count = 0;
        while (off + 3 <= data.Length)
        {
            int headLen = data[off];
            int bodyLen = data[off + 1] | (data[off + 2] << 8);
            off += 3;
            if (headLen == 0 || off + headLen + bodyLen > data.Length)
            {
                Logger.Warn($"merge frame overran (off={off} headLen={headLen} bodyLen={bodyLen} total={data.Length}) — inner framing may differ");
                break;
            }

            var headBytes = new byte[headLen];
            Array.Copy(data, off, headBytes, 0, headLen);
            CSHead head;
            try
            {
                head = CSHead.Parser.ParseFrom(headBytes);
            }
            catch (Exception e)
            {
                Logger.Warn($"merge CSHead parse failed: {e.Message}");
                break;
            }

            var body = new byte[bodyLen];
            Array.Copy(data, off + headLen, body, 0, bodyLen);
            off += headLen + bodyLen;
            count++;
            connection.DispatchInner(head.Msgid, head.UpSeqid, body);
        }

        Logger.Info($"merge unpacked {count} message(s) from {data.Length} bytes");
    }
}

 
 
 
[PacketOpcode((int)CSMessageID.CsGameTimeFreezeStartReq)]
public class GameTimeFreezeStartHandler : Handler<CS_GAME_TIME_FREEZE_START_REQ>
{
    public override void OnHandle(EndfieldConnection connection, CS_GAME_TIME_FREEZE_START_REQ cmd)
    {
        connection.Send((int)SCMessageID.ScGameTimeFreezeStartRsp, new SC_GAME_TIME_FREEZE_START_RSP { Reason = cmd.Reason, InTimeFreeze = true, });
    }
}

[PacketOpcode((int)CSMessageID.CsGameTimeFreezeEndReq)]
public class GameTimeFreezeEndHandler : Handler<CS_GAME_TIME_FREEZE_END_REQ>
{
    public override void OnHandle(EndfieldConnection connection, CS_GAME_TIME_FREEZE_END_REQ cmd)
    {
        connection.Send((int)SCMessageID.ScGameTimeFreezeEndRsp, new SC_GAME_TIME_FREEZE_END_RSP { Reason = cmd.Reason, InTimeFreeze = false, });
    }
}
