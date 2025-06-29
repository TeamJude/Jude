using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jude.Server.Migrations
{
    /// <inheritdoc />
    public partial class claims : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Claims",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IngestedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TransactionNumber = table.Column<string>(type: "text", nullable: false),
                    ClaimNumber = table.Column<string>(type: "text", nullable: false),
                    PatientName = table.Column<string>(type: "text", nullable: false),
                    MembershipNumber = table.Column<string>(type: "text", nullable: false),
                    ProviderPractice = table.Column<string>(type: "text", nullable: false),
                    ClaimAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    ApprovedAmount = table.Column<decimal>(type: "numeric", nullable: true),
                    Currency = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Source = table.Column<int>(type: "integer", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CIMASPayload = table.Column<string>(type: "text", nullable: true),
                    AgentRecommendation = table.Column<string>(type: "text", nullable: true),
                    AgentReasoning = table.Column<string>(type: "text", nullable: true),
                    AgentConfidenceScore = table.Column<decimal>(type: "numeric", nullable: true),
                    AgentProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsFlagged = table.Column<bool>(type: "boolean", nullable: false),
                    FraudIndicators = table.Column<List<string>>(type: "text[]", nullable: true),
                    FraudRiskLevel = table.Column<int>(type: "integer", nullable: false),
                    RequiresHumanReview = table.Column<bool>(type: "boolean", nullable: false),
                    FinalDecision = table.Column<int>(type: "integer", nullable: true),
                    ReviewerComments = table.Column<string>(type: "text", nullable: true),
                    RejectionReason = table.Column<string>(type: "text", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReviewedById = table.Column<Guid>(type: "uuid", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Claims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Claims_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Claims_UserId",
                table: "Claims",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Claims");
        }
    }
}
