using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ecfrInsights.Data.Entities;
    public partial class CfrTitleComplexity : IEntityTypeConfiguration<CfrTitleComplexity>
    {
        public void Configure(EntityTypeBuilder<CfrTitleComplexity> builder)
        {
            builder.HasKey(c => new { c.Title, c.DateComputed });
            builder.Property(c => c.Title).ValueGeneratedNever();
            builder.Property(c => c.HierarchicalCount).IsRequired();
            builder.Property(c => c.TotalAgencies).IsRequired();
            builder.Property(c => c.Wordcount).IsRequired();
            builder.Property(c => c.TotalCorrections).IsRequired();
            builder.Property(c => c.NormHierarchical).HasPrecision(18, 4);
            builder.Property(c => c.NormAgencies).HasPrecision(18, 4);
            builder.Property(c => c.NormWordcount).HasPrecision(18, 4);
            builder.Property(c => c.NormCorrections).HasPrecision(18, 4);
            builder.Property(c => c.ComplexityScore).HasPrecision(18, 4);
            
    }
    }
