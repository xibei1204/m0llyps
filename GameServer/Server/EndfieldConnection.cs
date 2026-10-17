using System.Net.Sockets;
using MollyPS.GameServer.Game;
using MollyPS.GameServer.Server.Packet;
using MollyPS.Util;
using Google.Protobuf;
using Proto;

namespace MollyPS.GameServer.Server;

 












public partial class EndfieldConnection(TcpClient client) : IDisposable
{
    private static readonly Logger Logger = new("GameServer");

    private readonly NetworkStream _stream = client.GetStream();
    private readonly object _sendLock = new();
    private Thread? _receiveThread;
    private int _stopRequested;
    private int _finalized;
    private Exception? _shutdownError;

    private const int SocketPollMs = 250;
    private const int FrameAssemblyTimeoutMs = 30000;

    private ChaCha20? _encryptor;
    private ChaCha20? _decryptor;

    private ulong _lastClientUpSeqId;
    private volatile bool _closed;
    public bool IsClosed => _closed;

    public PlayerInstance? Player
    {
        get; set;
    }

    public void Start()
    {
        Logger.Info($"New connection from {client.Client.RemoteEndPoint} t={Environment.TickCount64}.");
        _receiveThread = new Thread(ReceiveLoop) { IsBackground = true, Name = "EndfieldSession" };
        _receiveThread.Start();
    }

    public void Stop()
    {
        if (Interlocked.Exchange(ref _stopRequested, 1) != 0) return;
        _closed = true;
        client.Close();
    }

    public void WaitForStop()
    {
        if (_receiveThread != null && _receiveThread != Thread.CurrentThread)
            _receiveThread.Join();
        if (_shutdownError != null)
            throw new IOException($"Player {Player?.Uid} could not be saved during shutdown.", _shutdownError);
    }

    private void FinalizeSession()
    {
        if (Interlocked.Exchange(ref _finalized, 1) != 0) return;
        Stop();
        try
        {
            Player?.OnLogout();
        }
        catch (Exception e)
        {
            _shutdownError = e;
            Logger.Error("OnLogout failed", e);
        }
        Listener.UnregisterConnection(this);
    }

    private void ReceiveLoop()
    {
        try
        {
            while (!_closed)
            {
                var prefix = new byte[3];
                if (!ReadExact(prefix, 3)) break;
                var headLen = prefix[0];
                var bodyLen = prefix[1] | (prefix[2] << 8);
                if (headLen == 0)
                {
                    Logger.Error("Invalid packet: headLen == 0");
                    break;
                }

                var payload = new byte[headLen + bodyLen];
                if (!ReadExact(payload, payload.Length)) break;

                if (_decryptor != null)
                    _decryptor.Transform(payload);

                var head = new byte[headLen];
                Buffer.BlockCopy(payload, 0, head, 0, headLen);
                var body = new byte[bodyLen];
                Buffer.BlockCopy(payload, headLen, body, 0, bodyLen);

                CSHead csHead;
                try
                {
                    csHead = CSHead.Parser.ParseFrom(head);
                }
                catch (Exception e)
                {
                    Logger.Error($"Failed to parse CSHead (headLen={headLen})", e);
                    break;
                }

                if (body.Length > 0)
                {
                    var expected = Crypto.Crc32(body, 0, body.Length);
                    if (expected != csHead.Checksum)
                        Logger.Warn($"checksum mismatch on msgid {csHead.Msgid}: got 0x{csHead.Checksum:x}, expected 0x{expected:x}");

                }

                _lastClientUpSeqId = csHead.UpSeqid;
                Dispatch(csHead, body);
            }
        }
        catch (Exception e)
        {
            if (!_closed)
                Logger.Error("Connection error", e);
        }
        finally
        {
            FinalizeSession();
        }
    }

