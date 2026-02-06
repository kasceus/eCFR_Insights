using ecfrInsights.Data.Interfaces;

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ecfrInsights.Data.Entities;

/// <summary>
/// Tracks historical snapshots of CFR documents at specific points in time.
/// Stores XML snapshots and document hashes to detect changes.
/// </summary>
public partial class CfrHierarchyHistory : ICfrHierarchy
{
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
  
    public string? CfrReferenceTitle { get; set; }

    // Foreign key to CfrDocument (Title)
    public int TitleNumber { get; set; }


    public string? Chapter { get; set; }

    public string? Subtitle { get; set; }


    public string? Part { get; set; }

    public string? Subchapter { get; set; }

   
    public string? Subpart { get; set; }

   
    public string? Section { get; set; }

    
    public string? Appendix { get; set; }

    public DateTime? LatestAmendedOn { get; set; }

    public DateTime? LatestIssueDate { get; set; }

    public DateTime? UpToDateAsOf { get; set; }

    public bool Reserved { get; set; }


    /// <summary>
    /// this will be the contents of the reference document
    /// </summary>
    public string? ReferenceContent { get; set; } = default!;

    public string? Citation { get; set; } = default!;

    [NotMapped]
    public int WordCount => string.IsNullOrEmpty(ReferenceContent) ? 0 : ReferenceContent.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
    [NotMapped]
    public int WordComplexityScore => string.IsNullOrEmpty(ReferenceContent) ? 0 : ReferenceContent.Split(' ', StringSplitOptions.RemoveEmptyEntries).Count(word => word.Length > 6);

    [NotMapped]
    public int TotalChildren => ChildReferences?.Count ?? 0;

    public ICollection<CfrHierarchyHistory> ChildReferences { get; set; } = [];

    public virtual CfrHierarchyHistory? ParentReference { get; set; }
    public virtual CfrTitleHistory Title { get; set; } = default!;
    public virtual CfrHierarchy CurrentHierarchy  { get; set; } = default!;

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
}