using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BagiraWebApi.Migrations
{
    /// <inheritdoc />
    public partial class RemovetableParserCompanies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ParserCompanies");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastUpdated",
                table: "ParserGoods",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastUpdated",
                table: "ParserGoods");

            migrationBuilder.CreateTable(
                name: "ParserCompanies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParserCompanies", x => x.Id);
                });
        }
    }
}
