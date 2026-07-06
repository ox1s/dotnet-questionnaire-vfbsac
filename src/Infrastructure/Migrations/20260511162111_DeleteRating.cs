using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations;

/// <inheritdoc />
public partial class DeleteRating : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "fk_answer_submissions_submission_id",
            schema: "public",
            table: "answer");

        migrationBuilder.DropForeignKey(
            name: "fk_question_forms_form_id",
            schema: "public",
            table: "question");

        migrationBuilder.DropPrimaryKey(
            name: "pk_question",
            schema: "public",
            table: "question");

        migrationBuilder.DropPrimaryKey(
            name: "pk_answer",
            schema: "public",
            table: "answer");

        migrationBuilder.RenameTable(
            name: "question",
            schema: "public",
            newName: "questions",
            newSchema: "public");

        migrationBuilder.RenameTable(
            name: "answer",
            schema: "public",
            newName: "answers",
            newSchema: "public");

        migrationBuilder.RenameIndex(
            name: "ix_question_form_id_order",
            schema: "public",
            table: "questions",
            newName: "ix_questions_form_id_order");

        migrationBuilder.RenameIndex(
            name: "ix_answer_submission_id_question_id",
            schema: "public",
            table: "answers",
            newName: "ix_answers_submission_id_question_id");

        migrationBuilder.AddPrimaryKey(
            name: "pk_questions",
            schema: "public",
            table: "questions",
            column: "id");

        migrationBuilder.AddPrimaryKey(
            name: "pk_answers",
            schema: "public",
            table: "answers",
            column: "id");

        migrationBuilder.AddForeignKey(
            name: "fk_answers_submissions_submission_id",
            schema: "public",
            table: "answers",
            column: "submission_id",
            principalSchema: "public",
            principalTable: "submissions",
            principalColumn: "id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "fk_questions_forms_form_id",
            schema: "public",
            table: "questions",
            column: "form_id",
            principalSchema: "public",
            principalTable: "forms",
            principalColumn: "id",
            onDelete: ReferentialAction.Cascade);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "fk_answers_submissions_submission_id",
            schema: "public",
            table: "answers");

        migrationBuilder.DropForeignKey(
            name: "fk_questions_forms_form_id",
            schema: "public",
            table: "questions");

        migrationBuilder.DropPrimaryKey(
            name: "pk_questions",
            schema: "public",
            table: "questions");

        migrationBuilder.DropPrimaryKey(
            name: "pk_answers",
            schema: "public",
            table: "answers");

        migrationBuilder.RenameTable(
            name: "questions",
            schema: "public",
            newName: "question",
            newSchema: "public");

        migrationBuilder.RenameTable(
            name: "answers",
            schema: "public",
            newName: "answer",
            newSchema: "public");

        migrationBuilder.RenameIndex(
            name: "ix_questions_form_id_order",
            schema: "public",
            table: "question",
            newName: "ix_question_form_id_order");

        migrationBuilder.RenameIndex(
            name: "ix_answers_submission_id_question_id",
            schema: "public",
            table: "answer",
            newName: "ix_answer_submission_id_question_id");

        migrationBuilder.AddPrimaryKey(
            name: "pk_question",
            schema: "public",
            table: "question",
            column: "id");

        migrationBuilder.AddPrimaryKey(
            name: "pk_answer",
            schema: "public",
            table: "answer",
            column: "id");

        migrationBuilder.AddForeignKey(
            name: "fk_answer_submissions_submission_id",
            schema: "public",
            table: "answer",
            column: "submission_id",
            principalSchema: "public",
            principalTable: "submissions",
            principalColumn: "id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "fk_question_forms_form_id",
            schema: "public",
            table: "question",
            column: "form_id",
            principalSchema: "public",
            principalTable: "forms",
            principalColumn: "id",
            onDelete: ReferentialAction.Cascade);
    }
}
