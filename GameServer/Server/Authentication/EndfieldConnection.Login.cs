using Google.Protobuf;
using MollyPS.GameServer.Game;
using MollyPS.GameServer.Server.Packet.Send.Player;
using MollyPS.Util;
using Proto;

namespace MollyPS.GameServer.Server;

 
public partial class EndfieldConnection
{
    private void HandleLogin(byte[] body)
    {
        MSG_A1 login;
        try
        {
            login = MSG_A1.Parser.ParseFrom(body);
        }
        catch (InvalidProtocolBufferException)
        {
            Logger.Error("Expected plaintext CS_LOGIN. Native SRSA is not supported; verify the ref-C patch initialized for this GameAssembly.");
            Stop();
            return;
        }

        var uid = login.A1 ?? "0";
        Logger.Info($"CS_LOGIN: uid={uid} res_ver={login.A7} client_ver={login.A6} pubkey={(login.A8 != null && login.A8.Length > 0 ? $"{login.A8?.Length ?? 0}B" : "none")} last_down_seq={login.A5}");
         
        var key = Crypto.GenerateRandomBytes(32);
        var nonce = Crypto.GenerateRandomBytes(12);
        var wantCrypto = ConfigManager.Config.Game.EnableCrypto && login.A8 != null && login.A8.Length > 0;
        MSG_B1 resp;
        try
        {
            resp = new MSG_B1
            {
                F1 = uid,  
                F9 = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),  
                F10 = (int)TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow).TotalHours,  
                F14 = ConfigManager.Config.Game.BranchVersion,
            };
            if (wantCrypto)
            {
                using var rsa = Crypto.ImportClientPublicKey(login.A8!.ToByteArray());
                resp.F3 = ByteString.CopyFrom(Crypto.RsaEncryptSessionKey(rsa, key, ConfigManager.Config.Game.RsaPadding));
                resp.F4 = ByteString.CopyFrom(nonce);
            }
        }
        catch (Exception e)
        {
            Logger.Error("Failed to build MSG_B1", e);
            Stop();
            return;
        }

        if (wantCrypto)
        {
             
            _decryptor = new ChaCha20(key, nonce, 1);
        }

        SendRaw((int)SCMessageID.ScLogin, resp.ToByteArray());

        if (wantCrypto)
        {
            _encryptor = new ChaCha20(key, nonce, 1);
            Logger.Info("Login handshake done, ChaCha20 enabled");
        }
        else
        {
            Logger.Info("Login handshake done (no encryption, patched client)");
        }

        Player = new PlayerInstance(this, ulong.TryParse(uid, out var parsedUid) ? parsedUid : ConfigManager.Config.Game.DefaultUid);
        PlayerSyncPackets.SendFullSync(Player);
    }
}
