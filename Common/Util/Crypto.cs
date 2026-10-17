using System.Security.Cryptography;

namespace MollyPS.Util;

 




public class ChaCha20(byte[] key, byte[] nonce, uint counter = 1)
{
    private const int Rounds = 20;

    private static readonly uint[] Sigma = [0x61707865, 0x3320646E, 0x79622D32, 0x6B206574];

    private readonly uint[] _state = BuildState(key, nonce, counter);
    private byte[] _keystream = new byte[64];
    private int _keystreamPos = 64;

    private static uint[] BuildState(byte[] key, byte[] nonce, uint counter)
    {
        if (key.Length != 32)
            throw new ArgumentException("ChaCha20 key must be 32 bytes", nameof(key));
        if (nonce.Length != 12)
            throw new ArgumentException("ChaCha20 nonce must be 12 bytes", nameof(nonce));

        var state = new uint[16];
        Sigma.CopyTo(state, 0);
        for (var i = 0; i < 8; i++)
            state[4 + i] = BitConverter.ToUInt32(key, i * 4);
        state[12] = counter;
        state[13] = BitConverter.ToUInt32(nonce, 0);
        state[14] = BitConverter.ToUInt32(nonce, 4);
        state[15] = BitConverter.ToUInt32(nonce, 8);
        return state;
    }

    private static void QuarterRound(ref uint a, ref uint b, ref uint c, ref uint d)
    {
        a += b; d ^= a; d = (d << 16) | (d >> 16);
        c += d; b ^= c; b = (b << 12) | (b >> 20);
        a += b; d ^= a; d = (d << 8) | (d >> 24);
        c += d; b ^= c; b = (b << 7) | (b >> 25);
    }

    private void NextBlock()
    {
        var x = (uint[])_state.Clone();
        for (var i = 0; i < Rounds; i += 2)
        {
            QuarterRound(ref x[0], ref x[4], ref x[8], ref x[12]);
            QuarterRound(ref x[1], ref x[5], ref x[9], ref x[13]);
            QuarterRound(ref x[2], ref x[6], ref x[10], ref x[14]);
            QuarterRound(ref x[3], ref x[7], ref x[11], ref x[15]);
            QuarterRound(ref x[0], ref x[5], ref x[10], ref x[15]);
            QuarterRound(ref x[1], ref x[6], ref x[11], ref x[12]);
            QuarterRound(ref x[2], ref x[7], ref x[8], ref x[13]);
            QuarterRound(ref x[3], ref x[4], ref x[9], ref x[14]);
        }
        for (var i = 0; i < 16; i++)
            x[i] += _state[i];
        Buffer.BlockCopy(x, 0, _keystream = new byte[64], 0, 64);
        _keystreamPos = 0;

         
        if (++_state[12] == 0)
            throw new InvalidOperationException("ChaCha20 counter overflow");
    }

    public void Transform(Span<byte> data)
    {
        for (var i = 0; i < data.Length; i++)
        {
            if (_keystreamPos >= 64)
                NextBlock();
            data[i] ^= _keystream[_keystreamPos++];
        }
    }

    public byte[] TransformBytes(byte[] data)
    {
        var output = (byte[])data.Clone();
        Transform(output);
        return output;
    }
}

public static class Crypto
{
    public static readonly Logger Logger = new("Crypto");

    private static uint[]? _crcTable;

    private static uint[] CrcTable
    {
        get
        {
            if (_crcTable != null) return _crcTable;
            _crcTable = new uint[256];
            for (uint i = 0; i < 256; i++)
            {
                var c = i;
                for (var k = 0; k < 8; k++)
                    c = (c & 1) != 0 ? 0xEDB88320 ^ (c >> 1) : c >> 1;
                _crcTable[i] = c;
            }
            return _crcTable;
        }
    }

     
    public static uint Crc32(byte[] data, int offset, int length)
    {
        uint crc = 0xFFFFFFFF;
        for (var i = offset; i < offset + length; i++)
            crc = CrcTable[(crc ^ data[i]) & 0xFF] ^ (crc >> 8);
        return crc ^ 0xFFFFFFFF;
    }

     



    public static RSA ImportClientPublicKey(byte[] raw)
    {
        if (raw == null || raw.Length == 0)
            throw new ArgumentException("empty public key");

        var bytes = raw;

         
        var asText = System.Text.Encoding.ASCII.GetString(bytes);
        if (asText.Contains("-----BEGIN"))
        {
            var start = asText.IndexOf("KEY-----", StringComparison.Ordinal);
            var end = asText.IndexOf("-----END", StringComparison.Ordinal);
            if (start >= 0 && end > start)
            {
                var body = asText.Substring(start + "KEY-----".Length, end - start - "KEY-----".Length);
                bytes = Convert.FromBase64String(new string(body.Where(c => !char.IsWhiteSpace(c)).ToArray()));
            }
            else
            {
                bytes = Convert.FromBase64String(new string(asText.Where(c => !char.IsWhiteSpace(c) && c != '-').ToArray()));
            }
        }
        else if (bytes.Length > 64 && bytes.All(b => b is (byte)'+' or (byte)'/' or (byte)'=' or >= (byte)'0' and <= (byte)'9' or >= (byte)'A' and <= (byte)'Z' or >= (byte)'a' and <= (byte)'z'))
        {
             
            try { bytes = Convert.FromBase64String(System.Text.Encoding.ASCII.GetString(bytes)); } catch {   }
        }

        var rsa = RSA.Create();
         
        try
        {
            rsa.ImportSubjectPublicKeyInfo(bytes, out _);
            return rsa;
        }
        catch {   }
         
        try
        {
            rsa.ImportRSAPublicKey(bytes, out _);
            return rsa;
        }
        catch {   }

        throw new InvalidDataException($"Unsupported client public key format, len={raw.Length}, head={Convert.ToHexString(raw[..Math.Min(16, raw.Length)])}");
    }

     
    public static byte[] RsaEncryptSessionKey(RSA clientKey, byte[] data, string padding)
    {
        return padding switch
        {
            "Pkcs1" => clientKey.Encrypt(data, RSAEncryptionPadding.Pkcs1),
            "OaepSHA256" => clientKey.Encrypt(data, RSAEncryptionPadding.OaepSHA256),
            _ => clientKey.Encrypt(data, RSAEncryptionPadding.OaepSHA1),
        };
    }

     



    public static byte[] AesCbcEncrypt(byte[] key, byte[] data)
    {
        using var aes = Aes.Create();
        aes.Key = key;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.GenerateIV();
        using var enc = aes.CreateEncryptor();
        var cipher = enc.TransformFinalBlock(data, 0, data.Length);
        var result = new byte[aes.IV.Length + cipher.Length];
        Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
        Buffer.BlockCopy(cipher, 0, result, aes.IV.Length, cipher.Length);
        return result;
    }

    public static byte[] GenerateRandomBytes(int length)
    {
        var bytes = new byte[length];
        RandomNumberGenerator.Fill(bytes);
        return bytes;
    }

    public static string CreateSessionKey()
    {
        var bytes = GenerateRandomBytes(32);
        return Convert.ToBase64String(bytes);
    }
}
