using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pregiato.API.Migrations
{
    /// <inheritdoc />
    public partial class FixedProducersConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ContractDTO",
                columns: table => new
                {
                    ContractId = table.Column<Guid>(type: "uuid", nullable: true),
                    ModelId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProposalCode = table.Column<int>(type: "integer", nullable: true),
                    ContractFilePath = table.Column<string>(type: "text", nullable: true),
                    Content = table.Column<byte[]>(type: "bytea", nullable: true)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "Jobs",
                columns: table => new
                {
                    JobId = table.Column<Guid>(type: "uuid", nullable: false),
                    IdModel = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    JobDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Location = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Jobs", x => x.JobId);
                });

            migrationBuilder.CreateTable(
                name: "Models",
                columns: table => new
                {
                    IdModel = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    CPF = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: false),
                    RG = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Age = table.Column<int>(type: "integer", nullable: false),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    PostalCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    Address = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    NumberAddress = table.Column<string>(type: "text", nullable: false),
                    Complement = table.Column<string>(type: "text", nullable: false),
                    BankAccount = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Status = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Neighborhood = table.Column<string>(type: "text", nullable: false),
                    City = table.Column<string>(type: "text", nullable: false),
                    UF = table.Column<string>(type: "text", nullable: false),
                    TelefonePrincipal = table.Column<string>(type: "text", nullable: false),
                    TelefoneSecundario = table.Column<string>(type: "text", nullable: false),
                    DNA = table.Column<JsonDocument>(type: "jsonb", nullable: true, defaultValueSql: "'{}'::jsonb"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    CodProducers = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Models", x => x.IdModel);
                });

            migrationBuilder.CreateTable(
                name: "Producers",
                columns: table => new
                {
                    IdProducer = table.Column<Guid>(type: "uuid", nullable: false),
                    IdContract = table.Column<Guid>(type: "uuid", nullable: false),
                    CodProducers = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    NameProducer = table.Column<string>(type: "text", nullable: false),
                    AmountContract = table.Column<decimal>(type: "numeric", nullable: false),
                    InfoModel = table.Column<string>(type: "jsonb", nullable: true),
                    StatusContratc = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<string>(type: "text", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<string>(type: "text", nullable: false, defaultValueSql: "NOW()"),
                    ValidityContract = table.Column<string>(type: "text", nullable: false),
                    CodProposal = table.Column<int>(type: "integer", nullable: false),
                    TotalAgreements = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Producers", x => x.IdProducer);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CodProducers = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    NickName = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    UserType = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "ModelJobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IdModel = table.Column<Guid>(type: "uuid", nullable: false),
                    IdJob = table.Column<Guid>(type: "uuid", nullable: false),
                    JobDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Location = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Time = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    AdditionalDescription = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModelJobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ModelJobs_Jobs_IdJob",
                        column: x => x.IdJob,
                        principalTable: "Jobs",
                        principalColumn: "JobId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ModelJobs_Models_IdModel",
                        column: x => x.IdModel,
                        principalTable: "Models",
                        principalColumn: "IdModel",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ModelsBilling",
                columns: table => new
                {
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    BillingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IdModel = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModelsBilling", x => x.Amount);
                    table.ForeignKey(
                        name: "FK_ModelsBilling_Models_IdModel",
                        column: x => x.IdModel,
                        principalTable: "Models",
                        principalColumn: "IdModel",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Contracts",
                columns: table => new
                {
                    ContractId = table.Column<Guid>(type: "uuid", nullable: false),
                    PaymentId = table.Column<Guid>(type: "uuid", nullable: false),
                    CodProducers = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataContrato = table.Column<string>(type: "text", nullable: false),
                    VigenciaContrato = table.Column<string>(type: "text", nullable: false),
                    ValorContrato = table.Column<decimal>(type: "numeric", nullable: false),
                    FormaPagamento = table.Column<string>(type: "text", nullable: false),
                    StatusPagamento = table.Column<string>(type: "text", nullable: false),
                    ContractFilePath = table.Column<string>(type: "text", nullable: true),
                    Content = table.Column<byte[]>(type: "bytea", nullable: true),
                    CodProposta = table.Column<int>(type: "integer", nullable: false, defaultValue: 110),
                    StatusContratc = table.Column<string>(type: "text", nullable: false),
                    ModelId = table.Column<Guid>(type: "uuid", nullable: false),
                    Discriminator = table.Column<string>(type: "character varying(55)", maxLength: 55, nullable: false),
                    ModelIdModel = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contracts", x => x.ContractId);
                    table.ForeignKey(
                        name: "FK_Contracts_Models_ModelId",
                        column: x => x.ModelId,
                        principalTable: "Models",
                        principalColumn: "IdModel",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Contracts_Models_ModelIdModel",
                        column: x => x.ModelIdModel,
                        principalTable: "Models",
                        principalColumn: "IdModel");
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    IdProducer = table.Column<Guid>(type: "uuid", nullable: false),
                    ContractId = table.Column<Guid>(type: "uuid", nullable: false),
                    Valor = table.Column<decimal>(type: "numeric", nullable: false),
                    QuantidadeParcela = table.Column<int>(type: "integer", nullable: true),
                    FinalCartao = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: true),
                    DataPagamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StatusPagamento = table.Column<string>(type: "text", nullable: false),
                    Comprovante = table.Column<byte[]>(type: "bytea", nullable: true),
                    DataAcordoPagamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MetodoPagamento = table.Column<string>(type: "text", nullable: false),
                    Provider = table.Column<string>(type: "text", nullable: false),
                    AutorizationNumber = table.Column<string>(type: "text", nullable: true),
                    CodProducers = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.IdProducer);
                    table.ForeignKey(
                        name: "FK_Payments_Contracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "Contracts",
                        principalColumn: "ContractId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_ModelId",
                table: "Contracts",
                column: "ModelId");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_ModelIdModel",
                table: "Contracts",
                column: "ModelIdModel");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_PaymentId",
                table: "Contracts",
                column: "PaymentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ModelJobs_IdJob",
                table: "ModelJobs",
                column: "IdJob");

            migrationBuilder.CreateIndex(
                name: "IX_ModelJobs_IdModel",
                table: "ModelJobs",
                column: "IdModel");

            migrationBuilder.CreateIndex(
                name: "IX_Models_CPF",
                table: "Models",
                column: "CPF");

            migrationBuilder.CreateIndex(
                name: "IX_ModelsBilling_IdModel",
                table: "ModelsBilling",
                column: "IdModel",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_ContractId",
                table: "Payments",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_CodProducers",
                table: "Users",
                column: "CodProducers");

            migrationBuilder.AddForeignKey(
                name: "FK_Contracts_Payments_PaymentId",
                table: "Contracts",
                column: "PaymentId",
                principalTable: "Payments",
                principalColumn: "IdProducer",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_Models_ModelId",
                table: "Contracts");

            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_Models_ModelIdModel",
                table: "Contracts");

            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_Payments_PaymentId",
                table: "Contracts");

            migrationBuilder.DropTable(
                name: "ContractDTO");

            migrationBuilder.DropTable(
                name: "ModelJobs");

            migrationBuilder.DropTable(
                name: "ModelsBilling");

            migrationBuilder.DropTable(
                name: "Producers");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Jobs");

            migrationBuilder.DropTable(
                name: "Models");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "Contracts");
        }
    }
}
