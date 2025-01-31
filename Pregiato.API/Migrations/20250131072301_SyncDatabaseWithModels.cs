using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pregiato.API.Migrations
{
    /// <inheritdoc />
    public partial class SyncDatabaseWithModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "ContractsModels");

            migrationBuilder.DropColumn(
                name: "ModelId",
                table: "ContractsModels");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "ContractsModels");

            migrationBuilder.AddForeignKey(
                name: "FK_ContractsModels_Contracts_ContractId",
                table: "ContractsModels",
                column: "ContractId",
                principalTable: "Contracts",
                principalColumn: "ContractId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ContractsModels_Contracts_ContractId",
                table: "ContractsModels");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "ContractsModels",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "ModelId",
                table: "ContractsModels",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "ContractsModels",
                type: "timestamp with time zone",
                nullable: true);
        }
    }
}
