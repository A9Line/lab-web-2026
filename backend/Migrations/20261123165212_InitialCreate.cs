using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "laboratories",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("laboratories_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    login = table.Column<string>(type: "text", nullable: false),
                    full_name = table.Column<string>(type: "text", nullable: false),
                    role = table.Column<string>(type: "text", nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("users_pkey", x => x.id);
                    table.CheckConstraint("users_role_check", "role IN ('LabHead', 'Engineer', 'EcoResponsible')");
                });

            migrationBuilder.CreateTable(
                name: "waste_types",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    hazard_class = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("waste_types_pkey", x => x.id);
                    table.CheckConstraint("waste_types_hazard_class_check", "hazard_class BETWEEN 1 AND 5");
                });

            migrationBuilder.CreateTable(
                name: "waste_requests",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    number = table.Column<string>(type: "text", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    laboratory_id = table.Column<int>(type: "integer", nullable: false),
                    waste_type_id = table.Column<int>(type: "integer", nullable: false),
                    created_by_user_id = table.Column<int>(type: "integer", nullable: false),
                    assignee_user_id = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("waste_requests_pkey", x => x.id);
                    table.CheckConstraint("waste_requests_description_check", "char_length(description) BETWEEN 10 AND 500");
                    table.CheckConstraint("waste_requests_status_check", "status IN ('New', 'InProgress', 'Closed', 'Cancelled')");
                    table.CheckConstraint("waste_requests_title_check", "char_length(title) BETWEEN 5 AND 80");
                    table.ForeignKey(
                        name: "waste_requests_assignee_user_id_fkey",
                        column: x => x.assignee_user_id,
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "waste_requests_created_by_user_id_fkey",
                        column: x => x.created_by_user_id,
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "waste_requests_laboratory_id_fkey",
                        column: x => x.laboratory_id,
                        principalTable: "laboratories",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "waste_requests_waste_type_id_fkey",
                        column: x => x.waste_type_id,
                        principalTable: "waste_types",
                        principalColumn: "id");
                });

            migrationBuilder.InsertData(
                table: "laboratories",
                columns: new[] { "id", "name" },
                values: new object[,]
                {
                    { 1, "Лаборатория аналитической химии" },
                    { 2, "Лаборатория органического синтеза" },
                    { 3, "Лаборатория 214 (физхимия)" }
                });

            migrationBuilder.InsertData(
                table: "users",
                columns: new[] { "id", "full_name", "login", "password_hash", "role" },
                values: new object[,]
                {
                    { 1, "Петрова Анна Сергеевна", "petrova", null, "LabHead" },
                    { 2, "Иванов Дмитрий Олегович", "ivanov", null, "Engineer" },
                    { 3, "Сабиров Ильнур Ринатович", "sabirov", null, "Engineer" },
                    { 4, "Кузнецова Мария Петровна", "kuznetsova", null, "EcoResponsible" }
                });

            migrationBuilder.InsertData(
                table: "waste_types",
                columns: new[] { "id", "hazard_class", "name" },
                values: new object[,]
                {
                    { 1, 2, "Отработанные кислоты" },
                    { 2, 3, "Органические растворители" },
                    { 3, 1, "Ртутные лампы" },
                    { 4, 5, "Битая лабораторная посуда" }
                });

            migrationBuilder.CreateIndex(
                name: "laboratories_name_key",
                table: "laboratories",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "users_login_key",
                table: "users",
                column: "login",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_waste_requests_created_by_user_id",
                table: "waste_requests",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_waste_requests_waste_type_id",
                table: "waste_requests",
                column: "waste_type_id");

            migrationBuilder.CreateIndex(
                name: "waste_requests_assignee_user_id_idx",
                table: "waste_requests",
                column: "assignee_user_id");

            migrationBuilder.CreateIndex(
                name: "waste_requests_laboratory_id_idx",
                table: "waste_requests",
                column: "laboratory_id");

            migrationBuilder.CreateIndex(
                name: "waste_requests_number_key",
                table: "waste_requests",
                column: "number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "waste_requests_status_idx",
                table: "waste_requests",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "waste_types_name_key",
                table: "waste_types",
                column: "name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "waste_requests");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "laboratories");

            migrationBuilder.DropTable(
                name: "waste_types");
        }
    }
}
