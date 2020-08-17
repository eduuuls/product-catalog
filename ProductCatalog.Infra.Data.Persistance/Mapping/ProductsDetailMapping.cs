using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductCatalog.Domain.Entities;
using System.Diagnostics.CodeAnalysis;

namespace ProductCatalog.Infra.Data.Persistance.Mapping
{
    public class ProductsDetailMapping : IEntityTypeConfiguration<ProductDetail>
    {
        public void Configure(EntityTypeBuilder<ProductDetail> builder)
        {
            builder.ToTable("ProductsDetail");
            builder.HasKey("Id");
            builder.Property(x => x.Id).ValueGeneratedNever();
            
            builder.Property("Code")
                    .HasColumnType("varchar(50)")
                    .HasMaxLength(50);
            
            builder.Property("BarCode")
                    .HasColumnType("varchar(500)")
                    .HasMaxLength(500);
            
            builder.Property("Manufacturer")
                    .HasColumnType("varchar(200)")
                    .HasMaxLength(200);
            
            builder.Property("Supplier")
                    .HasColumnType("varchar(200)")
                    .HasMaxLength(200);
            
            builder.Property("Model")
                    .HasColumnType("varchar(100)")
                    .HasMaxLength(100);
            
            builder.Property("ReferenceModel")
                    .HasColumnType("varchar(100)")
                    .HasMaxLength(100);
            
            builder.Property("OtherSpecs")
                    .HasColumnType("varchar(8000)")
                        .HasMaxLength(8000);
            
            builder.Property("ProductId");
            
            builder.HasOne(x => x.Product)
                    .WithOne(x=> x.Detail); 
        }
    }
}
