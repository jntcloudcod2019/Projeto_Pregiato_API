using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pregiato.API.Migrations
{
    /// <inheritdoc />
    public partial class AddNewFieldsToContractBase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BairroEmpresa",
                table: "Contracts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CEPEmpresa",
                table: "Contracts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CNPJEmpresa",
                table: "Contracts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CidadeEmpresa",
                table: "Contracts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ComplementoEmpresa",
                table: "Contracts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EnderecoEmpresa",
                table: "Contracts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NomeEmpresa",
                table: "Contracts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NumeroEmpresa",
                table: "Contracts",
                type: "text",
                nullable: false,
                defaultValue: "");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BairroEmpresa",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "CEPEmpresa",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "CNPJEmpresa",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "CidadeEmpresa",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "ComplementoEmpresa",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "EnderecoEmpresa",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "NomeEmpresa",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "NumeroEmpresa",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "TelefonePrincipal",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "TelefoneSecundario",
                table: "Contracts");
        }
    }
}
