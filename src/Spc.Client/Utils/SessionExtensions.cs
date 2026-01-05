using System.Text.Json;
namespace Spc.Client.Utils;

public static class SessionExtensions
{
    public static void SetJson<T>(this ISession session, string key, T value)
        => session.SetString(key, JsonSerializer.Serialize(value));

    public static T? GetJson<T>(this ISession session, string key)
    {
        var str = session.GetString(key);
        return str is null ? default : JsonSerializer.Deserialize<T>(str);
    }
}