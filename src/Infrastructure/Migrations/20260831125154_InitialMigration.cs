using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations;

/// <inheritdoc />
public partial class InitialMigration : Migration
{
    private static readonly string[] AnswerSubmissionQuestionIndexColumns = ["submission_id", "question_id"];
    private static readonly string[] QuestionFormOrderIndexColumns = ["form_id", "order"];
    private static readonly string[] SubmissionPeriodIndexColumns = ["form_id", "submitted_at"];

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "public");

        migrationBuilder.CreateTable(
            name: "departments",
            schema: "public",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                is_deleted = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_departments", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "forms",
            schema: "public",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                is_active = table.Column<bool>(type: "boolean", nullable: false),
                is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                required_filters = table.Column<string>(type: "text", nullable: true),
                target_role = table.Column<int>(type: "integer", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_forms", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "specialities",
            schema: "public",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                is_deleted = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_specialities", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "specializations",
            schema: "public",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                speciality_id = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_specializations", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "submissions",
            schema: "public",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                submitted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                discipline_id = table.Column<Guid>(type: "uuid", nullable: true),
                teacher_id = table.Column<Guid>(type: "uuid", nullable: true),
                context_department_id = table.Column<Guid>(type: "uuid", nullable: true),
                context_speciality_id = table.Column<Guid>(type: "uuid", nullable: true),
                context_specialization_id = table.Column<Guid>(type: "uuid", nullable: true),
                context_organization_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                context_education_form = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                context_employee_category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                context_position = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                form_id = table.Column<Guid>(type: "uuid", nullable: false),
                user_id = table.Column<Guid>(type: "uuid", nullable: false),
                device_id = table.Column<string>(type: "text", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_submissions", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "teachers",
            schema: "public",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                full_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                is_deleted = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_teachers", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "users",
            schema: "public",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                Login = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                password_hash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                role = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                is_active = table.Column<bool>(type: "boolean", nullable: false),
                display_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                department_id = table.Column<Guid>(type: "uuid", nullable: true),
                group_id = table.Column<Guid>(type: "uuid", nullable: true),
                teacher_id = table.Column<Guid>(type: "uuid", nullable: true),
                organization_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_users", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "disciplines",
            schema: "public",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                department_id = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_disciplines", x => x.id);
                table.ForeignKey(
                    name: "fk_disciplines_departments_department_id",
                    column: x => x.department_id,
                    principalSchema: "public",
                    principalTable: "departments",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "questions",
            schema: "public",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                text = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                type = table.Column<int>(type: "integer", nullable: false),
                order = table.Column<int>(type: "integer", nullable: false),
                is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                form_id = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_questions", x => x.id);
                table.ForeignKey(
                    name: "fk_questions_forms_form_id",
                    column: x => x.form_id,
                    principalSchema: "public",
                    principalTable: "forms",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "answers",
            schema: "public",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                submission_id = table.Column<Guid>(type: "uuid", nullable: false),
                question_id = table.Column<Guid>(type: "uuid", nullable: false),
                value = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: true),
                numeric_value = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                weight = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                is_deleted = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_answers", x => x.id);
                table.ForeignKey(
                    name: "fk_answers_submissions_submission_id",
                    column: x => x.submission_id,
                    principalSchema: "public",
                    principalTable: "submissions",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

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
            name: "ix_answers_submission_id_question_id",
            schema: "public",
            table: "answers",
            columns: AnswerSubmissionQuestionIndexColumns,
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_departments_name",
            schema: "public",
            table: "departments",
            column: "name",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_disciplines_department_id",
            schema: "public",
            table: "disciplines",
            column: "department_id");

        migrationBuilder.CreateIndex(
            name: "ix_disciplines_name",
            schema: "public",
            table: "disciplines",
            column: "name",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_questions_form_id_order",
            schema: "public",
            table: "questions",
            columns: QuestionFormOrderIndexColumns,
            unique: true);

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
            name: "ix_submissions_form_id",
            schema: "public",
            table: "submissions",
            column: "form_id");

        migrationBuilder.CreateIndex(
            name: "ix_submissions_form_id_submitted_at",
            schema: "public",
            table: "submissions",
            columns: SubmissionPeriodIndexColumns);

        migrationBuilder.CreateIndex(
            name: "ix_submissions_teacher_id",
            schema: "public",
            table: "submissions",
            column: "teacher_id");

        migrationBuilder.CreateIndex(
            name: "ix_teacher_departments_department_id",
            schema: "public",
            table: "teacher_departments",
            column: "department_id");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "answers",
            schema: "public");

        migrationBuilder.DropTable(
            name: "disciplines",
            schema: "public");

        migrationBuilder.DropTable(
            name: "questions",
            schema: "public");

        migrationBuilder.DropTable(
            name: "specialities",
            schema: "public");

        migrationBuilder.DropTable(
            name: "specializations",
            schema: "public");

        migrationBuilder.DropTable(
            name: "teacher_departments",
            schema: "public");

        migrationBuilder.DropTable(
            name: "users",
            schema: "public");

        migrationBuilder.DropTable(
            name: "submissions",
            schema: "public");

        migrationBuilder.DropTable(
            name: "forms",
            schema: "public");

        migrationBuilder.DropTable(
            name: "departments",
            schema: "public");

        migrationBuilder.DropTable(
            name: "teachers",
            schema: "public");
    }
}
