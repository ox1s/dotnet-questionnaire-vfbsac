using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations;

/// <inheritdoc />
public partial class RemoveTeacherDepartment : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "department_id",
            schema: "public",
            table: "teachers");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(
            name: "department_id",
            schema: "public",
            table: "teachers",
            type: "uuid",
            nullable: false,
            defaultValue: Guid.Empty);
    }
}
