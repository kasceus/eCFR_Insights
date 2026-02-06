using ecfrInsights.Data.eCFRApi;
using ecfrInsights.Data.Interfaces;

using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace ecfrInsights.Data.Entities;

/// <summary>
/// Database entity for stored CFR titles.
/// </summary>
public partial class CfrTitle: ICfrTitle
{
    public int Number { get; set; }

    public string Name { get; set; } = default!;

    public DateTime? LatestAmendedOn { get; set; }

    public DateTime? LatestIssueDate { get; set; }

    public DateTime? UpToDateAsOf { get; set; }

    public bool Reserved { get; set; }

    public int SectionCount { get; set; }
    public string XmlDocumentHash { get; set; } = default!;

    // Navigation property: One Title has Many CfrHierarchies
    public ICollection<CfrHierarchy> CfrReferences { get; set; } = [];

    public ICollection<Correction> Corrections { get; set; } = [];

    /// <summary>
    /// Implicit conversion from Title to StoredTitle.
    /// </summary>
    public static implicit operator CfrTitle(Title title)
    {
        return new CfrTitle
        {
            Number = title.Number,
            Name = title.Name,
            LatestAmendedOn = string.IsNullOrEmpty(title.LatestAmendedOn) ? null : DateTime.Parse(title.LatestAmendedOn),
            LatestIssueDate = string.IsNullOrEmpty(title.LatestIssueDate) ? null : DateTime.Parse(title.LatestIssueDate),
            UpToDateAsOf = string.IsNullOrEmpty(title.UpToDateAsOf) ? null : DateTime.Parse(title.UpToDateAsOf),
            Reserved = title.Reserved
        };
    }
   
    

}
