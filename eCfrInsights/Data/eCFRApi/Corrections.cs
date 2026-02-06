using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ecfrInsights.Data.eCFRApi;

/// <summary>
/// Root container for corrections data from the eCFR API.
/// </summary>
public class CorrectionsRoot
{
    [JsonPropertyName("ecfr_corrections")]
    public List<ApiCorrection> Corrections { get; set; } = [];
}

/// <summary>
/// Represents a single correction entry from the eCFR API.
/// </summary>
public class ApiCorrection
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("cfr_references")]
    public List<CorrectionReference> CfrReferences { get; set; } = [];

    [JsonPropertyName("corrective_action")]
    public string CorrectiveAction { get; set; } = default!;

    [JsonPropertyName("error_corrected")]
    public string ErrorCorrected { get; set; } = default!;

    [JsonPropertyName("error_occurred")]
    public string ErrorOccurred { get; set; } = default!;

    [JsonPropertyName("fr_citation")]
    public string FrCitation { get; set; } = default!;

    [JsonPropertyName("position")]
    public int Position { get; set; }

    [JsonPropertyName("display_in_toc")]
    public bool DisplayInToc { get; set; }

    [JsonPropertyName("title")]
    public int Title { get; set; }

    [JsonPropertyName("year")]
    public int Year { get; set; }

    [JsonPropertyName("last_modified")]
    public string LastModified { get; set; } = default!;
}

/// <summary>
/// Represents a CFR reference within a correction.
/// </summary>
public class CorrectionReference
{
    [JsonPropertyName("cfr_reference")]
    public string CfrReference { get; set; } = default!;

    [JsonPropertyName("hierarchy")]
    public CorrectionHierarchy Hierarchy { get; set; } = default!;
}

/// <summary>
/// Represents the hierarchical structure of a CFR reference.
/// </summary>
public class CorrectionHierarchy
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = default!;

    [JsonPropertyName("subtitle")]
    public string Subtitle { get; set; }

    [JsonPropertyName("chapter")]
    public string Chapter { get; set; }

    [JsonPropertyName("subchapter")]
    public string Subchapter { get; set; }

    [JsonPropertyName("part")]
    public string Part { get; set; }

    [JsonPropertyName("subpart")]
    public string Subpart { get; set; }

    [JsonPropertyName("section")]
    public string Section { get; set; }

    [JsonPropertyName("subject_group")]
    public string SubjectGroup { get; set; }

    [JsonPropertyName("appendix")]
    public string Appendix { get; set; }
}
