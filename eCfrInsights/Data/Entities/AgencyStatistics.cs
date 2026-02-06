namespace ecfrInsights.Data.Entities;

public partial class AgencyStatistics
{
    public string Slug { get; set; } = default!;
    public int TotalHierarchies { get; set; }
    public int TotalWords { get; set; }
    public int TotalSubAgencies { get; set; }
    public double ComplexityScore { get; set; }
    public double NormHierarchical { get; set; }
    public double NormAgencies { get; set; }
    public double NormWordcount { get; set; }

    public double NormCorrections { get; set; }
    public DateTime ForDate { get; set; }
    public string AgencyName { get; set; } = default!;
    public DateTime DateComputed { get; set; } = DateTime.Now;
    public void CalculateComplexityScore()
    {
        ComplexityScore =
            (NormHierarchical * 2) +
            (NormAgencies * 3) +
            (NormWordcount * 1);
    }

}
