using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations;

/// <inheritdoc />
public partial class CreateFiltersForSoftDelete : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "is_deleted",
            schema: "public",
            table: "users",
            type: "boolean",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "is_deleted",
            schema: "public",
            table: "teachers",
            type: "boolean",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "is_deleted",
            schema: "public",
            table: "submissions",
            type: "boolean",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "is_deleted",
            schema: "public",
            table: "specializations",
            type: "boolean",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "is_deleted",
            schema: "public",
            table: "specialities",
            type: "boolean",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "is_deleted",
            schema: "public",
            table: "question",
            type: "boolean",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "is_deleted",
            schema: "public",
            table: "OutboxMessages",
            type: "boolean",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "is_deleted",
            schema: "public",
            table: "forms",
            type: "boolean",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "is_deleted",
            schema: "public",
            table: "disciplines",
            type: "boolean",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "is_deleted",
            schema: "public",
            table: "departments",
            type: "boolean",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "is_deleted",
            schema: "public",
            table: "answer",
            type: "boolean",
            nullable: false,
            defaultValue: false);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "is_deleted",
            schema: "public",
            table: "users");

        migrationBuilder.DropColumn(
            name: "is_deleted",
            schema: "public",
            table: "teachers");

        migrationBuilder.DropColumn(
            name: "is_deleted",
            schema: "public",
            table: "submissions");

        migrationBuilder.DropColumn(
            name: "is_deleted",
            schema: "public",
            table: "specializations");

        migrationBuilder.DropColumn(
            name: "is_deleted",
            schema: "public",
            table: "specialities");

        migrationBuilder.DropColumn(
            name: "is_deleted",
            schema: "public",
            table: "question");

        migrationBuilder.DropColumn(
            name: "is_deleted",
            schema: "public",
            table: "OutboxMessages");

        migrationBuilder.DropColumn(
            name: "is_deleted",
            schema: "public",
            table: "forms");

        migrationBuilder.DropColumn(
            name: "is_deleted",
            schema: "public",
            table: "disciplines");

        migrationBuilder.DropColumn(
            name: "is_deleted",
            schema: "public",
            table: "departments");

        migrationBuilder.DropColumn(
            name: "is_deleted",
            schema: "public",
            table: "answer");
    }
}
