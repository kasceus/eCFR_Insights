using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ecfrInsights.Data.Entities;

public partial class AgencyHierarchy : IEntityTypeConfiguration<AgencyHierarchy>
{
    public void Configure(EntityTypeBuilder<AgencyHierarchy> builder)
    {
        builder.HasKey(ah => new { ah.Slug, ah.CfrReferenceNumber });
        builder.Property(ah => ah.Slug)
            .HasColumnType("nvarchar")
            .HasMaxLength(255)
            .IsRequired();
        builder.Property(ah => ah.CfrReferenceNumber)
            .HasColumnType("nvarchar")
            .HasMaxLength(255)
            .IsRequired();
        // Foreign key to GraphsModel
        builder
            .HasOne(ah => ah.Agency)
            .WithMany(a => a.AgencyHierarchies)
            .HasForeignKey(ah => ah.Slug)
            .OnDelete(DeleteBehavior.Cascade);
        // Foreign key to CfrHierarchy
        builder
            .HasOne(ah => ah.CfrHierarchy)
            .WithMany(cfr => cfr.AgencyHierarchies)
            .HasForeignKey(ah => ah.CfrReferenceNumber)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
