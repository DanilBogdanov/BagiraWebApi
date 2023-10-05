using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BagiraWebApi.Migrations
{
    /// <inheritdoc />
    public partial class AddImageToGood : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImgDataVersion",
                table: "Goods",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImgExt",
                table: "Goods",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImgUrl",
                table: "Goods",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImgDataVersion",
                table: "Goods");

            migrationBuilder.DropColumn(
                name: "ImgExt",
                table: "Goods");

            migrationBuilder.DropColumn(
                name: "ImgUrl",
                table: "Goods");
        }
    }
}
