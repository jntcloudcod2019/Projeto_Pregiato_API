using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pregiato.API.Migrations
{
    /// <inheritdoc />
    public partial class AddFieldsToTermoCompromisso : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            //migrationBuilder.AlterColumn<string>(
            //    name: "StatusPagamento",
            //    table: "Contracts",
            //    type: "text",
            //    nullable: false,
            //    oldClrType: typeof(int),
            //    oldType: "StatusPagamentoEnum");

            //migrationBuilder.AlterColumn<string>(
            //    name: "FormaPagamento",
            //    table: "Contracts",
            //    type: "text",
            //    nullable: false,
            //    oldClrType: typeof(int),
            //    oldType: "integer",
            //    oldMaxLength: 50);

            //migrationBuilder.AddColumn<string>(
            //    name: "DataAgendamento",
            //    table: "Contracts",
            //    type: "text",
            //    nullable: false,
            //    defaultValue: "");

            //migrationBuilder.AddColumn<string>(
            //    name: "HorarioAgendamento",
            //    table: "Contracts",
            //    type: "text",
            //    nullable: false,
            //    defaultValue: "");

            //migrationBuilder.AddColumn<decimal>(
            //    name: "ValorCache",
            //    table: "Contracts",
            //    type: "numeric",
            //    nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropColumn(
            //    name: "DataAgendamento",
            //    table: "Contracts");

            //migrationBuilder.DropColumn(
            //    name: "HorarioAgendamento",
            //    table: "Contracts");

            //migrationBuilder.DropColumn(
            //    name: "ValorCache",
            //    table: "Contracts");

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

            //migrationBuilder.AlterColumn<int>(
            //    name: "StatusPagamento",
            //    table: "Contracts",
            //    type: "StatusPagamentoEnum",
            //    nullable: false,
            //    oldClrType: typeof(string),
            //    oldType: "text");

            //migrationBuilder.AlterColumn<int>(
            //    name: "FormaPagamento",
            //    table: "Contracts",
            //    type: "integer",
            //    maxLength: 50,
            //    nullable: false,
            //    oldClrType: typeof(string),
            //    oldType: "text");
        }
    }
}
