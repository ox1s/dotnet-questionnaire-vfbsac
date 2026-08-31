using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations;

/// <inheritdoc />
public partial class RenumberQuestionTypeAndUserRole : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // QuestionType.WeightedRating was renumbered from 6 to 3 to close the
        // numbering gaps left after MultipleChoice (3), SingleChoice (4) and
        // Rating (5) were all removed; backfill any existing rows.
        migrationBuilder.Sql(
            "UPDATE public.questions SET type = 3 WHERE type = 6;");

        // UserRole was renumbered to close the gap left after DeputyHead (2)
        // was removed; backfill forms.target_role (the only int-backed column
        // that stores this enum - Users.Role is stored as a string).
        migrationBuilder.Sql(
            """
            UPDATE public.forms SET target_role = CASE target_role
                WHEN 3 THEN 2
                WHEN 4 THEN 3
                WHEN 5 THEN 4
                ELSE target_role
            END
            WHERE target_role IN (3, 4, 5);
            """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            "UPDATE public.questions SET type = 6 WHERE type = 3;");

        migrationBuilder.Sql(
            """
            UPDATE public.forms SET target_role = CASE target_role
                WHEN 2 THEN 3
                WHEN 3 THEN 4
                WHEN 4 THEN 5
                ELSE target_role
            END
            WHERE target_role IN (2, 3, 4);
            """);
    }
}
