using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShoppingFood.Migrations
{
    /// <inheritdoc />
    public partial class AddCapitalPricetoProductModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CapitalPrice",
                table: "Products",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CapitalPrice",
                table: "Products");
        }
    }
}
