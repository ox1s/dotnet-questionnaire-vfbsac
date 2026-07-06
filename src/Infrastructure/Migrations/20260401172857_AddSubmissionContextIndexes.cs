using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations;

public partial class AddSubmissionContextIndexes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateIndex(
            name: "ix_submissions_context_department_id",
            schema: "public",
            table: "submissions",
            column: "context_department_id");

        migrationBuilder.CreateIndex(
            name: "ix_submissions_context_speciality_id",
            schema: "public",
            table: "submissions",
            column: "context_speciality_id");

        migrationBuilder.CreateIndex(
            name: "ix_submissions_context_specialization_id",
            schema: "public",
            table: "submissions",
            column: "context_specialization_id");

        migrationBuilder.CreateIndex(
            name: "ix_submissions_discipline_id",
            schema: "public",
            table: "submissions",
            column: "discipline_id");

        migrationBuilder.CreateIndex(
            name: "ix_submissions_teacher_id",
            schema: "public",
            table: "submissions",
            column: "teacher_id");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "ix_submissions_context_department_id",
            schema: "public",
            table: "submissions");

        migrationBuilder.DropIndex(
            name: "ix_submissions_context_speciality_id",
            schema: "public",
            table: "submissions");

        migrationBuilder.DropIndex(
            name: "ix_submissions_context_specialization_id",
            schema: "public",
            table: "submissions");

        migrationBuilder.DropIndex(
            name: "ix_submissions_discipline_id",
            schema: "public",
            table: "submissions");

        migrationBuilder.DropIndex(
            name: "ix_submissions_teacher_id",
            schema: "public",
            table: "submissions");
    }
}
