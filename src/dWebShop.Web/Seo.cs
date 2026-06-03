using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Components;

namespace dWebShop.Web;

public static class Seo
{
    public const string BaseUrl = "https://asgifiks.ba";

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false,
    };

    public static string Canonical(string path)
    {
        if (string.IsNullOrEmpty(path)) return BaseUrl + "/";
        return path.StartsWith("http") ? path : BaseUrl + (path.StartsWith('/') ? path : "/" + path);
    }

    public static MarkupString JsonLd(JsonObject obj)
    {
        obj["@context"] ??= "https://schema.org";
        return new MarkupString($"<script type=\"application/ld+json\">{obj.ToJsonString(JsonOpts)}</script>");
    }
}
