using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ecfrInsights.Data.Interfaces;

public interface ICfrHierarchy
{
    [Key]
    [Required]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [MaxLength(512)]
    public string CfrReferenceNumber{ get; set; }

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

    /// <summary>
    /// this will be the contents of the reference document
    /// </summary>
    public string? ReferenceContent { get; set; } 

    public string? Citation { get; set; }
}
