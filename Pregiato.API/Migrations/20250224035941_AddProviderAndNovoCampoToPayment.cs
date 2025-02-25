using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pregiato.API.Migrations
{
    public partial class AddProviderAndNovoCampoToPayment : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Adiciona a coluna Provider como texto (mapeando o enum para string)
            migrationBuilder.AddColumn<string>(
                name: "Provider",
                table: "Payments",
                type: "text",
                nullable: false,
                defaultValue: "Stone"); // Valor padrão

            // Adiciona a coluna NovoCampo como texto (opcional)
            migrationBuilder.AddColumn<string>(
                name: "AutorizationNumber",
                table: "Payments",
                type: "text",
                nullable: true); // Pode ser nulo
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove a coluna Provider
            migrationBuilder.DropColumn(
                name: "Provider",
                table: "Payments");

            // Remove a coluna NovoCampo
            migrationBuilder.DropColumn(
                name: "AutorizationNumber",
                table: "Payments");
        }
    }
}