using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShoppingFood.Migrations
{
    /// <inheritdoc />
    public partial class EditMenuModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Action",
                table: "Menus",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Controller",
                table: "Menus",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Url",
                table: "Menus",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Action",
                table: "Menus");

            migrationBuilder.DropColumn(
                name: "Controller",
                table: "Menus");

            migrationBuilder.DropColumn(
                name: "Url",
                table: "Menus");
        }
    }
}
