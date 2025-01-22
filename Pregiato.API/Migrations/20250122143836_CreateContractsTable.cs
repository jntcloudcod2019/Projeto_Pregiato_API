using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pregiato.API.Migrations
{
    /// <inheritdoc />
    public partial class CreateContractsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PhotographyContracts_Contracts_ContractId",
                table: "PhotographyContracts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PhotographyContracts",
                table: "PhotographyContracts");

            migrationBuilder.RenameTable(
                name: "PhotographyContracts",
                newName: "PhotographyProductionContracts");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PhotographyProductionContracts",
                table: "PhotographyProductionContracts",
                column: "ContractId");

            migrationBuilder.AddForeignKey(
                name: "FK_PhotographyProductionContracts_Contracts_ContractId",
                table: "PhotographyProductionContracts",
                column: "ContractId",
                principalTable: "Contracts",
                principalColumn: "ContractId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PhotographyProductionContracts_Contracts_ContractId",
                table: "PhotographyProductionContracts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PhotographyProductionContracts",
                table: "PhotographyProductionContracts");

            migrationBuilder.RenameTable(
                name: "PhotographyProductionContracts",
                newName: "PhotographyContracts");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PhotographyContracts",
                table: "PhotographyContracts",
                column: "ContractId");

            migrationBuilder.AddForeignKey(
                name: "FK_PhotographyContracts_Contracts_ContractId",
                table: "PhotographyContracts",
                column: "ContractId",
                principalTable: "Contracts",
                principalColumn: "ContractId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
