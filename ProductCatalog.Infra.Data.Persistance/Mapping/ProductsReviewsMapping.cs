using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductCatalog.Domain.Entities;
using System.Diagnostics.CodeAnalysis;

namespace ProductCatalog.Infra.Data.Persistance.Mapping
{
    public class ProductsReviewsMapping : IEntityTypeConfiguration<ProductReview>
    {
        public void Configure(EntityTypeBuilder<ProductReview> builder)
        {
            builder.ToTable("ProductsReviews");
            builder.HasKey("Id");
            
            builder.Property(x => x.Id).ValueGeneratedNever();

            builder.Property("ExternalId")
                    .HasColumnType("varchar(50)")
                    .HasMaxLength(50);

            builder.Property("Reviewer")
                    .HasColumnType("varchar(50)")
                    .HasMaxLength(20);
            
            builder.Property("Date");
            
            builder.Property("Title")
                    .HasColumnType("varchar(500)")
                    .HasMaxLength(500);
            
            builder.Property("Text")
                    .HasColumnType("varchar(2000)")
                    .HasMaxLength(2000);
            
            builder.Property("Stars");
            
            builder.Property("Result")
                    .HasColumnType("varchar(30)")
                    .HasMaxLength(30);
            
            builder.Property("IsRecommended");
            
            builder.HasOne(x => x.Product)
                    .WithMany(x=> x.Reviews)
                    .HasForeignKey(x=> x.ProductId);
        }
    }
}
