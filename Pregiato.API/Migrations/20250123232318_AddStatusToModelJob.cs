using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pregiato.API.Migrations
{
    /// <inheritdoc />
    public partial class AddStatusToModelJob : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
            name: "Status",
            table: "ModelJob",
            type: "character varying(50)",
            maxLength: 50,
            nullable: false,
            defaultValue: "Pending" // Define um valor padrão
  );

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
            name: "Status",
            table: "ModelJob"
            );

        }
    }
}
