using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pregiato.API.Migrations
{
    /// <inheritdoc />
    public partial class RemovePaymentContractRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_Payments_PaymentId",
                table: "Contracts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Payments",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_ContractId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Contracts_PaymentId",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "IdProducer",
                table: "Payments");

            migrationBuilder.AlterColumn<string>(
                name: "ValidityContract",
                table: "Producers",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Payments",
                table: "Payments",
                column: "ContractId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Payments",
                table: "Payments");

            migrationBuilder.AlterColumn<string>(
                name: "ValidityContract",
                table: "Producers",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "IdProducer",
                table: "Payments",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_Payments",
                table: "Payments",
                column: "IdProducer");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_ContractId",
                table: "Payments",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_PaymentId",
                table: "Contracts",
                column: "PaymentId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Contracts_Payments_PaymentId",
                table: "Contracts",
                column: "PaymentId",
                principalTable: "Payments",
                principalColumn: "IdProducer",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
