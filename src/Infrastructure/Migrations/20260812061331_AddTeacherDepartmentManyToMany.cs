using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddTeacherDepartmentManyToMany : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Create the join table first so existing single-department assignments
        // can be migrated into it before the legacy column is dropped.
        migrationBuilder.CreateTable(
            name: "teacher_departments",
            schema: "public",
            columns: table => new
            {
                teacher_id = table.Column<Guid>(type: "uuid", nullable: false),
                department_id = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_teacher_departments", x => new { x.teacher_id, x.department_id });
                table.ForeignKey(
                    name: "fk_teacher_departments_departments_department_id",
                    column: x => x.department_id,
                    principalSchema: "public",
                    principalTable: "departments",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_teacher_departments_teachers_teacher_id",
                    column: x => x.teacher_id,
                    principalSchema: "public",
                    principalTable: "teachers",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "ix_teacher_departments_department_id",
            schema: "public",
            table: "teacher_departments",
            column: "department_id");

        // Migrate existing single-department assignments into the join table before
        // the legacy "department_id" column on "teachers" is dropped below.
        migrationBuilder.Sql(
            """
            INSERT INTO public.teacher_departments (teacher_id, department_id)
            SELECT id, department_id
            FROM public.teachers
            WHERE department_id IS NOT NULL;
            """);

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

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(
            name: "department_id",
            schema: "public",
            table: "teachers",
            type: "uuid",
            nullable: true);

        // Best-effort data restore: a teacher linked to multiple departments can only keep
        // one on the legacy single-FK column, so the lowest department id is kept deterministically.
        migrationBuilder.Sql(
            """
            UPDATE public.teachers AS t
            SET department_id = (
                SELECT td.department_id
                FROM public.teacher_departments AS td
                WHERE td.teacher_id = t.id
                ORDER BY td.department_id
                LIMIT 1
            );
            """);

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

        migrationBuilder.DropTable(
            name: "teacher_departments",
            schema: "public");
    }
}
