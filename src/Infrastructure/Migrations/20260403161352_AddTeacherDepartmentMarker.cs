using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddTeacherDepartmentMarker : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(
            name: "department_id",
            schema: "public",
            table: "teachers",
            type: "uuid",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "ix_teachers_department_id",
            schema: "public",
            table: "teachers",
            column: "department_id");

        migrationBuilder.AddForeignKey(
            name: "fk_teachers_departments_department_id",
            schema: "public",
            table: "teachers",
            column: "department_id",
            principalSchema: "public",
            principalTable: "departments",
            principalColumn: "id",
            onDelete: ReferentialAction.Restrict);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "fk_teachers_departments_department_id",
            schema: "public",
            table: "teachers");

        migrationBuilder.DropIndex(
            name: "ix_teachers_department_id",
            schema: "public",
            table: "teachers");

        migrationBuilder.DropColumn(
            name: "department_id",
            schema: "public",
            table: "teachers");
    }
}
