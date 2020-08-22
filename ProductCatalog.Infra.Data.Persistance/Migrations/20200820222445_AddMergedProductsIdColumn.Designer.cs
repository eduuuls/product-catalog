﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ProductCatalog.Infra.Data.Persistance;

namespace ProductCatalog.Infra.Data.Persistance.Migrations
{
    [DbContext(typeof(ProductsCatalogDbContext))]
    [Migration("20200820222445_AddMergedProductsIdColumn")]
    partial class AddMergedProductsIdColumn
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.6")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("ProductCatalog.Domain.Entities.Category", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("DataProvider")
                        .HasColumnType("int");

                    b.Property<string>("Description")
                        .HasColumnType("varchar(100)")
                        .HasMaxLength(100);

                    b.Property<string>("ImageUrl")
                        .HasColumnType("varchar(600)")
                        .HasMaxLength(600);

                    b.Property<bool>("IsActive")
                        .HasColumnType("bit");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("varchar(80)")
                        .HasMaxLength(80);

                    b.Property<int>("NumberOfProducts")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasDefaultValue(0);

                    b.Property<string>("SubType")
                        .HasColumnType("varchar(80)")
                        .HasMaxLength(80);

                    b.Property<string>("Url")
                        .IsRequired()
                        .HasColumnType("varchar(600)")
                        .HasMaxLength(600);

                    b.HasKey("Id");

                    b.ToTable("Categories");
                });

            modelBuilder.Entity("ProductCatalog.Domain.Entities.Product", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("CategoryId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("DataProvider")
                        .HasColumnType("int");

                    b.Property<string>("Description")
                        .HasColumnType("varchar(300)");

                    b.Property<string>("ExternalId")
                        .HasColumnType("varchar(50)")
                        .HasMaxLength(50);

                    b.Property<string>("ImageUrl")
                        .IsRequired()
                        .HasColumnType("varchar(1000)")
                        .HasMaxLength(1000);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("varchar(200)");

                    b.Property<int>("RelevancePoints")
                        .HasColumnType("int");

                    b.Property<string>("Url")
                        .HasColumnType("varchar(1000)")
                        .HasMaxLength(1000);

                    b.HasKey("Id");

                    b.HasIndex("CategoryId");

                    b.ToTable("Products");
                });

            modelBuilder.Entity("ProductCatalog.Domain.Entities.ProductDetail", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("BarCode")
                        .HasColumnType("varchar(500)")
                        .HasMaxLength(500);

                    b.Property<string>("Brand")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Code")
                        .HasColumnType("varchar(50)")
                        .HasMaxLength(50);

                    b.Property<string>("Manufacturer")
                        .HasColumnType("varchar(200)")
                        .HasMaxLength(200);

                    b.Property<string>("MergedProductsId")
                        .HasColumnType("varchar(4000)")
                        .HasMaxLength(4000);

                    b.Property<string>("Model")
                        .HasColumnType("varchar(100)")
                        .HasMaxLength(100);

                    b.Property<string>("OtherSpecs")
                        .HasColumnType("varchar(8000)")
                        .HasMaxLength(8000);

                    b.Property<Guid>("ProductId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("ReferenceModel")
                        .HasColumnType("varchar(100)")
                        .HasMaxLength(100);

                    b.Property<string>("Supplier")
                        .HasColumnType("varchar(200)")
                        .HasMaxLength(200);

                    b.HasKey("Id");

                    b.HasIndex("ProductId")
                        .IsUnique();

                    b.ToTable("ProductsDetail");
                });

            modelBuilder.Entity("ProductCatalog.Domain.Entities.ProductReview", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime?>("Date")
                        .HasColumnType("datetime2");

                    b.Property<string>("ExternalId")
                        .HasColumnType("varchar(50)")
                        .HasMaxLength(50);

                    b.Property<bool?>("IsRecommended")
                        .HasColumnType("bit");

                    b.Property<Guid>("ProductId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Result")
                        .HasColumnType("varchar(30)")
                        .HasMaxLength(30);

                    b.Property<string>("Reviewer")
                        .HasColumnType("varchar(50)")
                        .HasMaxLength(20);

                    b.Property<short?>("Stars")
                        .HasColumnType("smallint");

                    b.Property<string>("Text")
                        .HasColumnType("varchar(2000)")
                        .HasMaxLength(2000);

                    b.Property<string>("Title")
                        .HasColumnType("varchar(500)")
                        .HasMaxLength(500);

                    b.HasKey("Id");

                    b.HasIndex("ProductId");

                    b.ToTable("ProductsReviews");
                });

            modelBuilder.Entity("ProductCatalog.Domain.Entities.Product", b =>
                {
                    b.HasOne("ProductCatalog.Domain.Entities.Category", "ProductCategory")
                        .WithMany("Products")
                        .HasForeignKey("CategoryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("ProductCatalog.Domain.Entities.ProductDetail", b =>
                {
                    b.HasOne("ProductCatalog.Domain.Entities.Product", "Product")
                        .WithOne("Detail")
                        .HasForeignKey("ProductCatalog.Domain.Entities.ProductDetail", "ProductId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("ProductCatalog.Domain.Entities.ProductReview", b =>
                {
                    b.HasOne("ProductCatalog.Domain.Entities.Product", "Product")
                        .WithMany("Reviews")
                        .HasForeignKey("ProductId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
