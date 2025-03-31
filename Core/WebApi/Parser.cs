using System.Text.Json;

namespace Led3D_2.WebApi;

public static class Parser
{
    public static object ParseJsonElement(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Object => ParseJsonObject(element),
            JsonValueKind.Array => ParseJsonArray(element),
            JsonValueKind.String => element.GetString()!,
            JsonValueKind.Number => element.TryGetInt32(out int intVal) ? (object)intVal : element.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null!,
            _ => throw new ArgumentException($"Unsupported JSON value kind: {element.ValueKind}")
        };
    }

    public static Dictionary<string, object> ParseJsonObject(JsonElement element)
    {
        return element.EnumerateObject().ToDictionary(kvp => kvp.Name, kvp => ParseJsonElement(kvp.Value));
    }

    public static List<object> ParseJsonArray(JsonElement element)
    {
        return element.EnumerateArray().Select(ParseJsonElement).ToList();
    }
}