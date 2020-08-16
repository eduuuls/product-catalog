using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductCatalog.Domain.Entities;
using System.Diagnostics.CodeAnalysis;

namespace ProductCatalog.Infra.Data.Persistance.Mapping
{
    [ExcludeFromCodeCoverage]
    public class CategoriesMapping : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.ToTable("Categories");
            builder.HasKey("Id");

            builder.Property(x => x.Id).ValueGeneratedNever();
            
            builder.Property("Name")
                    .HasColumnType("varchar(80)")
                    .HasMaxLength(80)
                    .IsRequired();

            builder.Property("SubType")
                    .HasColumnType("varchar(80)")
                    .HasMaxLength(80);

            builder.Property("Description")
                    .HasColumnType("varchar(100)")
                    .HasMaxLength(100);
            
            builder.Property("Url")
                    .HasColumnType("varchar(600)")
                    .HasMaxLength(600)
                    .IsRequired();
            
            builder.Property("ImageUrl")
                    .HasColumnType("varchar(600)")
                    .HasMaxLength(600);
            
            builder.Property("IsActive").IsRequired();
            
            builder.Property("NumberOfProducts")
                    .IsRequired()
                    .HasDefaultValue(0);
            
            builder.Property("DataProvider");
            
            builder.HasMany(x => x.Products);
        }
    }
}
