using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuizAppBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddGameSessionState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CurrentQuestionIndex",
                table: "GameSessions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "QuestionOrderJson",
                table: "GameSessions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentQuestionIndex",
                table: "GameSessions");

            migrationBuilder.DropColumn(
                name: "QuestionOrderJson",
                table: "GameSessions");
        }
    }
}
