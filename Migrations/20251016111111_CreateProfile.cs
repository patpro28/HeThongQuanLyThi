using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HeThongQuanLyThi.Migrations
{
    /// <inheritdoc />
    public partial class CreateProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Profiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    FullName = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Gender = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Address = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Phone = table.Column<string>(type: "TEXT", maxLength: 30, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Profiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StudentInfos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProfileId = table.Column<int>(type: "INTEGER", nullable: false),
                    GradeLevel = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    ClassName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    EnrollmentYear = table.Column<int>(type: "INTEGER", nullable: true),
                    ParentPhone = table.Column<string>(type: "TEXT", maxLength: 30, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentInfos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentInfos_Profiles_ProfileId",
                        column: x => x.ProfileId,
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TeacherInfos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProfileId = table.Column<int>(type: "INTEGER", nullable: false),
                    Subject = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Degree = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    HireDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    HomeroomClass = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeacherInfos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeacherInfos_Profiles_ProfileId",
                        column: x => x.ProfileId,
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Profiles_UserId",
                table: "Profiles",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentInfos_ProfileId",
                table: "StudentInfos",
                column: "ProfileId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeacherInfos_ProfileId",
                table: "TeacherInfos",
                column: "ProfileId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StudentInfos");

            migrationBuilder.DropTable(
                name: "TeacherInfos");

            migrationBuilder.DropTable(
                name: "Profiles");
        }
    }
}
