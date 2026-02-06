using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace ecfrInsights.Data.Entities;

public partial class Agency : IEntityTypeConfiguration<Agency>
{
    public void Configure(EntityTypeBuilder<Agency> builder)
    {
        builder.HasKey(a => a.Slug);
        builder.Property(a => a.Name).IsRequired();

        // Self-referencing one-to-many: Parent -> Children
        // Navigation names: "Parent" (single), "Children" (collection)
        // FK property name on the child: "ParentSlug"
        builder
            .HasMany("Children")
            .WithOne("Parent")
            .HasForeignKey("ParentSlug")
            .OnDelete(DeleteBehavior.Restrict);

      
    }
}