using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ecfrInsights.Data.Entities
{
    public partial class CfrTitle : IEntityTypeConfiguration<CfrTitle>
    {
        public void Configure(EntityTypeBuilder<CfrTitle> builder)
        {
            builder.HasKey(t => t.Number);
            builder.Property(t => t.Name).IsRequired();
            builder.Property(t => t.LatestAmendedOn).HasColumnType("datetime2");
            builder.Property(t => t.LatestIssueDate).HasColumnType("datetime2");
            builder.Property(t => t.UpToDateAsOf).HasColumnType("datetime2");

            // Title -> CfrHierarchies one-to-many
            // One title has many CFR references
            builder
                .HasMany(t => t.CfrReferences)
                .WithOne(cfr => cfr.Title)
                .HasForeignKey(cfr => cfr.TitleNumber)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}