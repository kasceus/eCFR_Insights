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
public partial class CfrTitleHistory:ICfrTitle
{
    public int Number { get; set; }

    public string Name { get; set; } = default!;

    public DateTime? LatestAmendedOn { get; set; }

    public DateTime? LatestIssueDate { get; set; }

    public DateTime? UpToDateAsOf { get; set; }

    public bool Reserved { get; set; }

    public int SectionCount { get; set; }
    public string XmlDocumentHash { get; set; } = default!;
    // Navigation property: One Title has Many hierarchy histories
    public ICollection<CfrHierarchyHistory> CfrHierarchyHistories { get; set; } = [];

}
