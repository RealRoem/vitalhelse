using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace VitalHelse.Helpers
{
    public static class SessionExtensions
    {
        public static void SetObjectAsJson(this ISession session, string key, object value)
        {
            session.SetString(key, JsonSerializer.Serialize(value));
        }

        public static T? GetObjectFromJson<T>(this ISession session)
        {
            var value = session.GetString(typeof(T).FullName);
            return value == null ? default : JsonSerializer.Deserialize<T>(value);
        }
        
        public static T? GetObjectFromJson<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default : JsonSerializer.Deserialize<T>(value);
        }
    }
}