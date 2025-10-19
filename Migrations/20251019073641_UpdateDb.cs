using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HeThongQuanLyThi.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Points",
                table: "Problems");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Points",
                table: "Problems",
                type: "decimal(9,2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
