using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pregiato.API.Migrations
{
    /// <inheritdoc />
    public partial class Add_NovoCampo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Autorizationumber",
                table: "Payment",
                newName: "AutorizationNumber");

            //migrationBuilder.AlterColumn<int>(
            //    name: "Provider",
            //    table: "Payment",
            //    type: "integer",
            //    nullable: true,
            //    oldClrType: typeof(string),
            //    oldType: "text",
            //    oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UF",
                table: "Model",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UF",
                table: "Model");

            migrationBuilder.RenameColumn(
                name: "AutorizationNumber",
                table: "Payment",
                newName: "AutorizatioNumber");

            //migrationBuilder.AlterColumn<string>(
            //    name: "Provider",
            //    table: "Payment",
            //    type: "text",
            //    nullable: true,
            //    oldClrType: typeof(int),
            //    oldType: "integer",
            //    oldNullable: true);
        }
    }
}
