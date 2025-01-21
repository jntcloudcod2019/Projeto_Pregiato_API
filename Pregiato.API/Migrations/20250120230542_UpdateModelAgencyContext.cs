using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pregiato.API.Migrations
{
    /// <inheritdoc />
    public partial class UpdateModelAgencyContext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_PhotographyProductionContracts",
                table: "PhotographyProductionContracts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ImageRightsTerms",
                table: "ImageRightsTerms");

            migrationBuilder.DropColumn(
                name: "City",
                table: "CommitmentTerms");

            migrationBuilder.DropColumn(
                name: "Content",
                table: "CommitmentTerms");

            migrationBuilder.DropColumn(
                name: "ContractFilePath",
                table: "CommitmentTerms");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "CommitmentTerms");

            migrationBuilder.DropColumn(
                name: "JobId",
                table: "CommitmentTerms");

            migrationBuilder.DropColumn(
                name: "ModelId",
                table: "CommitmentTerms");

            migrationBuilder.DropColumn(
                name: "Neighborhood",
                table: "CommitmentTerms");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "CommitmentTerms");

            migrationBuilder.DropColumn(
                name: "City",
                table: "AgencyContracts");

            migrationBuilder.DropColumn(
                name: "Content",
                table: "AgencyContracts");

            migrationBuilder.DropColumn(
                name: "ContractFilePath",
                table: "AgencyContracts");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "AgencyContracts");

            migrationBuilder.DropColumn(
                name: "JobId",
                table: "AgencyContracts");

            migrationBuilder.DropColumn(
                name: "ModelId",
                table: "AgencyContracts");

            migrationBuilder.DropColumn(
                name: "Neighborhood",
                table: "AgencyContracts");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "AgencyContracts");

            migrationBuilder.DropColumn(
                name: "City",
                table: "PhotographyProductionContracts");

            migrationBuilder.DropColumn(
                name: "Content",
                table: "PhotographyProductionContracts");

            migrationBuilder.DropColumn(
                name: "ContractFilePath",
                table: "PhotographyProductionContracts");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "PhotographyProductionContracts");

            migrationBuilder.DropColumn(
                name: "JobId",
                table: "PhotographyProductionContracts");

            migrationBuilder.DropColumn(
                name: "ModelId",
                table: "PhotographyProductionContracts");

            migrationBuilder.DropColumn(
                name: "Neighborhood",
                table: "PhotographyProductionContracts");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "PhotographyProductionContracts");

            migrationBuilder.DropColumn(
                name: "City",
                table: "ImageRightsTerms");

            migrationBuilder.DropColumn(
                name: "Content",
                table: "ImageRightsTerms");

            migrationBuilder.DropColumn(
                name: "ContractFilePath",
                table: "ImageRightsTerms");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "ImageRightsTerms");

            migrationBuilder.DropColumn(
                name: "JobId",
                table: "ImageRightsTerms");

            migrationBuilder.DropColumn(
                name: "ModelId",
                table: "ImageRightsTerms");

            migrationBuilder.DropColumn(
                name: "Neighborhood",
                table: "ImageRightsTerms");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "ImageRightsTerms");

            migrationBuilder.RenameTable(
                name: "PhotographyProductionContracts",
                newName: "PhotographyContracts");

            migrationBuilder.RenameTable(
                name: "ImageRightsTerms",
                newName: "ImageRightsContracts");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PhotographyContracts",
                table: "PhotographyContracts",
                column: "ContractId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ImageRightsContracts",
                table: "ImageRightsContracts",
                column: "ContractId");

            migrationBuilder.CreateTable(
                name: "ContractBase",
                columns: table => new
                {
                    ContractId = table.Column<Guid>(type: "uuid", nullable: false),
                    ModelId = table.Column<Guid>(type: "uuid", nullable: false),
                    JobId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Neighborhood = table.Column<string>(type: "text", nullable: true),
                    City = table.Column<string>(type: "text", nullable: true),
                    ContractFilePath = table.Column<string>(type: "text", nullable: true),
                    Content = table.Column<byte[]>(type: "bytea", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractBase", x => x.ContractId);
                });

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.DropTable(
                name: "ContractBase");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PhotographyContracts",
                table: "PhotographyContracts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ImageRightsContracts",
                table: "ImageRightsContracts");

            migrationBuilder.RenameTable(
                name: "PhotographyContracts",
                newName: "PhotographyProductionContracts");

            migrationBuilder.RenameTable(
                name: "ImageRightsContracts",
                newName: "ImageRightsTerms");

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "CommitmentTerms",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<byte[]>(
                name: "Content",
                table: "CommitmentTerms",
                type: "bytea",
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<string>(
                name: "ContractFilePath",
                table: "CommitmentTerms",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "CommitmentTerms",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "JobId",
                table: "CommitmentTerms",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ModelId",
                table: "CommitmentTerms",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "Neighborhood",
                table: "CommitmentTerms",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "CommitmentTerms",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "AgencyContracts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<byte[]>(
                name: "Content",
                table: "AgencyContracts",
                type: "bytea",
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<string>(
                name: "ContractFilePath",
                table: "AgencyContracts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "AgencyContracts",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "JobId",
                table: "AgencyContracts",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ModelId",
                table: "AgencyContracts",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "Neighborhood",
                table: "AgencyContracts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "AgencyContracts",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "PhotographyProductionContracts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<byte[]>(
                name: "Content",
                table: "PhotographyProductionContracts",
                type: "bytea",
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<string>(
                name: "ContractFilePath",
                table: "PhotographyProductionContracts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "PhotographyProductionContracts",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "JobId",
                table: "PhotographyProductionContracts",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ModelId",
                table: "PhotographyProductionContracts",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "Neighborhood",
                table: "PhotographyProductionContracts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "PhotographyProductionContracts",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "ImageRightsTerms",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<byte[]>(
                name: "Content",
                table: "ImageRightsTerms",
                type: "bytea",
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<string>(
                name: "ContractFilePath",
                table: "ImageRightsTerms",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "ImageRightsTerms",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "JobId",
                table: "ImageRightsTerms",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ModelId",
                table: "ImageRightsTerms",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "Neighborhood",
                table: "ImageRightsTerms",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "ImageRightsTerms",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddPrimaryKey(
                name: "PK_PhotographyProductionContracts",
                table: "PhotographyProductionContracts",
                column: "ContractId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ImageRightsTerms",
                table: "ImageRightsTerms",
                column: "ContractId");
        }
    }
}
