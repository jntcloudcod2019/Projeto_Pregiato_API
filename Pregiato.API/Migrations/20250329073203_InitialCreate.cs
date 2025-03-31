using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pregiato.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
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
                name: "Contracts",
                columns: table => new
                {
                    ContractId = table.Column<Guid>(type: "uuid", nullable: false),
                    PaymentId = table.Column<Guid>(type: "uuid", nullable: true),
                    CodProducers = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataContrato = table.Column<string>(type: "text", nullable: false),
                    VigenciaContrato = table.Column<string>(type: "text", nullable: false),
                    ValorContrato = table.Column<decimal>(type: "numeric", nullable: false),
                    FormaPagamento = table.Column<string>(type: "text", nullable: false),
                    StatusPagamento = table.Column<string>(type: "text", nullable: false),
                    ContractFilePath = table.Column<string>(type: "text", nullable: false),
                    Content = table.Column<byte[]>(type: "bytea", nullable: false),
                    CodProposta = table.Column<int>(type: "integer", nullable: false, defaultValue: 400),
                    StatusContratc = table.Column<string>(type: "text", nullable: false),
                    IdModel = table.Column<Guid>(type: "uuid", nullable: false),
                    ModelIdModel = table.Column<Guid>(type: "uuid", nullable: false),
                    ContractType = table.Column<string>(type: "character varying(21)", maxLength: 21, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contracts", x => x.ContractId);
                });

            migrationBuilder.CreateTable(
                name: "Producers",
                columns: table => new
                {
                    CodProducers = table.Column<string>(type: "text", nullable: false),
                    ContractId = table.Column<Guid>(type: "uuid", nullable: false),
                    NameProducer = table.Column<string>(type: "text", nullable: false),
                    AmountContract = table.Column<decimal>(type: "numeric", nullable: false),
                    InfoModel = table.Column<string>(type: "jsonb", nullable: true),
                    StatusContratc = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<string>(type: "text", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<string>(type: "text", nullable: false, defaultValueSql: "NOW()"),
                    ValidityContract = table.Column<string>(type: "text", nullable: true),
                    CodProposal = table.Column<int>(type: "integer", nullable: false),
                    TotalAgreements = table.Column<int>(type: "integer", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Producers", x => x.CodProducers);
                    table.ForeignKey(
                        name: "FK_Producers_Contracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "Contracts",
                        principalColumn: "ContractId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Models",
                columns: table => new
                {
                    ModelId = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CPF = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    RG = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Age = table.Column<int>(type: "integer", nullable: false),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    PostalCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
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
                    table.PrimaryKey("PK_Models", x => x.ModelId);
                    table.ForeignKey(
                        name: "FK_Models_Producers_CodProducers",
                        column: x => x.CodProducers,
                        principalTable: "Producers",
                        principalColumn: "CodProducers");
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    PaymentId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContractId = table.Column<Guid>(type: "uuid", nullable: true),
                    Valor = table.Column<decimal>(type: "numeric", nullable: false),
                    QuantidadeParcela = table.Column<int>(type: "integer", nullable: false),
                    FinalCartao = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    DataPagamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StatusPagamento = table.Column<string>(type: "text", nullable: false),
                    Comprovante = table.Column<byte[]>(type: "bytea", nullable: true),
                    DataAcordoPagamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    MetodoPagamento = table.Column<string>(type: "text", nullable: false),
                    Provider = table.Column<string>(type: "text", nullable: true),
                    AutorizationNumber = table.Column<string>(type: "text", nullable: false),
                    CodProducers = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.PaymentId);
                    table.ForeignKey(
                        name: "FK_Payments_Contracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "Contracts",
                        principalColumn: "ContractId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Payments_Producers_CodProducers",
                        column: x => x.CodProducers,
                        principalTable: "Producers",
                        principalColumn: "CodProducers",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Jobs",
                columns: table => new
                {
                    JobId = table.Column<Guid>(type: "uuid", nullable: false),
                    IdModel = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    JobDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Location = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Jobs", x => x.JobId);
                    table.ForeignKey(
                        name: "FK_Jobs_Models_IdModel",
                        column: x => x.IdModel,
                        principalTable: "Models",
                        principalColumn: "ModelId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ModelJob",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    ModelId = table.Column<Guid>(type: "uuid", nullable: false),
                    JobId = table.Column<Guid>(type: "uuid", nullable: true),
                    JobDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Location = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Time = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    AdditionalDescription = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModelJob", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ModelJob_Jobs_JobId",
                        column: x => x.JobId,
                        principalTable: "Jobs",
                        principalColumn: "JobId");
                    table.ForeignKey(
                        name: "FK_ModelJob_Models_ModelId",
                        column: x => x.ModelId,
                        principalTable: "Models",
                        principalColumn: "ModelId");
                });

            migrationBuilder.CreateTable(
                name: "ModelsBilling",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    JobId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    BillingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IdModel = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModelsBilling", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ModelsBilling_Jobs_JobId",
                        column: x => x.JobId,
                        principalTable: "Jobs",
                        principalColumn: "JobId");
                    table.ForeignKey(
                        name: "FK_ModelsBilling_Models_IdModel",
                        column: x => x.IdModel,
                        principalTable: "Models",
                        principalColumn: "ModelId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_CodProducers",
                table: "Contracts",
                column: "CodProducers");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_ModelIdModel",
                table: "Contracts",
                column: "ModelIdModel");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_IdModel",
                table: "Jobs",
                column: "IdModel");

            migrationBuilder.CreateIndex(
                name: "IX_ModelJob_JobId",
                table: "ModelJob",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_ModelJob_ModelId",
                table: "ModelJob",
                column: "ModelId");

            migrationBuilder.CreateIndex(
                name: "IX_Models_CodProducers",
                table: "Models",
                column: "CodProducers");

            migrationBuilder.CreateIndex(
                name: "IX_Models_CPF",
                table: "Models",
                column: "CPF");

            migrationBuilder.CreateIndex(
                name: "IX_ModelsBilling_IdModel",
                table: "ModelsBilling",
                column: "IdModel");

            migrationBuilder.CreateIndex(
                name: "IX_ModelsBilling_JobId",
                table: "ModelsBilling",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_CodProducers",
                table: "Payments",
                column: "CodProducers");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_ContractId",
                table: "Payments",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_Producers_ContractId",
                table: "Producers",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_CodProducers",
                table: "Users",
                column: "CodProducers");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Name",
                table: "Users",
                column: "Name");

            migrationBuilder.AddForeignKey(
                name: "FK_Contracts_Models_ContractId",
                table: "Contracts",
                column: "ContractId",
                principalTable: "Models",
                principalColumn: "ModelId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Contracts_Models_ModelIdModel",
                table: "Contracts",
                column: "ModelIdModel",
                principalTable: "Models",
                principalColumn: "ModelId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Contracts_Producers_CodProducers",
                table: "Contracts",
                column: "CodProducers",
                principalTable: "Producers",
                principalColumn: "CodProducers",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_Models_ContractId",
                table: "Contracts");

            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_Models_ModelIdModel",
                table: "Contracts");

            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_Producers_CodProducers",
                table: "Contracts");

            migrationBuilder.DropTable(
                name: "ModelJob");

            migrationBuilder.DropTable(
                name: "ModelsBilling");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Jobs");

            migrationBuilder.DropTable(
                name: "Models");

            migrationBuilder.DropTable(
                name: "Producers");

            migrationBuilder.DropTable(
                name: "Contracts");
        }
    }
}
