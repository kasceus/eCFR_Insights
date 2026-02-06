using ecfrInsights.Data.eCFRApi;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ecfrInsights.Data.Entities
{
    public partial class Correction : IEntityTypeConfiguration<Correction>
    {
        public void Configure(EntityTypeBuilder<Correction> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.CorrectiveAction).IsRequired();
            builder.Property(c => c.FrCitation).IsRequired();
            builder.Property(c => c.ErrorCorrected).HasColumnType("datetime2");
            builder.Property(c => c.ErrorOccurred).HasColumnType("datetime2");
            builder.Property(c => c.LastModified).HasColumnType("datetime2");

            // Foreign key to StoredCfrReference
            builder
                .HasOne(c => c.CfrDocument)
                .WithMany(cfr => cfr.Corrections)
                .HasForeignKey(c => c.Title)
                .OnDelete(DeleteBehavior.Cascade);

            // Index for querying corrections by title
            builder.HasIndex(c => c.Title);

            // Index for querying corrections by year
            builder.HasIndex(c => c.Year);
        }

       
    }
}