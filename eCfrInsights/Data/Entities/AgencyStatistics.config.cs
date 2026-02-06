using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ecfrInsights.Data.Entities;

public partial class AgencyStatistics : IEntityTypeConfiguration<AgencyStatistics>
{
    public void Configure(EntityTypeBuilder<AgencyStatistics> builder)
    {
        builder.HasKey(a => new { a.Slug, a.ForDate });
        builder.Property(a => a.Slug).HasMaxLength(255).IsRequired();
        builder.Property(a => a.TotalWords).IsRequired();
        builder.Property(a => a.TotalSubAgencies).IsRequired();
        builder.Property(a => a.ComplexityScore).IsRequired();
        
    }
}
