using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ecfrInsights.Data.Entities;

/// <summary>
/// Entity configuration for CfrReferenceHistory.
/// Tracks historical snapshots of CFR documents at specific points in time.
/// </summary>
public partial class CfrHierarchyHistory : IEntityTypeConfiguration<CfrHierarchyHistory>
{
    public void Configure(EntityTypeBuilder<CfrHierarchyHistory> builder)
    {
        builder.HasKey(cfr => cfr.CfrReferenceNumber);

        builder.Property(cfr => cfr.Chapter).IsRequired(false);
        builder.Property(cfr => cfr.LatestAmendedOn).HasColumnType("datetime2");
        builder.Property(cfr => cfr.LatestIssueDate).HasColumnType("datetime2");
        builder.Property(cfr => cfr.UpToDateAsOf).HasColumnType("datetime2");

        //the following are nullable
        builder.Property(cfr => cfr.Section).HasColumnType("nvarchar")
            .HasMaxLength(255)
            .IsRequired(false);
        builder.Property(cfr => cfr.Appendix).HasColumnType("nvarchar")
            .HasMaxLength(255)
            .IsRequired(false);
        builder.Property(cfr => cfr.Authority).HasColumnType("nvarchar")
            .HasMaxLength(255)
            .IsRequired(false);
        builder.Property(cfr => cfr.Subtitle).HasColumnType("nvarchar")
            .HasMaxLength(255)
            .IsRequired(false);
        builder.Property(cfr => cfr.Chapter).HasColumnType("nvarchar")
            .HasMaxLength(255)
            .IsRequired(false);
        builder.Property(cfr => cfr.Subtitle).HasColumnType("nvarchar")
            .HasMaxLength(255)
            .IsRequired(false);
        builder.Property(cfr => cfr.Part).HasColumnType("nvarchar")
            .HasMaxLength(255)
            .IsRequired(false);
        builder.Property(cfr => cfr.Subchapter).HasColumnType("nvarchar")
            .HasMaxLength(255)
            .IsRequired(false);
        builder.Property(cfr => cfr.Subpart).HasColumnType("nvarchar")
            .HasMaxLength(255)
            .IsRequired(false);

        // Foreign key to CfrDocument (Title)
        builder
            .HasOne(cfr => cfr.Title)
            .WithMany(title => title.CfrHierarchyHistories)
            .HasForeignKey(cfr => cfr.TitleNumber)
            .OnDelete(DeleteBehavior.Cascade);
                

        // Self-referencing parent-child relationship
        builder
            .HasOne(cfr => cfr.ParentReference)
            .WithMany(cfr => cfr.ChildReferences)
            .HasForeignKey(cfr => cfr.ParentCfrReferenceNumber)
            .OnDelete(DeleteBehavior.Cascade);
       


        // Index for querying by title
        builder.HasIndex(cfr => cfr.TitleNumber);

        // Index for parent-child relationship queries
        builder.HasIndex(cfr => cfr.ParentCfrReferenceNumber);
    }
}
