using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BagiraWebApi.Migrations
{
    /// <inheritdoc />
    public partial class AddTableGoodPropertyValues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GoodPropertyValues",
                columns: table => new
                {
                    GoodId = table.Column<int>(type: "int", nullable: false),
                    PropertyId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ValueId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GoodPropertyValues", x => new { x.GoodId, x.PropertyId });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GoodPropertyValues");
        }
    }
}
