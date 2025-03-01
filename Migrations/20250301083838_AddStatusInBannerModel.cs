using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShoppingFood.Migrations
{
    /// <inheritdoc />
    public partial class AddStatusInBannerModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Banners",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Banners");
        }
    }
}
