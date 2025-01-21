using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pregiato.API.Migrations
{
    /// <inheritdoc />
    public partial class AddNeighborhoodAndCityToContracts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "PhotographyProductionContracts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Neighborhood",
                table: "PhotographyProductionContracts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Models",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Neighborhood",
                table: "Models",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "ImageRightsTerms",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Neighborhood",
                table: "ImageRightsTerms",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "CommitmentTerms",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Neighborhood",
                table: "CommitmentTerms",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "AgencyContracts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Neighborhood",
                table: "AgencyContracts",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "City",
                table: "PhotographyProductionContracts");

            migrationBuilder.DropColumn(
                name: "Neighborhood",
                table: "PhotographyProductionContracts");

            migrationBuilder.DropColumn(
                name: "City",
                table: "Models");

            migrationBuilder.DropColumn(
                name: "Neighborhood",
                table: "Models");

            migrationBuilder.DropColumn(
                name: "City",
                table: "ImageRightsTerms");

            migrationBuilder.DropColumn(
                name: "Neighborhood",
                table: "ImageRightsTerms");

            migrationBuilder.DropColumn(
                name: "City",
                table: "CommitmentTerms");

            migrationBuilder.DropColumn(
                name: "Neighborhood",
                table: "CommitmentTerms");

            migrationBuilder.DropColumn(
                name: "City",
                table: "AgencyContracts");

            migrationBuilder.DropColumn(
                name: "Neighborhood",
                table: "AgencyContracts");
        }
    }
}
