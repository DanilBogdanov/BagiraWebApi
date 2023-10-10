using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BagiraWebApi.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexGoodPath : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Path",
                table: "Goods",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Goods_Path",
                table: "Goods",
                column: "Path");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Goods_Path",
                table: "Goods");

            migrationBuilder.AlterColumn<string>(
                name: "Path",
                table: "Goods",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