    private bool ReadExact(byte[] buffer, int length)
    {
        var read = 0;
        var idleDeadline = Environment.TickCount64 + SocketPollMs;
        var hardDeadline = Environment.TickCount64 + FrameAssemblyTimeoutMs;

        while (read < length)
        {
            if (_closed) return false;

            var waitMs = (int)Math.Max(1, Math.Min(idleDeadline, hardDeadline) - Environment.TickCount64);
            if (waitMs <= 0)
            {
                 
                 
                if (!IsPeerAlive()) return false;
                if (Environment.TickCount64 >= hardDeadline) return false;
                idleDeadline = Environment.TickCount64 + SocketPollMs;
                continue;
            }

            try
            {
                if (!client.Client.Poll(waitMs * 1000, SelectMode.SelectRead))
                {
                    idleDeadline = Environment.TickCount64 + SocketPollMs;
                    continue;
                }
            }
            catch
            {
                return false;
            }

            var n = _stream.Read(buffer, read, length - read);
            if (n <= 0) return false;  
            read += n;
            idleDeadline = Environment.TickCount64 + SocketPollMs;
        }

        return true;
    }

     



    private bool IsPeerAlive()
    {
        try
        {
            if (!client.Connected) return false;
            if (!client.Client.Poll(0, SelectMode.SelectRead)) return true;  
            return client.Available > 0;
        }
        catch
        {
            return false;
        }
    }

    private void Dispatch(CSHead head, byte[] body)
    {
        var msgId = head.Msgid;

         
        if (ConfigManager.Config.Debug.EnablePacketLog &&
            msgId != (int)CSMessageID.CsMoveObjectMove &&
            msgId != (int)CSMessageID.CsPing)
        {
            var messageName = PacketLogHelper.GetMessageName(msgId);
            var log = $"<-- CS: {messageName}({msgId})\nLength: {body.Length}";

            if (ConfigManager.Config.Debug.LogPacketPayload && body.Length > 0)
            {
                var json = PacketLogHelper.ConvertPacketToJson(msgId, body);
                log += $"\n{json}";
            }

            if (ConfigManager.Config.Debug.LogPacketToConsole)
                Logger.Debug(log);
            else
                Logger.Info(log);
        }

         
        if (msgId == (int)CSMessageID.CsLogin && Player == null)
        {
            HandleLogin(body);
            return;
        }

        PacketDispatcher.Dispatch(this, msgId, body);
    }

     
    public void DispatchInner(int msgId, ulong upSeqId, byte[] body)
    {
        _lastClientUpSeqId = upSeqId;
        PacketDispatcher.Dispatch(this, msgId, body);
    }

     
    public void Send(int msgId, IMessage msg)
    {
        var body = msg.ToByteArray();

         
        if (ConfigManager.Config.Debug.EnablePacketLog &&
            msgId != (int)SCMessageID.ScPing)
        {
            var messageName = PacketLogHelper.GetMessageName(msgId);
            var log = $"--> SC: {messageName}({msgId})\nLength: {body.Length}, Up: {_lastClientUpSeqId}";

            if (ConfigManager.Config.Debug.LogPacketPayload && body.Length > 0)
            {
                var json = PacketLogHelper.ConvertPacketToJson(msgId, body);
                log += $"\n{json}";
            }

            if (ConfigManager.Config.Debug.LogPacketToConsole)
                Logger.Debug(log);
            else
                Logger.Info(log);
        }

        SendRaw(msgId, body);
    }

    private void SendRaw(int msgId, byte[] body)
    {
        if (_closed) return;
        lock (_sendLock)
        {
            try
            {
                var head = new CSHead
                {
                    Msgid = msgId,
                    UpSeqid = _lastClientUpSeqId,
                    DownSeqid = 0,
                    IsCompress = false,
                    Checksum = Crypto.Crc32(body, 0, body.Length),
                }.ToByteArray();

                var packet = new byte[3 + head.Length + body.Length];
                packet[0] = (byte)head.Length;
                packet[1] = (byte)(body.Length & 0xFF);
                packet[2] = (byte)((body.Length >> 8) & 0xFF);
                Buffer.BlockCopy(head, 0, packet, 3, head.Length);
                Buffer.BlockCopy(body, 0, packet, 3 + head.Length, body.Length);
                if (_encryptor != null)
                {
                    var span = packet.AsSpan(3);
                    _encryptor.Transform(span);
                }
                _stream.Write(packet, 0, packet.Length);
                _stream.Flush();
            }
            catch (Exception e)
            {
                Logger.Error("Send failed", e);
                Stop();
            }
        }
    }

    public void Dispose()
    {
        Stop();
        WaitForStop();
        client.Dispose();
    }
}
