using Microsoft.EntityFrameworkCore.Migrations;

namespace ProductCatalog.Infra.Data.Persistance.Migrations
{
    public partial class AddMergedProductsIdColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MergedProductsId",
                table: "ProductsDetail",
                type: "varchar(4000)",
                maxLength: 4000,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MergedProductsId",
                table: "ProductsDetail");
        }
    }
}
