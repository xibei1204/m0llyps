using Newtonsoft.Json;

namespace MollyPS.Util;

public static class Extensions
{
    public static T RandomElement<T>(this List<T> values)
    {
        var index = new Random().Next(values.Count);
        return values[index];
    }

    public static ICollection<T> Clone<T>(this ICollection<T> values)
    {
        List<T> list = [.. values];

        return list;
    }

    public static int RandomInt(int from, int to)
    {
        return new Random().Next(from, to);
    }

    public static void SafeAdd<T>(this List<T> list, T item)
    {
        if (!list.Contains(item)) list.Add(item);
    }

    public static void SafeAddRange<T>(this List<T> list, List<T> item)
    {
        foreach (var i in item) list.SafeAdd(i);
    }

    public static long GetUnixSec()
    {
        return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public static long ToUnixSec(this DateTime dt)
    {
        return new DateTimeOffset(dt).ToUnixTimeSeconds();
    }

    public static long GetUnixMs()
    {
        return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }

    public static long ToUnixMs(this DateTime dt)
    {
        return new DateTimeOffset(dt).ToUnixTimeMilliseconds();
    }

    public static string ToArrayString<T>(this List<T> list)
    {
        return list.JoinFormat(", ", "");
    }

    public static string ToJsonString<TK, TV>(this Dictionary<TK, TV> dic) where TK : notnull
    {
        return JsonConvert.SerializeObject(dic);
    }

    public static string JoinFormat<T>(this IEnumerable<T> list, string separator,
        string formatString)
    {
        formatString = string.IsNullOrWhiteSpace(formatString) ? "{0}" : formatString;
        return string.Join(separator,
            list.Select(item => string.Format(formatString, item)));
    }
}
