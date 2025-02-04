CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;
CREATE TABLE "Clients" (
    "IdClient" uuid NOT NULL DEFAULT (gen_random_uuid()),
    "Name" character varying(255) NOT NULL,
    "Email" character varying(255) NOT NULL,
    "ClientDocument" character varying(50) NOT NULL,
    "Contact" character varying(20) NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL DEFAULT (NOW()),
    "UpdatedAt" timestamp with time zone NOT NULL DEFAULT (NOW()),
    "Status" boolean NOT NULL,
    CONSTRAINT "PK_Clients" PRIMARY KEY ("IdClient")
);

CREATE TABLE "ClientsBilling" (
    "BillingId" uuid NOT NULL,
    "ClientId" uuid NOT NULL,
    "Amount" numeric NOT NULL,
    "BillingDate" timestamp with time zone NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_ClientsBilling" PRIMARY KEY ("BillingId")
);

CREATE TABLE "Contracts" (
    "ContractId" uuid NOT NULL,
    "ModelId" uuid NOT NULL,
    "JobId" uuid NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    "Neighborhood" text,
    "City" text,
    "ContractFilePath" text,
    "Content" bytea NOT NULL,
    CONSTRAINT "PK_Contracts" PRIMARY KEY ("ContractId")
);

CREATE TABLE "ContractsModels" (
    "ContractId" uuid NOT NULL DEFAULT (gen_random_uuid()),
    "ModelId" uuid NOT NULL,
    "ContractFile" text NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL DEFAULT (NOW()),
    "UpdatedAt" timestamp with time zone DEFAULT (NOW()),
    CONSTRAINT "PK_ContractsModels" PRIMARY KEY ("ContractId"),
    CONSTRAINT "CK_ContractsModels_FileFormat" CHECK ("ContractFile" ~ '\.(doc|docx|pdf)$')
);

CREATE TABLE "Jobs" (
    "IdJob" uuid NOT NULL DEFAULT (gen_random_uuid()),
    "Description" character varying(500) NOT NULL,
    "Status" character varying(20) NOT NULL DEFAULT 'Pending',
    "JobDate" timestamp with time zone NOT NULL,
    "Location" character varying(255) NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL DEFAULT (NOW()),
    "UpdatedAt" timestamp with time zone NOT NULL DEFAULT (NOW()),
    CONSTRAINT "PK_Jobs" PRIMARY KEY ("IdJob")
);

CREATE TABLE "LoginUserRequest" (
    "Username" text,
    "Password" text NOT NULL,
    "UserType" text
);

CREATE TABLE "Models" (
    "IdModel" uuid NOT NULL DEFAULT (gen_random_uuid()),
    "Name" character varying(255) NOT NULL,
    "CPF" character varying(14) NOT NULL,
    "RG" character varying(20) NOT NULL,
    "Email" character varying(255) NOT NULL,
    "PostalCode" character varying(10) NOT NULL,
    "Address" character varying(255) NOT NULL,
    "BankAccount" character varying(30) NOT NULL,
    "Status" boolean NOT NULL DEFAULT TRUE,
    "PasswordHash" character varying(255) NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL DEFAULT (NOW()),
    "UpdatedAt" timestamp with time zone NOT NULL DEFAULT (NOW()),
    "DNA" jsonb NOT NULL DEFAULT ('{}'::jsonb),
    "Neighborhood" text NOT NULL,
    "City" text NOT NULL,
    CONSTRAINT "PK_Models" PRIMARY KEY ("IdModel")
);

CREATE TABLE "ModelsBilling" (
    "Id" uuid NOT NULL DEFAULT (gen_random_uuid()),
    "IdModel" uuid NOT NULL,
    "Amount" numeric(10,2) NOT NULL,
    "BillingDate" timestamp with time zone NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL DEFAULT (NOW()),
    "UpdatedAt" timestamp with time zone NOT NULL DEFAULT (NOW()),
    CONSTRAINT "PK_ModelsBilling" PRIMARY KEY ("Id")
);

