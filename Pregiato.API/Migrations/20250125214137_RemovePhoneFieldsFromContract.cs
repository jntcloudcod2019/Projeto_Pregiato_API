using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pregiato.API.Migrations
{
    /// <inheritdoc />
    public partial class RemovePhoneFieldsFromContract : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TelefonePrincipal",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "TelefoneSecundario",
                table: "Contracts");

            migrationBuilder.AddColumn<string>(
                name: "TelefonePrincipal",
                table: "Models",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TelefoneSecundario",
                table: "Models",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TelefonePrincipal",
                table: "Models");

            migrationBuilder.DropColumn(
                name: "TelefoneSecundario",
                table: "Models");

            migrationBuilder.AddColumn<string>(
                name: "TelefonePrincipal",
                table: "Contracts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TelefoneSecundario",
                table: "Contracts",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
