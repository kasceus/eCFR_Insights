using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ecfrInsights.Data.eCFRApi;

/// <summary>
/// Root container for titles data from the eCFR API.
/// </summary>
public class TitlesRoot
{
    [JsonPropertyName("titles")]
    public List<Title> Titles { get; set; } = [];

    [JsonPropertyName("meta")]
    public TitlesMeta Meta { get; set; } = default!;
}

/// <summary>
/// Represents a single CFR title.
/// </summary>
public class Title
{
    [JsonPropertyName("number")]
    public int Number { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = default!;

    [JsonPropertyName("latest_amended_on")]
    public string LatestAmendedOn { get; set; }

    [JsonPropertyName("latest_issue_date")]
    public string LatestIssueDate { get; set; }

    [JsonPropertyName("up_to_date_as_of")]
    public string UpToDateAsOf { get; set; }

    [JsonPropertyName("reserved")]
    public bool Reserved { get; set; }
}

/// <summary>
/// Metadata for the titles data. not needed to be stored
/// </summary>
public class TitlesMeta
{
    [JsonPropertyName("date")]
    public string Date { get; set; } = default!;

    [JsonPropertyName("import_in_progress")]
    public bool ImportInProgress { get; set; }
}