CREATE TABLE "Users" (
    "UserId" uuid NOT NULL DEFAULT (gen_random_uuid()),
    "Email" character varying(255) NOT NULL,
    "Name" character varying(255) NOT NULL,
    "PasswordHash" character varying(255) NOT NULL,
    "UserType" character varying(50) NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL DEFAULT (NOW()),
    "UpdatedAt" timestamp with time zone NOT NULL DEFAULT (NOW()),
    CONSTRAINT "PK_Users" PRIMARY KEY ("UserId")
);

CREATE TABLE "AgencyContracts" (
    "ContractId" uuid NOT NULL,
    CONSTRAINT "PK_AgencyContracts" PRIMARY KEY ("ContractId"),
    CONSTRAINT "FK_AgencyContracts_Contracts_ContractId" FOREIGN KEY ("ContractId") REFERENCES "Contracts" ("ContractId") ON DELETE CASCADE
);

CREATE TABLE "CommitmentTerms" (
    "ContractId" uuid NOT NULL,
    CONSTRAINT "PK_CommitmentTerms" PRIMARY KEY ("ContractId"),
    CONSTRAINT "FK_CommitmentTerms_Contracts_ContractId" FOREIGN KEY ("ContractId") REFERENCES "Contracts" ("ContractId") ON DELETE CASCADE
);

CREATE TABLE "ImageRightsContracts" (
    "ContractId" uuid NOT NULL,
    CONSTRAINT "PK_ImageRightsContracts" PRIMARY KEY ("ContractId"),
    CONSTRAINT "FK_ImageRightsContracts_Contracts_ContractId" FOREIGN KEY ("ContractId") REFERENCES "Contracts" ("ContractId") ON DELETE CASCADE
);

CREATE TABLE "PhotographyProductionContracts" (
    "ContractId" uuid NOT NULL,
    CONSTRAINT "PK_PhotographyProductionContracts" PRIMARY KEY ("ContractId"),
    CONSTRAINT "FK_PhotographyProductionContracts_Contracts_ContractId" FOREIGN KEY ("ContractId") REFERENCES "Contracts" ("ContractId") ON DELETE CASCADE
);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250122150957_InitialMigration', '9.0.1');

ALTER TABLE "Models" ALTER COLUMN "Neighborhood" DROP NOT NULL;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250122152335_UpdateNeighborhoodNullable', '9.0.1');

ALTER TABLE "Models" ALTER COLUMN "City" DROP NOT NULL;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250122153005_UpdateCityNullable', '9.0.1');

ALTER TABLE "Contracts" ADD "CodProposta" integer NOT NULL DEFAULT 110;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250122195613_AddCodPropostaToContracts', '9.0.1');

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250122201534_EditCodPropostaToContracts', '9.0.1');

CREATE TABLE "ModelJob" (
    "ModelJobId" uuid NOT NULL,
    "ModelId" uuid NOT NULL,
    "JobId" uuid NOT NULL,
    "JobDate" timestamp with time zone NOT NULL,
    "Location" character varying(255) NOT NULL,
    "Time" character varying(50) NOT NULL,
    "AdditionalDescription" character varying(500) NOT NULL,
    CONSTRAINT "PK_ModelJob" PRIMARY KEY ("ModelJobId"),
    CONSTRAINT "FK_ModelJob_Jobs_JobId" FOREIGN KEY ("JobId") REFERENCES "Jobs" ("IdJob") ON DELETE CASCADE,
    CONSTRAINT "FK_ModelJob_Models_ModelId" FOREIGN KEY ("ModelId") REFERENCES "Models" ("IdModel") ON DELETE CASCADE
);

CREATE INDEX "IX_ModelJob_JobId" ON "ModelJob" ("JobId");

