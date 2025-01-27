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
            migrationBuilder.AddColumn<int>(
                name: "StatusPagamento",
                table: "Contracts",
                type: "StatusPagamentoEnum",
                nullable: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "StatusPagamento",
                table: "Contracts",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "StatusPagamentoEnum");
        }
    }
}
