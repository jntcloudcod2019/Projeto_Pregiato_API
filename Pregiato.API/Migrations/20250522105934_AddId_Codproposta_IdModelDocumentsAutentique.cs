using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pregiato.API.Migrations
{
    /// <inheritdoc />
    public partial class AddId_Codproposta_IdModelDocumentsAutentique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "IdDocumentAutentique",
                table: "DocumentsAutentique",
                type: "text",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<int>(
                name: "CodProposta",
                table: "DocumentsAutentique",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "IdModel",
                table: "DocumentsAutentique",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CodProposta",
                table: "DocumentsAutentique");

            migrationBuilder.DropColumn(
                name: "IdModel",
                table: "DocumentsAutentique");

            migrationBuilder.AlterColumn<Guid>(
                name: "IdDocumentAutentique",
                table: "DocumentsAutentique",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}
