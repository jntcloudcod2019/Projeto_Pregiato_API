using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pregiato.API.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCityColumnInContracts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "City",
                table: "AgencyContracts",
                type: "character varying(100)",
                nullable: true, // Permite valores nulos
                oldClrType: typeof(string),
                oldType: "character varying(100)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "City",
                table: "AgencyContracts",
                type: "character varying(100)",
                nullable: false, // Volta para NOT NULL
                defaultValue: "N/A", // Valor padrão caso necessário
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldNullable: true);
        }
    }
}
