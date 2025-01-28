using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pregiato.API.Migrations
{
    /// <inheritdoc />
    public partial class RevertUpdateEnumsToStrings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //// Reverter alterações nos contratos
            //migrationBuilder.AlterColumn<int>(
            //    name: "FormaPagamento",
            //    table: "Contracts",
            //    type: "MetodoPagamentoEnum",
            //    nullable: false,
            //    oldClrType: typeof(string),
            //    oldType: "text");

            //// Reverter alterações nos pagamentos
            //migrationBuilder.AlterColumn<int>(
            //    name: "StatusPagamento",
            //    table: "Payment",
            //    type: "StatusPagamentoEnum",
            //    nullable: false,
            //    oldClrType: typeof(string),
            //    oldType: "text");

            //migrationBuilder.AlterColumn<int>(
            //    name: "MetodoPagamento",
            //    table: "Payment",
            //    type: "MetodoPagamentoEnum",
            //    nullable: false,
            //    oldClrType: typeof(string),
            //    oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "FinalCartao",
                table: "Payment",
                type: "character varying(4)",
                maxLength: 4,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<byte[]>(
                name: "Comprovante",
                table: "Payment",
                type: "bytea",
                nullable: true,
                oldClrType: typeof(byte[]),
                oldType: "bytea");

            // Alteração nas tabelas de modelos
            migrationBuilder.AlterColumn<string>(
                name: "Neighborhood",
                table: "Models",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "City",
                table: "Models",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            // Alteração de colunas de data
            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "ContractsModels",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldDefaultValueSql: "NOW()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "ContractsModels",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "NOW()");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Restaurar alterações nos contratos
            //migrationBuilder.AlterColumn<string>(
            //    name: "FormaPagamento",
            //    table: "Contracts",
            //    type: "text",
            //    nullable: false,
            //    oldClrType: typeof(int),
            //    oldType: "MetodoPagamentoEnum");

            //// Restaurar alterações nos pagamentos
            //migrationBuilder.AlterColumn<string>(
            //    name: "StatusPagamento",
            //    table: "Payment",
            //    type: "text",
            //    nullable: false,
            //    oldClrType: typeof(int),
            //    oldType: "StatusPagamentoEnum");

            //migrationBuilder.AlterColumn<string>(
            //    name: "MetodoPagamento",
            //    table: "Payment",
            //    type: "text",
            //    nullable: false,
            //    oldClrType: typeof(int),
            //    oldType: "MetodoPagamentoEnum");

            migrationBuilder.AlterColumn<string>(
                name: "FinalCartao",
                table: "Payment",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(4)",
                oldMaxLength: 4,
                oldNullable: true);

            migrationBuilder.AlterColumn<byte[]>(
                name: "Comprovante",
                table: "Payment",
                type: "bytea",
                nullable: false,
                defaultValue: new byte[0],
                oldClrType: typeof(byte[]),
                oldType: "bytea",
                oldNullable: true);

            // Restaurar alterações nas tabelas de modelos
            migrationBuilder.AlterColumn<string>(
                name: "Neighborhood",
                table: "Models",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "City",
                table: "Models",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            // Restaurar colunas de data
            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "ContractsModels",
                type: "timestamp with time zone",
                nullable: true,
                defaultValueSql: "NOW()",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "ContractsModels",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW()",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");
        }
    }
}
