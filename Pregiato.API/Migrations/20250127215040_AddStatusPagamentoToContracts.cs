using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pregiato.API.Migrations
{
    /// <inheritdoc />
    public partial class AddStatusPagamentoToContracts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Adiciona a coluna StatusPagamento como StatusPagamentoEnum
            migrationBuilder.AddColumn<string>(
                name: "StatusPagamento",
                table: "Contracts",
                type: "StatusPagamentoEnum",
                nullable: false);
               // defaultValue: "Pendente"); // Ajuste o valor padrão se necessário
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove a coluna StatusPagamento em caso de reversão
            migrationBuilder.DropColumn(
                name: "StatusPagamento",
                table: "Contracts");
        }
    }
}
