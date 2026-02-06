
using ecfrInsights.Data.eCFRApi;
using ecfrInsights.Data.Interfaces;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ecfrInsights.Data.Entities;

/// <summary>
/// CFR reference data for an agency or correction.
/// Represents a specific section in the Code of Federal Regulations.
/// </summary>
public partial class CfrHierarchy : ICfrHierarchy
{
    /// <summary>
    /// Persisted canonical key for this reference (used as EF key).
    /// Built from TitleNumber + structural parts when not explicitly set.
    /// </summary>
    private string? _cfrReferenceNumber;

    [Key]
    [Required]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [MaxLength(512)]
    public string CfrReferenceNumber
    {
        get => !string.IsNullOrEmpty(_cfrReferenceNumber)
            ? _cfrReferenceNumber
            : BuildReferenceNumber(TitleNumber, Subtitle, Chapter, Subchapter, Part, Subpart, Section, Appendix);
        set => _cfrReferenceNumber = value;
    }

    public string? ParentCfrReferenceNumber { get; set; }
  
    public string? Authority { get; set; }
    public string? Source { get; set; }
    /// <summary>
    /// title of the reference document, e.g. "Title 14 - Aeronautics and Space"
    /// </summary>
    public string? CfrReferenceTitle { get; set; } 

    // Foreign key to CfrDocument (Title)
    public int TitleNumber { get; set; }

    /// <summary>
    /// Only contains the identifier for this specific hierarchical level (e.g., "I" for Subtitle, "1" for Chapter).
    /// Does NOT include parent context. Parent hierarchy is maintained via ParentCfrReferenceNumber.
    /// </summary>
    public string? Chapter { get; set; } 

    /// <summary>
    /// Only contains the identifier for this specific hierarchical level.
    /// Does NOT include parent context.
    /// </summary>
    public string? Subtitle { get; set; } 

    /// <summary>
    /// Only contains the identifier for this specific hierarchical level.
    /// Does NOT include parent context.
    /// </summary>
    public string? Part { get; set; }

    /// <summary>
    /// Only contains the identifier for this specific hierarchical level.
    /// Does NOT include parent context.
    /// </summary>
    public string? Subchapter { get; set; } 

    /// <summary>
    /// Only contains the identifier for this specific hierarchical level.
    /// Does NOT include parent context.
    /// </summary>
    public string? Subpart { get; set; } 

    /// <summary>
    /// Only contains the identifier for this specific hierarchical level.
    /// Does NOT include parent context.
    /// </summary>
    public string? Section { get; set; }

    /// <summary>
    /// Only contains the identifier for this specific hierarchical level.
    /// Does NOT include parent context.
    /// </summary>
    public string? Appendix { get; set; } 

    public DateTime? LatestAmendedOn { get; set; }

    public DateTime? LatestIssueDate { get; set; }

    public DateTime? UpToDateAsOf { get; set; }

    public bool Reserved { get; set; }

    // Foreign key to StoredAgency
    public string? AgencySlug { get; set; }

    /// <summary>
    /// this will be the contents of the reference document
    /// </summary>
    public string? ReferenceContent { get; set; } = default!;

    public string? Citation{ get; set; } = default!;

    // Navigation property to Title
    public CfrTitle Title { get; set; } = default!;

    /// <summary>
    /// Parent reference in the hierarchy (e.g., if this is a Section, parent is the Part).
    /// Null for Title-level references.
    /// </summary>
    public CfrHierarchy? ParentReference { get; set; }

    /// <summary>
    /// Child references in the hierarchy (e.g., if this is a Part, children are Subparts).
    /// </summary>
    public virtual ICollection<CfrHierarchy> ChildReferences { get; set; } = [];

    public virtual ICollection<CfrHierarchyHistory> Histories{ get; set; } = [];

    public virtual ICollection<AgencyHierarchy> AgencyHierarchies { get; set; } =[];
    public static string BuildReferenceNumber(int titleNumber, string? subtitle, string? chapter, string? subchapter, string? part, string? subpart, string? section, string? appendix)
    {
        var sb = new StringBuilder(titleNumber.ToString());
        if (!string.IsNullOrWhiteSpace(subtitle)) sb.Append('-').Append(subtitle.Trim());
        if (!string.IsNullOrWhiteSpace(chapter)) sb.Append('-').Append(chapter.Trim());
        if (!string.IsNullOrWhiteSpace(subchapter)) sb.Append('-').Append(subchapter.Trim());
        if (!string.IsNullOrWhiteSpace(part)) sb.Append('-').Append(part.Trim());
        if (!string.IsNullOrWhiteSpace(subpart)) sb.Append('-').Append(subpart.Trim());
        if (!string.IsNullOrWhiteSpace(section)) sb.Append('-').Append(section.Trim());
        if (!string.IsNullOrWhiteSpace(appendix)) sb.Append('-').Append(appendix.Trim());
        return sb.ToString();
    }
    public static implicit operator CfrHierarchy(ApiCfrReference cfr)
    {
        var r = new CfrHierarchy
        {
            TitleNumber = cfr.Title,
            Chapter = cfr.Chapter,
            Subtitle = cfr.Subtitle,
            Part = cfr.Part,
            Subchapter = cfr.Subchapter,
        };
        r.CfrReferenceNumber = BuildReferenceNumber(r.TitleNumber, r.Subtitle, r.Chapter, r.Subchapter, r.Part, r.Subpart, r.Section, r.Appendix);
        return r;
    }
}