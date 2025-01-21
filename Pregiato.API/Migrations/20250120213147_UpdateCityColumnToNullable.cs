using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pregiato.API.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCityColumnToNullable : Migration
    {
        /// <inheritdoc />
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
            name: "City",
            table: "AgencyContracts",
            type: "character varying(100)",
            nullable: false, // Volta para NOT NULL
            oldClrType: typeof(string),
            oldType: "character varying(100)",
            oldNullable: true);

        }
    }
}
