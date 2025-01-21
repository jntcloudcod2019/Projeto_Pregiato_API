using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pregiato.API.Migrations
{
    /// <inheritdoc />
    public partial class AdjustModelAgencyContext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AgencyContracts_ContractBase_ContractId",
                table: "AgencyContracts");

            migrationBuilder.DropForeignKey(
                name: "FK_CommitmentTerms_ContractBase_ContractId",
                table: "CommitmentTerms");

            migrationBuilder.DropForeignKey(
                name: "FK_ImageRightsContracts_ContractBase_ContractId",
                table: "ImageRightsContracts");

            migrationBuilder.DropForeignKey(
                name: "FK_PhotographyContracts_ContractBase_ContractId",
                table: "PhotographyContracts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ContractBase",
                table: "ContractBase");

            migrationBuilder.RenameTable(
                name: "ContractBase",
                newName: "Contracts");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Contracts",
                table: "Contracts",
                column: "ContractId");

            migrationBuilder.AddForeignKey(
                name: "FK_AgencyContracts_Contracts_ContractId",
                table: "AgencyContracts",
                column: "ContractId",
                principalTable: "Contracts",
                principalColumn: "ContractId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CommitmentTerms_Contracts_ContractId",
                table: "CommitmentTerms",
                column: "ContractId",
                principalTable: "Contracts",
                principalColumn: "ContractId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ImageRightsContracts_Contracts_ContractId",
                table: "ImageRightsContracts",
                column: "ContractId",
                principalTable: "Contracts",
                principalColumn: "ContractId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PhotographyContracts_Contracts_ContractId",
                table: "PhotographyContracts",
                column: "ContractId",
                principalTable: "Contracts",
                principalColumn: "ContractId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AgencyContracts_Contracts_ContractId",
                table: "AgencyContracts");

            migrationBuilder.DropForeignKey(
                name: "FK_CommitmentTerms_Contracts_ContractId",
                table: "CommitmentTerms");

            migrationBuilder.DropForeignKey(
                name: "FK_ImageRightsContracts_Contracts_ContractId",
                table: "ImageRightsContracts");

            migrationBuilder.DropForeignKey(
                name: "FK_PhotographyContracts_Contracts_ContractId",
                table: "PhotographyContracts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Contracts",
                table: "Contracts");

            migrationBuilder.RenameTable(
                name: "Contracts",
                newName: "ContractBase");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ContractBase",
                table: "ContractBase",
                column: "ContractId");

            migrationBuilder.AddForeignKey(
                name: "FK_AgencyContracts_ContractBase_ContractId",
                table: "AgencyContracts",
                column: "ContractId",
                principalTable: "ContractBase",
                principalColumn: "ContractId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CommitmentTerms_ContractBase_ContractId",
                table: "CommitmentTerms",
                column: "ContractId",
                principalTable: "ContractBase",
                principalColumn: "ContractId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ImageRightsContracts_ContractBase_ContractId",
                table: "ImageRightsContracts",
                column: "ContractId",
                principalTable: "ContractBase",
                principalColumn: "ContractId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PhotographyContracts_ContractBase_ContractId",
                table: "PhotographyContracts",
                column: "ContractId",
                principalTable: "ContractBase",
                principalColumn: "ContractId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.DropTable("AgencyContracts");
        }
    }
}
