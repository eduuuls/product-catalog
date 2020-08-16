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
            
            builder.Property("Reviewer")
                    .HasColumnType("varchar(20)")
                    .HasMaxLength(20);
            
            builder.Property("Date");
            
            builder.Property("Title")
                    .HasColumnType("varchar(100)")
                    .HasMaxLength(100);
            
            builder.Property("Text")
                    .HasColumnType("varchar(1000)")
                    .HasMaxLength(1000);
            
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
