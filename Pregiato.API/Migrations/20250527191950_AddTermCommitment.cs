using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pregiato.API.Migrations
{
    /// <inheritdoc />
    public partial class AddTermCommitment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ContractCommitmentTerm",
                columns: table => new
                {
                    IDcontract = table.Column<Guid>(type: "uuid", nullable: false),
                    IDModel = table.Column<Guid>(type: "uuid", nullable: false),
                    NameModel = table.Column<string>(type: "text", nullable: false),
                    CpfModel = table.Column<string>(type: "text", nullable: false),
                    Mark = table.Column<string>(type: "text", nullable: false),
                    DatOfActivity = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AppointmentTime = table.Column<TimeSpan>(type: "interval", nullable: false),
                    Locality = table.Column<string>(type: "text", nullable: false),
                    GrossCash = table.Column<decimal>(type: "numeric", nullable: false),
                    NetCacheModel = table.Column<decimal>(type: "numeric", nullable: false),
                    PaymentMethod = table.Column<string>(type: "text", nullable: false),
                    ContractFilePath = table.Column<string>(type: "text", nullable: false),
                    Content = table.Column<byte[]>(type: "bytea", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractCommitmentTerm", x => x.IDcontract);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContractCommitmentTerm");
        }
    }
}
