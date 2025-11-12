using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Questionnaire.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixAnswerOptionRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QuestionOptions_AnswerDetails_AnswerDetailId",
                table: "QuestionOptions");

            migrationBuilder.DropIndex(
                name: "IX_QuestionOptions_AnswerDetailId",
                table: "QuestionOptions");

            migrationBuilder.DropColumn(
                name: "AnswerDetailId",
                table: "QuestionOptions");

            migrationBuilder.CreateTable(
                name: "AnswerDetailSelectedOptions",
                columns: table => new
                {
                    AnswerDetailId = table.Column<int>(type: "integer", nullable: false),
                    QuestionOptionId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnswerDetailSelectedOptions", x => new { x.AnswerDetailId, x.QuestionOptionId });
                    table.ForeignKey(
                        name: "FK_AnswerDetailSelectedOptions_AnswerDetails_AnswerDetailId",
                        column: x => x.AnswerDetailId,
                        principalTable: "AnswerDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AnswerDetailSelectedOptions_QuestionOptions_QuestionOptionId",
                        column: x => x.QuestionOptionId,
                        principalTable: "QuestionOptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnswerDetailSelectedOptions_QuestionOptionId",
                table: "AnswerDetailSelectedOptions",
                column: "QuestionOptionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnswerDetailSelectedOptions");

            migrationBuilder.AddColumn<int>(
                name: "AnswerDetailId",
                table: "QuestionOptions",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuestionOptions_AnswerDetailId",
                table: "QuestionOptions",
                column: "AnswerDetailId");

            migrationBuilder.AddForeignKey(
                name: "FK_QuestionOptions_AnswerDetails_AnswerDetailId",
                table: "QuestionOptions",
                column: "AnswerDetailId",
                principalTable: "AnswerDetails",
                principalColumn: "Id");
        }
    }
}
