using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations;

public partial class AddAnalyticsIndexes : Migration
{
    private static readonly string[] SubmissionPeriodIndexColumns = ["form_id", "submitted_at"];

    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateIndex(
            name: "ix_submissions_form_id",
            schema: "public",
            table: "submissions",
            column: "form_id");

        migrationBuilder.CreateIndex(
            name: "ix_submissions_form_id_submitted_at",
            schema: "public",
            table: "submissions",
            columns: SubmissionPeriodIndexColumns);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "ix_submissions_form_id",
            schema: "public",
            table: "submissions");

        migrationBuilder.DropIndex(
            name: "ix_submissions_form_id_submitted_at",
            schema: "public",
            table: "submissions");
    }
}
