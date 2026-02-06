namespace ecfrInsights.Data.Entities;

public partial class CfrTitleComplexity
{
    public int Title { get; set; }
    public string TitleText { get; set; } = string.Empty;

    public int HierarchicalCount { get; set; }
    public int TotalAgencies { get; set; }
    public int Wordcount { get; set; }
    public int TotalCorrections { get; set; }

    // Normalized metrics (0–1)
    public double NormHierarchical { get; set; }
    public double NormAgencies { get; set; }
    public double NormWordcount { get; set; }
    public double NormCorrections { get; set; }

    public double ComplexityScore { get; set; }
    public DateTime DateComputed { get; set; } = DateTime.Now;
}
