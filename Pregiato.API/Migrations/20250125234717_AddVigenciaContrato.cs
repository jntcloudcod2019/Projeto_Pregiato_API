using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pregiato.API.Migrations
{
    /// <inheritdoc />
    public partial class AddVigenciaContrato : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Complement",
                table: "Models",
                newName: "Complement");

            migrationBuilder.AddColumn<string>(
                name: "VigenciaContrato",
                table: "Contracts",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VigenciaContrato",
                table: "Contracts");

            migrationBuilder.RenameColumn(
                name: "Complement",
                table: "Models",
                newName: "Complement");
        }
    }
}
