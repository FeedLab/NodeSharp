using System.Text.Json;
using System.Text.Json.Nodes;

namespace NodeSharp.Nodes.Common.Extension;

public static class JsonExtensions
{
    public static string ToPrettyJson(this string value)
    {
        var node = JsonNode.Parse(value)!;
        
        return JsonSerializer.Serialize(node, new JsonSerializerOptions
        {
            WriteIndented = true
        });
    }
}
