using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable
#pragma warning disable CA1861 // EF Core scaffolds composite index columns as inline arrays

namespace CPG.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MarketplaceEvolution_Sprint0 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CompanyName",
                table: "users",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "users",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AgentId",
                table: "loads",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "LastKnownLatitude",
                table: "loads",
                type: "numeric(9,6)",
                precision: 9,
                scale: 6,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "LastKnownLongitude",
                table: "loads",
                type: "numeric(9,6)",
                precision: 9,
                scale: 6,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastTelemetryAtUtc",
                table: "loads",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ProjectedAgentCommissionUsd",
                table: "loads",
                type: "numeric(12,2)",
                precision: 12,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StripeConnectAccountId",
                table: "carriers",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StripeOnboardingStatus",
                table: "carriers",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "NotStarted");

            migrationBuilder.CreateTable(
                name: "agent_client_invitations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AgentId = table.Column<Guid>(type: "uuid", nullable: false),
                    InvitedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    AcceptedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Token = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ExpiresAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModifiedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_agent_client_invitations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "agents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CpgLicenseReference = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CommissionRatePercent = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    StripeConnectAccountId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModifiedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_agents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "case_studies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Slug = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ServiceType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SummaryMarkdown = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    BodyMarkdown = table.Column<string>(type: "text", nullable: false),
                    HeroImageBlobUri = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    IsPublished = table.Column<bool>(type: "boolean", nullable: false),
                    PublishedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModifiedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_case_studies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "lane_rate_statistics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OriginZip3 = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    DestinationZip3 = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    ServiceType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Month = table.Column<int>(type: "integer", nullable: false),
                    AvgRatePerMileUsd = table.Column<decimal>(type: "numeric(8,4)", precision: 8, scale: 4, nullable: false),
                    P25RatePerMileUsd = table.Column<decimal>(type: "numeric(8,4)", precision: 8, scale: 4, nullable: false),
                    P75RatePerMileUsd = table.Column<decimal>(type: "numeric(8,4)", precision: 8, scale: 4, nullable: false),
                    SampleSize = table.Column<int>(type: "integer", nullable: false),
                    ComputedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lane_rate_statistics", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "payment_disbursements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LoadId = table.Column<Guid>(type: "uuid", nullable: false),
                    InvoiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    CarrierId = table.Column<Guid>(type: "uuid", nullable: false),
                    AgentId = table.Column<Guid>(type: "uuid", nullable: true),
                    GrossAmountUsd = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    CpgMarginAmountUsd = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    AgentCommissionAmountUsd = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    QuickPayFeeAmountUsd = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    CarrierNetAmountUsd = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    QuickPayRequested = table.Column<bool>(type: "boolean", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    StripeTransferId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ProcessedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    FailureReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModifiedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payment_disbursements", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "telemetry_devices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CarrierId = table.Column<Guid>(type: "uuid", nullable: false),
                    Provider = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ExternalDeviceId = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    WebhookSecretHash = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    LastSeenAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModifiedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_telemetry_devices", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "telemetry_log",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LoadId = table.Column<Guid>(type: "uuid", nullable: false),
                    TelemetryDeviceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Latitude = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: false),
                    Longitude = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: false),
                    SpeedMph = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: true),
                    HeadingDegrees = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    RecordedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ReceivedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_telemetry_log", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_loads_AgentId",
                table: "loads",
                column: "AgentId");

            migrationBuilder.CreateIndex(
                name: "IX_carriers_StripeConnectAccountId",
                table: "carriers",
                column: "StripeConnectAccountId",
                unique: true,
                filter: "\"StripeConnectAccountId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_agent_client_invitations_AgentId",
                table: "agent_client_invitations",
                column: "AgentId");

            migrationBuilder.CreateIndex(
                name: "IX_agent_client_invitations_Token",
                table: "agent_client_invitations",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_agents_StripeConnectAccountId",
                table: "agents",
                column: "StripeConnectAccountId",
                unique: true,
                filter: "\"StripeConnectAccountId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_agents_UserId",
                table: "agents",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_case_studies_IsPublished",
                table: "case_studies",
                column: "IsPublished");

            migrationBuilder.CreateIndex(
                name: "IX_case_studies_Slug",
                table: "case_studies",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_lane_rate_statistics_OriginZip3_DestinationZip3_ServiceType~",
                table: "lane_rate_statistics",
                columns: new[] { "OriginZip3", "DestinationZip3", "ServiceType", "Month" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_payment_disbursements_AgentId",
                table: "payment_disbursements",
                column: "AgentId");

            migrationBuilder.CreateIndex(
                name: "IX_payment_disbursements_CarrierId",
                table: "payment_disbursements",
                column: "CarrierId");

            migrationBuilder.CreateIndex(
                name: "IX_payment_disbursements_InvoiceId",
                table: "payment_disbursements",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_payment_disbursements_LoadId",
                table: "payment_disbursements",
                column: "LoadId");

            migrationBuilder.CreateIndex(
                name: "IX_payment_disbursements_Status",
                table: "payment_disbursements",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_telemetry_devices_CarrierId",
                table: "telemetry_devices",
                column: "CarrierId");

            migrationBuilder.CreateIndex(
                name: "IX_telemetry_devices_Provider_ExternalDeviceId",
                table: "telemetry_devices",
                columns: new[] { "Provider", "ExternalDeviceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_telemetry_log_LoadId_RecordedAtUtc",
                table: "telemetry_log",
                columns: new[] { "LoadId", "RecordedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_telemetry_log_TelemetryDeviceId",
                table: "telemetry_log",
                column: "TelemetryDeviceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "agent_client_invitations");

            migrationBuilder.DropTable(
                name: "agents");

            migrationBuilder.DropTable(
                name: "case_studies");

            migrationBuilder.DropTable(
                name: "lane_rate_statistics");

            migrationBuilder.DropTable(
                name: "payment_disbursements");

            migrationBuilder.DropTable(
                name: "telemetry_devices");

            migrationBuilder.DropTable(
                name: "telemetry_log");

            migrationBuilder.DropIndex(
                name: "IX_loads_AgentId",
                table: "loads");

            migrationBuilder.DropIndex(
                name: "IX_carriers_StripeConnectAccountId",
                table: "carriers");

            migrationBuilder.DropColumn(
                name: "CompanyName",
                table: "users");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "users");

            migrationBuilder.DropColumn(
                name: "AgentId",
                table: "loads");

            migrationBuilder.DropColumn(
                name: "LastKnownLatitude",
                table: "loads");

            migrationBuilder.DropColumn(
                name: "LastKnownLongitude",
                table: "loads");

            migrationBuilder.DropColumn(
                name: "LastTelemetryAtUtc",
                table: "loads");

            migrationBuilder.DropColumn(
                name: "ProjectedAgentCommissionUsd",
                table: "loads");

            migrationBuilder.DropColumn(
                name: "StripeConnectAccountId",
                table: "carriers");

            migrationBuilder.DropColumn(
                name: "StripeOnboardingStatus",
                table: "carriers");
        }
    }
}
