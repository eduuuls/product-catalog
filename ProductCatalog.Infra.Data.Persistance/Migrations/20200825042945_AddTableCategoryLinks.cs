using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ProductCatalog.Infra.Data.Persistance.Migrations
{
    public partial class AddTableCategoryLinks : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SubType",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "Url",
                table: "Categories");

            migrationBuilder.CreateTable(
                name: "CategoryLink",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CategoryId = table.Column<Guid>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    Url = table.Column<string>(nullable: true),
                    NumberOfProducts = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoryLink", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CategoryLink_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CategoryLink_CategoryId",
                table: "CategoryLink",
                column: "CategoryId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CategoryLink");

            migrationBuilder.AddColumn<string>(
                name: "SubType",
                table: "Categories",
                type: "varchar(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Url",
                table: "Categories",
                type: "varchar(600)",
                maxLength: 600,
                nullable: false,
                defaultValue: "");
        }
    }
}
