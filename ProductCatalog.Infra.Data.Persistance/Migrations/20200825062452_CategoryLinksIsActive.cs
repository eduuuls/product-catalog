using Microsoft.EntityFrameworkCore.Migrations;

namespace ProductCatalog.Infra.Data.Persistance.Migrations
{
    public partial class CategoryLinksIsActive : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "CategoryLink",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "CategoryLink");
        }
    }
}
