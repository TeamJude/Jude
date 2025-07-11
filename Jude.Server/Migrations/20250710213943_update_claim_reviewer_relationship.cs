using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jude.Server.Migrations
{
    /// <inheritdoc />
    public partial class update_claim_reviewer_relationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Claims_Users_UserId",
                table: "Claims");

            migrationBuilder.DropIndex(
                name: "IX_Claims_UserId",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Claims");

            migrationBuilder.CreateIndex(
                name: "IX_FraudIndicators_CreatedById",
                table: "FraudIndicators",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Claims_ReviewedById",
                table: "Claims",
                column: "ReviewedById");

            migrationBuilder.AddForeignKey(
                name: "FK_Claims_Users_ReviewedById",
                table: "Claims",
                column: "ReviewedById",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FraudIndicators_Users_CreatedById",
                table: "FraudIndicators",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Claims_Users_ReviewedById",
                table: "Claims");

            migrationBuilder.DropForeignKey(
                name: "FK_FraudIndicators_Users_CreatedById",
                table: "FraudIndicators");

            migrationBuilder.DropIndex(
                name: "IX_FraudIndicators_CreatedById",
                table: "FraudIndicators");

            migrationBuilder.DropIndex(
                name: "IX_Claims_ReviewedById",
                table: "Claims");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "Claims",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Claims_UserId",
                table: "Claims",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Claims_Users_UserId",
                table: "Claims",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