CREATE INDEX "IX_ModelJob_ModelId" ON "ModelJob" ("ModelId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250122212928_UpdateModelJobConfiguration', '9.0.1');

ALTER TABLE "Contracts" ADD "DataContrato" text NOT NULL DEFAULT '';

ALTER TABLE "Contracts" ADD "LocalContrato" text NOT NULL DEFAULT '';

ALTER TABLE "Contracts" ADD "MesContrato" text NOT NULL DEFAULT '';

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250125001537_AddContractFields', '9.0.1');

ALTER TABLE "Contracts" ADD "BairroEmpresa" text NOT NULL DEFAULT '';

ALTER TABLE "Contracts" ADD "CEPEmpresa" text NOT NULL DEFAULT '';

ALTER TABLE "Contracts" ADD "CNPJEmpresa" text NOT NULL DEFAULT '';

ALTER TABLE "Contracts" ADD "CidadeEmpresa" text NOT NULL DEFAULT '';

ALTER TABLE "Contracts" ADD "ComplementoEmpresa" text NOT NULL DEFAULT '';

ALTER TABLE "Contracts" ADD "EnderecoEmpresa" text NOT NULL DEFAULT '';

ALTER TABLE "Contracts" ADD "NomeEmpresa" text NOT NULL DEFAULT '';

ALTER TABLE "Contracts" ADD "NumeroEmpresa" text NOT NULL DEFAULT '';

ALTER TABLE "Contracts" ADD "TelefonePrincipal" text NOT NULL DEFAULT '';

ALTER TABLE "Contracts" ADD "TelefoneSecundario" text NOT NULL DEFAULT '';

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250125213114_AddNewFieldsToContractBase', '9.0.1');

ALTER TABLE "Contracts" DROP COLUMN "TelefonePrincipal";

ALTER TABLE "Contracts" DROP COLUMN "TelefoneSecundario";

ALTER TABLE "Models" ADD "TelefonePrincipal" text NOT NULL DEFAULT '';

ALTER TABLE "Models" ADD "TelefoneSecundario" text NOT NULL DEFAULT '';

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250125214137_RemovePhoneFieldsFromContract', '9.0.1');

ALTER TABLE "Models" ADD "NumberAddress" text NOT NULL DEFAULT '';

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250125221015_AddNumberAddressToModel', '9.0.1');

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250126002353_AddComplementModels', '9.0.1');

ALTER TABLE "Contracts" ADD "FormaPagamento" text NOT NULL DEFAULT '';

ALTER TABLE "Contracts" ADD "ValorContrato" numeric NOT NULL DEFAULT 0.0;

CREATE TABLE "Payment" (
    "Id" uuid NOT NULL,
    "ContractId" uuid NOT NULL,
    "Valor" numeric NOT NULL,
    "QuantidadeParcela" integer,
    "FinalCartao" text NOT NULL,
    "DataPagamento" timestamp with time zone NOT NULL,
    "StatusPagamento" text NOT NULL,
    "Comprovante" bytea NOT NULL,
    "DataAcordoPagamento" timestamp with time zone,
    "MetodoPagamento" text NOT NULL,
    CONSTRAINT "PK_Payment" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Payment_Contracts_ContractId" FOREIGN KEY ("ContractId") REFERENCES "Contracts" ("ContractId") ON DELETE CASCADE
);

CREATE INDEX "IX_Payment_ContractId" ON "Payment" ("ContractId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250126233935_AddFieldsToContracts', '9.0.1');

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250127000151_UpdateEnumsToStrings', '9.0.1');

ALTER TABLE "Payment" ALTER COLUMN "FinalCartao" TYPE character varying(4);
ALTER TABLE "Payment" ALTER COLUMN "FinalCartao" DROP NOT NULL;

ALTER TABLE "Payment" ALTER COLUMN "Comprovante" DROP NOT NULL;

UPDATE "Models" SET "Neighborhood" = '' WHERE "Neighborhood" IS NULL;
ALTER TABLE "Models" ALTER COLUMN "Neighborhood" SET NOT NULL;
ALTER TABLE "Models" ALTER COLUMN "Neighborhood" SET DEFAULT '';

UPDATE "Models" SET "City" = '' WHERE "City" IS NULL;
ALTER TABLE "Models" ALTER COLUMN "City" SET NOT NULL;
ALTER TABLE "Models" ALTER COLUMN "City" SET DEFAULT '';

ALTER TABLE "ContractsModels" ALTER COLUMN "UpdatedAt" DROP DEFAULT;

ALTER TABLE "ContractsModels" ALTER COLUMN "CreatedAt" DROP DEFAULT;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250127201033_RevertUpdateEnumsToStrings', '9.0.1');

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250127202103_RevertEnumsToOriginal', '9.0.1');

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250127212127_CapturePendingChanges', '9.0.1');

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250127223714_AddStatusPagamentoToContracts', '9.0.1');

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250128085545_TemporaryMigrationCheck', '9.0.1');

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250128090846_CorrectedMigration', '9.0.1');

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250129013424_InitialCreate', '9.0.1');

ALTER TABLE "ModelJob" ADD "Status" text NOT NULL DEFAULT '';

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250129032805_AddStatusToModelJob', '9.0.1');

ALTER TABLE "ModelJob" DROP CONSTRAINT "FK_ModelJob_Models_ModelId";

DROP TABLE "Models";

CREATE TABLE "Model" (
    "IdModel" uuid NOT NULL DEFAULT (gen_random_uuid()),
    "Name" character varying(255) NOT NULL,
    "CPF" character varying(14) NOT NULL,
    "RG" character varying(20) NOT NULL,
    "Email" character varying(255) NOT NULL,
    "PostalCode" character varying(10) NOT NULL,
    "Address" character varying(255) NOT NULL,
    "NumberAddress" text NOT NULL,
    "Complement" text NOT NULL,
    "BankAccount" character varying(30) NOT NULL,
    "Status" boolean NOT NULL DEFAULT TRUE,
    "PasswordHash" character varying(255) NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL DEFAULT (NOW()),
    "UpdatedAt" timestamp with time zone NOT NULL DEFAULT (NOW()),
    "DNA" jsonb NOT NULL DEFAULT ('{}'::jsonb),
    "Neighborhood" text NOT NULL,
    "City" text NOT NULL,
    "TelefonePrincipal" text NOT NULL,
    "TelefoneSecundario" text NOT NULL,
    CONSTRAINT "PK_Model" PRIMARY KEY ("IdModel")
);

CREATE INDEX "IX_Contracts_ModelId" ON "Contracts" ("ModelId");

ALTER TABLE "Contracts" ADD CONSTRAINT "FK_Contracts_Model_ModelId" FOREIGN KEY ("ModelId") REFERENCES "Model" ("IdModel") ON DELETE CASCADE;

ALTER TABLE "ModelJob" ADD CONSTRAINT "FK_ModelJob_Model_ModelId" FOREIGN KEY ("ModelId") REFERENCES "Model" ("IdModel") ON DELETE CASCADE;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250131044055_FixContractModelRelationship', '9.0.1');

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250131044429_RenameModelsToModel', '9.0.1');

ALTER TABLE "ContractsModels" DROP COLUMN "CreatedAt";

ALTER TABLE "ContractsModels" DROP COLUMN "ModelId";

ALTER TABLE "ContractsModels" DROP COLUMN "UpdatedAt";

ALTER TABLE "ContractsModels" ADD CONSTRAINT "FK_ContractsModels_Contracts_ContractId" FOREIGN KEY ("ContractId") REFERENCES "Contracts" ("ContractId") ON DELETE CASCADE;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250131072301_SyncDatabaseWithModels', '9.0.1');

UPDATE "LoginUserRequest" SET "Username" = '' WHERE "Username" IS NULL;
ALTER TABLE "LoginUserRequest" ALTER COLUMN "Username" SET NOT NULL;
ALTER TABLE "LoginUserRequest" ALTER COLUMN "Username" SET DEFAULT '';

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250204013836_InitialCreateServerPregiato', '9.0.1');

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250204015704_NovaMigracaoServidor', '9.0.1');

COMMIT;

