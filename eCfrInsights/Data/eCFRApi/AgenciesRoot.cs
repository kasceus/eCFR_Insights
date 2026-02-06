using System.Collections.Generic;
using System.Text.Json.Serialization;

public class AgenciesRoot
{
    [JsonPropertyName("agencies")]
    public List<ApiAgency> Agencies { get; set; } = new();
}

public class ApiAgency
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = default!;

    [JsonPropertyName("short_name")]
    public string ShortName { get; set; } = default!;

    [JsonPropertyName("display_name")]
    public string DisplayName { get; set; }= default!;

    [JsonPropertyName("sortable_name")]
    public string SortableName { get; set; }= default!;

    [JsonPropertyName("slug")]
    public string Slug { get; set; }= default!;

    // Recursive structure
    [JsonPropertyName("children")]
    public List<ApiAgency> Children { get; set; } = [];

    [JsonPropertyName("cfr_references")]
    public List<ApiCfrReference> CfrReferences { get; set; } = [];
}

public class ApiCfrReference
{
    [JsonPropertyName("title")]
    public int Title { get; set; }

    [JsonPropertyName("chapter")]
    public string Chapter { get; set; } = default!;

    [JsonPropertyName("subtitle")]
    public string Subtitle { get; set; } = default!;

    [JsonPropertyName("part")]
    public string Part { get; set; }= default!;

    [JsonPropertyName("subchapter")]
    public string Subchapter { get; set; }= default!;

}
public static class ApiCfrReferenceExtensions
{
    public static string GetReferenceNumber(this ApiCfrReference reference)
    {
        var parts = new List<string> { reference.Title.ToString() };
        if (!string.IsNullOrEmpty(reference.Subtitle)) parts.Add(reference.Subtitle);
        if (!string.IsNullOrEmpty(reference.Chapter)) parts.Add(reference.Chapter);
        if (!string.IsNullOrEmpty(reference.Subchapter)) parts.Add(reference.Subchapter);
        if (!string.IsNullOrEmpty(reference.Part)) parts.Add(reference.Part);
        return string.Join("-", parts);
    }
}