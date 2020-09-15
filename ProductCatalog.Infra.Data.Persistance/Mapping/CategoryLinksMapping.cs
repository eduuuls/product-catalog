using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductCatalog.Domain.Entities;
using System.Diagnostics.CodeAnalysis;

namespace ProductCatalog.Infra.Data.Persistance.Mapping
{
    [ExcludeFromCodeCoverage]
    public class CategoryLinksMapping : IEntityTypeConfiguration<CategoryLink>
    {
        public void Configure(EntityTypeBuilder<CategoryLink> builder)
        {
            builder.ToTable("CategoryLinks");
            builder.HasKey("Id");

            builder.Property(x => x.Id).ValueGeneratedNever();
            
            builder.Property("Description")
                    .HasColumnType("varchar(100)")
                    .HasMaxLength(100);
            
            builder.Property("Url")
                    .HasColumnType("varchar(600)")
                    .HasMaxLength(600)
                    .IsRequired();

            builder.Property("IsActive").IsRequired();

            builder.Property("NumberOfProducts")
                    .IsRequired()
                    .HasDefaultValue(0);

            builder.HasOne(x => x.Category)
                    .WithMany(x => x.Links)
                    .HasForeignKey(x => x.CategoryId);
        }
    }
}
