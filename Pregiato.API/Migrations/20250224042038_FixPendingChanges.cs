using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pregiato.API.Migrations
{
    /// <inheritdoc />
    public partial class FixPendingChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Autorizationumber",
                table: "Payment",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Provider",
                table: "Payment",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Autorizationumber",
                table: "Payment");

            migrationBuilder.DropColumn(
                name: "Provider",
                table: "Payment");
        }
    }
}
