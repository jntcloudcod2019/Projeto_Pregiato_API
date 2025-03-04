Build started...
Build succeeded.
CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250120072246_UpdateContractHierarchy') THEN
    CREATE TABLE "AgencyContracts" (
        "ContractId" uuid NOT NULL,
        "ModelId" uuid NOT NULL,
        "JobId" uuid NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone NOT NULL,
        "ContractFilePath" text NOT NULL,
        CONSTRAINT "PK_AgencyContracts" PRIMARY KEY ("ContractId")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250120072246_UpdateContractHierarchy') THEN
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
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250120072246_UpdateContractHierarchy') THEN
    CREATE TABLE "ClientsBilling" (
        "BillingId" uuid NOT NULL,
        "ClientId" uuid NOT NULL,
        "Amount" numeric NOT NULL,
        "BillingDate" timestamp with time zone NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_ClientsBilling" PRIMARY KEY ("BillingId")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250120072246_UpdateContractHierarchy') THEN
    CREATE TABLE "CommitmentTerms" (
        "ContractId" uuid NOT NULL,
        "ModelId" uuid NOT NULL,
        "JobId" uuid NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone NOT NULL,
        "ContractFilePath" text NOT NULL,
        CONSTRAINT "PK_CommitmentTerms" PRIMARY KEY ("ContractId")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250120072246_UpdateContractHierarchy') THEN
    CREATE TABLE "ContractsModels" (
        "ContractId" uuid NOT NULL DEFAULT (gen_random_uuid()),
        "ModelId" uuid NOT NULL,
        "ContractFile" text NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL DEFAULT (NOW()),
        "UpdatedAt" timestamp with time zone DEFAULT (NOW()),
        CONSTRAINT "PK_ContractsModels" PRIMARY KEY ("ContractId"),
        CONSTRAINT "CK_ContractsModels_FileFormat" CHECK (ContractFile ~ '\.(doc|docx|pdf)$')
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250120072246_UpdateContractHierarchy') THEN
    CREATE TABLE "ImageRightsTerms" (
        "ContractId" uuid NOT NULL,
        "ModelId" uuid NOT NULL,
        "JobId" uuid NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone NOT NULL,
        "ContractFilePath" text NOT NULL,
        CONSTRAINT "PK_ImageRightsTerms" PRIMARY KEY ("ContractId")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250120072246_UpdateContractHierarchy') THEN
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
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250120072246_UpdateContractHierarchy') THEN
    CREATE TABLE "LoginUserRequest" (
        "Username" text NOT NULL,
        "Password" text NOT NULL,
        "UserType" text NOT NULL
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250120072246_UpdateContractHierarchy') THEN
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
        CONSTRAINT "PK_Models" PRIMARY KEY ("IdModel")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250120072246_UpdateContractHierarchy') THEN
    CREATE TABLE "ModelsBilling" (
        "Id" uuid NOT NULL DEFAULT (gen_random_uuid()),
        "IdModel" uuid NOT NULL,
        "Amount" numeric(10,2) NOT NULL,
        "BillingDate" timestamp with time zone NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL DEFAULT (NOW()),
        "UpdatedAt" timestamp with time zone NOT NULL DEFAULT (NOW()),
        CONSTRAINT "PK_ModelsBilling" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250120072246_UpdateContractHierarchy') THEN
    CREATE TABLE "PhotographyProductionContracts" (
        "ContractId" uuid NOT NULL,
        "ModelId" uuid NOT NULL,
        "JobId" uuid NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone NOT NULL,
        "ContractFilePath" text NOT NULL,
        CONSTRAINT "PK_PhotographyProductionContracts" PRIMARY KEY ("ContractId")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250120072246_UpdateContractHierarchy') THEN
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
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250120072246_UpdateContractHierarchy') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20250120072246_UpdateContractHierarchy', '9.0.1');
    END IF;
END $EF$;
COMMIT;


