using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HeThongQuanLyThi.Migrations
{
    /// <inheritdoc />
    public partial class AddProblemAndChoices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Problems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Content = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    Explanation = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    Points = table.Column<decimal>(type: "decimal(9,2)", nullable: false),
                    SubjectId = table.Column<int>(type: "INTEGER", nullable: true),
                    AuthorProfileId = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Problems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Problems_Profiles_AuthorProfileId",
                        column: x => x.AuthorProfileId,
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Problems_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ProblemChoices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProblemId = table.Column<int>(type: "INTEGER", nullable: false),
                    Text = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    Explanation = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    IsCorrect = table.Column<bool>(type: "INTEGER", nullable: false),
                    Order = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProblemChoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProblemChoices_Problems_ProblemId",
                        column: x => x.ProblemId,
                        principalTable: "Problems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProblemChoices_ProblemId_Order",
                table: "ProblemChoices",
                columns: new[] { "ProblemId", "Order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Problems_AuthorProfileId",
                table: "Problems",
                column: "AuthorProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Problems_SubjectId",
                table: "Problems",
                column: "SubjectId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProblemChoices");

            migrationBuilder.DropTable(
                name: "Problems");
        }
    }
}
