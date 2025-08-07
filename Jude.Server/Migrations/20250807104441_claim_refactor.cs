using System;
using System.Collections.Generic;
using Jude.Server.Domains.Claims.Providers.CIMAS;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jude.Server.Migrations
{
    /// <inheritdoc />
    public partial class claim_refactor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "Citations");

            migrationBuilder.DropTable(
                name: "ClaimReviews");

            migrationBuilder.DropColumn(
                name: "AgentConfidenceScore",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "AgentProcessedAt",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "AgentReasoningLog",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "AgentRecommendation",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "ApprovedAmount",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "CIMASPayload",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "FinalDecision",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "FraudIndicators",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "FraudRiskLevel",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "IsFlagged",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "ProcessedAt",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "RequiresHumanReview",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "ReviewedAt",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "ReviewerComments",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "Source",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "SubmittedAt",
                table: "Claims");

            migrationBuilder.RenameColumn(
                name: "ProviderPractice",
                table: "Claims",
                newName: "PatientSurname");

            migrationBuilder.RenameColumn(
                name: "PatientName",
                table: "Claims",
                newName: "PatientFirstName");

            migrationBuilder.RenameColumn(
                name: "MembershipNumber",
                table: "Claims",
                newName: "MedicalSchemeName");

            migrationBuilder.RenameColumn(
                name: "ClaimAmount",
                table: "Claims",
                newName: "TotalClaimAmount");

            migrationBuilder.AddColumn<ClaimResponse>(
                name: "Data",
                table: "Claims",
                type: "jsonb",
                nullable: false);

            migrationBuilder.CreateTable(
                name: "AgentReviews",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Decision = table.Column<int>(type: "integer", nullable: false),
                    Recommendation = table.Column<string>(type: "text", nullable: false),
                    Reasoning = table.Column<string>(type: "text", nullable: false),
                    ConfidenceScore = table.Column<decimal>(type: "numeric", nullable: false),
                    ClaimId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgentReviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AgentReviews_Claims_ClaimId",
                        column: x => x.ClaimId,
                        principalTable: "Claims",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HumanReviews",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Decision = table.Column<int>(type: "integer", nullable: false),
                    Comments = table.Column<string>(type: "text", nullable: false),
                    ClaimId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HumanReviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HumanReviews_Claims_ClaimId",
                        column: x => x.ClaimId,
                        principalTable: "Claims",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AgentReviews_ClaimId",
                table: "AgentReviews",
                column: "ClaimId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HumanReviews_ClaimId",
                table: "HumanReviews",
                column: "ClaimId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AgentReviews");

            migrationBuilder.DropTable(
                name: "HumanReviews");

            migrationBuilder.DropColumn(
                name: "Data",
                table: "Claims");

            migrationBuilder.RenameColumn(
                name: "TotalClaimAmount",
                table: "Claims",
                newName: "ClaimAmount");

            migrationBuilder.RenameColumn(
                name: "PatientSurname",
                table: "Claims",
                newName: "ProviderPractice");

            migrationBuilder.RenameColumn(
                name: "PatientFirstName",
                table: "Claims",
                newName: "PatientName");

            migrationBuilder.RenameColumn(
                name: "MedicalSchemeName",
                table: "Claims",
                newName: "MembershipNumber");

            migrationBuilder.AddColumn<decimal>(
                name: "AgentConfidenceScore",
                table: "Claims",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AgentProcessedAt",
                table: "Claims",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<List<string>>(
                name: "AgentReasoningLog",
                table: "Claims",
                type: "text[]",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AgentRecommendation",
                table: "Claims",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ApprovedAmount",
                table: "Claims",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CIMASPayload",
                table: "Claims",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "Claims",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "FinalDecision",
                table: "Claims",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<List<string>>(
                name: "FraudIndicators",
                table: "Claims",
                type: "text[]",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FraudRiskLevel",
                table: "Claims",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsFlagged",
                table: "Claims",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ProcessedAt",
                table: "Claims",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "Claims",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresHumanReview",
                table: "Claims",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReviewedAt",
                table: "Claims",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReviewerComments",
                table: "Claims",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Source",
                table: "Claims",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "SubmittedAt",
                table: "Claims",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<string>(type: "text", nullable: false),
                    ActorId = table.Column<Guid>(type: "uuid", nullable: true),
                    ActorType = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    EntityType = table.Column<int>(type: "integer", nullable: false),
                    Metadata = table.Column<Dictionary<string, object>>(type: "jsonb", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Citations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimId = table.Column<Guid>(type: "uuid", nullable: false),
                    CitedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Context = table.Column<string>(type: "text", nullable: false),
                    Quote = table.Column<string>(type: "text", nullable: false),
                    Source = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Citations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Citations_Claims_ClaimId",
                        column: x => x.ClaimId,
                        principalTable: "Claims",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClaimReviews",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReviewerId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Decision = table.Column<int>(type: "integer", nullable: false),
                    IsEdited = table.Column<bool>(type: "boolean", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClaimReviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClaimReviews_Claims_ClaimId",
                        column: x => x.ClaimId,
                        principalTable: "Claims",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClaimReviews_Users_ReviewerId",
                        column: x => x.ReviewerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Citations_ClaimId",
                table: "Citations",
                column: "ClaimId");

            migrationBuilder.CreateIndex(
                name: "IX_ClaimReviews_ClaimId_ReviewerId",
                table: "ClaimReviews",
                columns: new[] { "ClaimId", "ReviewerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClaimReviews_ReviewerId",
                table: "ClaimReviews",
                column: "ReviewerId");
        }
    }
}
