using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pregiato.API.Migrations
{
    /// <inheritdoc />
    public partial class AddCreatedAtToContract : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Age",
                table: "Model",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Age",
                table: "Model");
        }
    }
}
