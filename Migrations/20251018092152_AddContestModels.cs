using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HeThongQuanLyThi.Migrations
{
    /// <inheritdoc />
    public partial class AddContestModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Contests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    SubjectId = table.Column<int>(type: "INTEGER", nullable: false),
                    AuthorProfileId = table.Column<int>(type: "INTEGER", nullable: true),
                    StartAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TimeLimitMinutes = table.Column<int>(type: "INTEGER", nullable: false),
                    MaxAttemptsPerStudent = table.Column<int>(type: "INTEGER", nullable: false),
                    IsPublished = table.Column<bool>(type: "INTEGER", nullable: false),
                    ShuffleQuestions = table.Column<bool>(type: "INTEGER", nullable: false),
                    ShuffleChoices = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Contests_Profiles_AuthorProfileId",
                        column: x => x.AuthorProfileId,
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Contests_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ContestAttempts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ContestId = table.Column<int>(type: "INTEGER", nullable: false),
                    StudentProfileId = table.Column<int>(type: "INTEGER", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Score = table.Column<decimal>(type: "decimal(9,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContestAttempts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContestAttempts_Contests_ContestId",
                        column: x => x.ContestId,
                        principalTable: "Contests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContestAttempts_Profiles_StudentProfileId",
                        column: x => x.StudentProfileId,
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContestProblems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ContestId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProblemId = table.Column<int>(type: "INTEGER", nullable: false),
                    Order = table.Column<int>(type: "INTEGER", nullable: false),
                    PointsOverride = table.Column<decimal>(type: "decimal(9,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContestProblems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContestProblems_Contests_ContestId",
                        column: x => x.ContestId,
                        principalTable: "Contests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContestProblems_Problems_ProblemId",
                        column: x => x.ProblemId,
                        principalTable: "Problems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ContestAnswers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AttemptId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProblemId = table.Column<int>(type: "INTEGER", nullable: false),
                    SelectedChoiceId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContestAnswers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContestAnswers_ContestAttempts_AttemptId",
                        column: x => x.AttemptId,
                        principalTable: "ContestAttempts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContestAnswers_ProblemChoices_SelectedChoiceId",
                        column: x => x.SelectedChoiceId,
                        principalTable: "ProblemChoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContestAnswers_Problems_ProblemId",
                        column: x => x.ProblemId,
                        principalTable: "Problems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContestAnswers_AttemptId_ProblemId",
                table: "ContestAnswers",
                columns: new[] { "AttemptId", "ProblemId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContestAnswers_ProblemId",
                table: "ContestAnswers",
                column: "ProblemId");

            migrationBuilder.CreateIndex(
                name: "IX_ContestAnswers_SelectedChoiceId",
                table: "ContestAnswers",
                column: "SelectedChoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_ContestAttempts_ContestId_StudentProfileId",
                table: "ContestAttempts",
                columns: new[] { "ContestId", "StudentProfileId" });

            migrationBuilder.CreateIndex(
                name: "IX_ContestAttempts_StudentProfileId",
                table: "ContestAttempts",
                column: "StudentProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_ContestProblems_ContestId_Order",
                table: "ContestProblems",
                columns: new[] { "ContestId", "Order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContestProblems_ContestId_ProblemId",
                table: "ContestProblems",
                columns: new[] { "ContestId", "ProblemId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContestProblems_ProblemId",
                table: "ContestProblems",
                column: "ProblemId");

            migrationBuilder.CreateIndex(
                name: "IX_Contests_AuthorProfileId",
                table: "Contests",
                column: "AuthorProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Contests_SubjectId_IsPublished_StartAt",
                table: "Contests",
                columns: new[] { "SubjectId", "IsPublished", "StartAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContestAnswers");

            migrationBuilder.DropTable(
                name: "ContestProblems");

            migrationBuilder.DropTable(
                name: "ContestAttempts");

            migrationBuilder.DropTable(
                name: "Contests");
        }
    }
}
