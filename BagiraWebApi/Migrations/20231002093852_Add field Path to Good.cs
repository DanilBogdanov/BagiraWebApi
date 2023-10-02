using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BagiraWebApi.Migrations
{
    /// <inheritdoc />
    public partial class AddfieldPathtoGood : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Path",
                table: "Goods",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Path",
                table: "Goods");
        }
    }
}
