using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pregiato.API.Migrations
{
    /// <inheritdoc />
    public partial class AddDateOfBirthCorrectly : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LoginUserRequest");

            migrationBuilder.AlterColumn<JsonDocument>(
                name: "DNA",
                table: "Model",
                type: "jsonb",
                nullable: true,
                defaultValueSql: "'{}'::jsonb",
                oldClrType: typeof(JsonDocument),
                oldType: "jsonb",
                oldDefaultValueSql: "'{}'::jsonb");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfBirth",
                table: "Model",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "Model");

            migrationBuilder.AlterColumn<JsonDocument>(
                name: "DNA",
                table: "Model",
                type: "jsonb",
                nullable: false,
                defaultValueSql: "'{}'::jsonb",
                oldClrType: typeof(JsonDocument),
                oldType: "jsonb",
                oldNullable: true,
                oldDefaultValueSql: "'{}'::jsonb");

            migrationBuilder.CreateTable(
                name: "LoginUserRequest",
                columns: table => new
                {
                    Password = table.Column<string>(type: "text", nullable: false),
                    UserType = table.Column<string>(type: "text", nullable: true),
                    Username = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                });
        }
    }
}
