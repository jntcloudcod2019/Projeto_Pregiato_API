using System;
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
            migrationBuilder.CreateTable(
                name: "Contracts",
                columns: table => new
                {
                    ContractId = table.Column<Guid>(type: "uuid", nullable: false),
                    ModelId = table.Column<Guid>(type: "uuid", nullable: false),
                    JobId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Neighborhood = table.Column<string>(type: "text", nullable: true),
                    City = table.Column<string>(type: "text", nullable: true),
                    ContractFilePath = table.Column<string>(type: "text", nullable: false),
                    Content = table.Column<byte[]>(type: "bytea", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contracts", x => x.ContractId);
                });

            migrationBuilder.CreateTable(
                name: "AgencyContracts",
                columns: table => new
                {
                    ContractId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgencyContracts", x => x.ContractId);
                    table.ForeignKey(
                        name: "FK_AgencyContracts_Contracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "Contracts",
                        principalColumn: "ContractId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CommitmentTerms",
                columns: table => new
                {
                    ContractId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommitmentTerms", x => x.ContractId);
                    table.ForeignKey(
                        name: "FK_CommitmentTerms_Contracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "Contracts",
                        principalColumn: "ContractId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ImageRightsContracts",
                columns: table => new
                {
                    ContractId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImageRightsContracts", x => x.ContractId);
                    table.ForeignKey(
                        name: "FK_ImageRightsContracts_Contracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "Contracts",
                        principalColumn: "ContractId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PhotographyContracts",
                columns: table => new
                {
                    ContractId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhotographyContracts", x => x.ContractId);
                    table.ForeignKey(
                        name: "FK_PhotographyContracts_Contracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "Contracts",
                        principalColumn: "ContractId",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AgencyContracts");

            migrationBuilder.DropTable(
                name: "CommitmentTerms");

            migrationBuilder.DropTable(
                name: "ImageRightsContracts");

            migrationBuilder.DropTable(
                name: "PhotographyContracts");

            migrationBuilder.DropTable(
                name: "Contracts");
        }
    }
}
