using System.Collections.Concurrent;
using System.Reflection;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto;

namespace MollyPS.GameServer.Server;

public static class PacketLogHelper
{
    private static readonly ConcurrentDictionary<int, ParseIMessage> CachedParsers = new();
    private static readonly ConcurrentDictionary<int, string> MessageNames = new();

    public static string ConvertPacketToJson(int msgId, byte[] payload)
    {
        try
        {
            var parser = GetParser(msgId);
            if (parser == null) return "Unknown message type";

            var message = parser(payload);
            var formatter = new JsonFormatter(new JsonFormatter.Settings(false));
            return formatter.Format(message);
        }
        catch (Exception ex)
        {
            return $"Parse error: {ex.Message}";
        }
    }

    public static string GetMessageName(int msgId)
    {
        if (MessageNames.TryGetValue(msgId, out var name))
            return name;

         
        var csName = Enum.GetName(typeof(CSMessageID), msgId);
        if (csName != null)
        {
            MessageNames[msgId] = csName;
            return csName;
        }

         
        var scName = Enum.GetName(typeof(SCMessageID), msgId);
        if (scName != null)
        {
            MessageNames[msgId] = scName;
            return scName;
        }

        return $"Unknown_{msgId}";
    }

    private static ParseIMessage? GetParser(int msgId)
    {
        if (CachedParsers.TryGetValue(msgId, out var parser))
            return parser;

        lock (CachedParsers)
        {
             
            if (CachedParsers.TryGetValue(msgId, out parser))
                return parser;

            var messageName = GetMessageName(msgId);
            if (messageName.StartsWith("Unknown_"))
                return null;

             
            messageName = messageName.Replace("CSMessageID_", "").Replace("SCMessageID_", "");

             
            var protoAssembly = typeof(CSHead).Assembly;
            var messageType = protoAssembly.GetType($"Proto.{messageName}");
            if (messageType == null)
                return null;

             
            var descriptorProperty = messageType.GetProperty("Descriptor", BindingFlags.Public | BindingFlags.Static);
            if (descriptorProperty?.GetValue(null) is not MessageDescriptor descriptor)
                return null;

             
            var parseMethod = descriptor.Parser.GetType().GetMethod("ParseFrom", [typeof(byte[])]);
            if (parseMethod == null)
                return null;

            parser = (ParseIMessage)Delegate.CreateDelegate(
                typeof(ParseIMessage),
                descriptor.Parser,
                parseMethod
            );

            CachedParsers[msgId] = parser;
            return parser;
        }
    }

    private delegate IMessage ParseIMessage(byte[] data);
}
