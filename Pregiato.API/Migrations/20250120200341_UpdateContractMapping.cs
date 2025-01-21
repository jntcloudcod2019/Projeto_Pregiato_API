using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pregiato.API.Migrations
{
    /// <inheritdoc />
    public partial class UpdateContractMapping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "Content",
                table: "PhotographyProductionContracts",
                type: "bytea",
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<byte[]>(
                name: "Content",
                table: "ImageRightsTerms",
                type: "bytea",
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<byte[]>(
                name: "Content",
                table: "CommitmentTerms",
                type: "bytea",
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<byte[]>(
                name: "Content",
                table: "AgencyContracts",
                type: "bytea",
                nullable: false,
                defaultValue: new byte[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Content",
                table: "PhotographyProductionContracts");

            migrationBuilder.DropColumn(
                name: "Content",
                table: "ImageRightsTerms");

            migrationBuilder.DropColumn(
                name: "Content",
                table: "CommitmentTerms");

            migrationBuilder.DropColumn(
                name: "Content",
                table: "AgencyContracts");
        }
    }
}
