using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations;

/// <inheritdoc />
public partial class RenumberWeightedRatingQuestionType : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // QuestionType.WeightedRating was renumbered from 6 to 5 to close the gap
        // left after QuestionType.Rating (5) was removed; backfill any existing rows.
        migrationBuilder.Sql(
            "UPDATE public.questions SET type = 5 WHERE type = 6;");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            "UPDATE public.questions SET type = 6 WHERE type = 5;");
    }
}
