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
                    .HasColumnType("varchar(100)")
                    .IsRequired();
            
            builder.Property("ExternalId")
                    .HasColumnType("varchar(30)")
                    .HasMaxLength(30);
            
            builder.Property("Description")
                    .HasColumnType("varchar(200)");
            
            builder.Property("Url")
                    .HasColumnType("varchar(600)")
                    .HasMaxLength(600);

            builder.Property("ImageUrl")
                    .HasColumnType("varchar(600)")
                    .HasMaxLength(600)
                        .IsRequired();

            builder.Property("DataProvider");
            
            builder.HasOne(x => x.ProductCategory)
                    .WithMany(x=> x.Products)
                    .HasForeignKey(c => c.CategoryId)
                    .IsRequired();
        }
    }
}
