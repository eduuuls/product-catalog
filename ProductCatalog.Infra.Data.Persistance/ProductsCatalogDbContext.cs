using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using ProductCatalog.Domain.Entities;
using ProductCatalog.Domain.Events.Base;
using ProductCatalog.Infra.Data.Persistance.Mapping;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace ProductCatalog.Infra.Data.Persistance
{
    public class ProductsCatalogDbContext : DbContext
    {
        public ProductsCatalogDbContext(DbContextOptions<ProductsCatalogDbContext> options)
             : base(options)
        {
            
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Ignore<ValidationResult>();
            modelBuilder.Ignore<Event>();

            modelBuilder.ApplyConfiguration(new CategoriesMapping());
            modelBuilder.ApplyConfiguration(new ProductsDetailMapping());
            modelBuilder.ApplyConfiguration(new ProductsMapping());
            modelBuilder.ApplyConfiguration(new ProductsReviewsMapping());
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductDetail> ProductsDetail { get; set; }
        public DbSet<ProductReview> ProductsReviews { get; set; }
    }
}