
namespace ecfrInsights.Data.Entities;

public partial class AgencyHierarchy
{
    /// <summary>
    /// GraphsModel slug
    /// </summary>
    public string Slug { get; set; } = null!;
    /// <summary>
    /// FK to the CFR reference number for this agency's hierarchy. This is the unique identifier for this hierarchy.
    /// </summary>
    public string CfrReferenceNumber { get; set; } = null!;
    /// <summary>
    /// GraphsModel ref
    /// </summary>
    public virtual Agency? Agency { get; set; }
    /// <summary>
    /// current hierarchy reference
    /// </summary>
    public virtual CfrHierarchy? CfrHierarchy { get; set; }
       
}
