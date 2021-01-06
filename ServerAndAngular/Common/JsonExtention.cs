using System.Text.Json;

namespace SoupDiscover.Common
{
    public static class JsonExtention
    {
        public static string TryGetValueAsString(this JsonElement element, string key, string optionalSubKey = null)
        {
            if (!element.TryGetProperty(key, out var foundElement))
            {
                return null;
            }
            if (foundElement.ValueKind == JsonValueKind.Object)
            {
                if (!foundElement.TryGetProperty(optionalSubKey, out foundElement))
                {
                    return null;
                }
            }
            if (foundElement.ValueKind != JsonValueKind.String)
            {
                return null;
            }
            return foundElement.GetString();
        }
    }
}
