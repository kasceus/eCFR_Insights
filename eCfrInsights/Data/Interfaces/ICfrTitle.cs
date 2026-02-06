using ecfrInsights.Data.Entities;

namespace ecfrInsights.Data.Interfaces;

public interface ICfrTitle
{
    public int Number { get; set; }

    public string Name { get; set; }

    public DateTime? LatestAmendedOn { get; set; }

    public DateTime? LatestIssueDate { get; set; }

    public DateTime? UpToDateAsOf { get; set; }

    public bool Reserved { get; set; }

    public int SectionCount { get; set; }
    public string XmlDocumentHash { get; set; } 



}
