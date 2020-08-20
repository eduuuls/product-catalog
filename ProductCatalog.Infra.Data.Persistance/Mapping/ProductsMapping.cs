using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductCatalog.Domain.Entities;
using System.Diagnostics.CodeAnalysis;

namespace ProductCatalog.Infra.Data.Persistance.Mapping
{
    public class ProductsMapping : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.ToTable("Products");
            builder.HasKey("Id");
            
            builder.Property(x => x.Id).ValueGeneratedNever();
            
            builder.Property("Name")
                    .HasColumnType("varchar(200)")
                    .IsRequired();
            
            builder.Property("ExternalId")
                    .HasColumnType("varchar(50)")
                    .HasMaxLength(50);
            
            builder.Property("Description")
                    .HasColumnType("varchar(300)");
            
            builder.Property("Url")
                    .HasColumnType("varchar(1000)")
                    .HasMaxLength(1000);

            builder.Property("ImageUrl")
                    .HasColumnType("varchar(1000)")
                    .HasMaxLength(1000)
                        .IsRequired();

            builder.Property("DataProvider")
                        .IsRequired();

            builder.Property("RelevancePoints")
                        .IsRequired();

            builder.HasOne(x => x.ProductCategory)
                    .WithMany(x=> x.Products)
                    .HasForeignKey(c => c.CategoryId)
                    .IsRequired();
        }
    }
}
