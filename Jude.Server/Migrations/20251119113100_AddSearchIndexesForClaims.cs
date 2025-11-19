using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jude.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddSearchIndexesForClaims : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Claims_ClaimNumber",
                table: "Claims",
                column: "ClaimNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Claims_MemberNumber",
                table: "Claims",
                column: "MemberNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Claims_PatientFirstName",
                table: "Claims",
                column: "PatientFirstName");

            migrationBuilder.CreateIndex(
                name: "IX_Claims_PatientSurname",
                table: "Claims",
                column: "PatientSurname");

            migrationBuilder.CreateIndex(
                name: "IX_Claims_PracticeNumber",
                table: "Claims",
                column: "PracticeNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Claims_ProviderName",
                table: "Claims",
                column: "ProviderName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Claims_ClaimNumber",
                table: "Claims");

            migrationBuilder.DropIndex(
                name: "IX_Claims_MemberNumber",
                table: "Claims");

            migrationBuilder.DropIndex(
                name: "IX_Claims_PatientFirstName",
                table: "Claims");

            migrationBuilder.DropIndex(
                name: "IX_Claims_PatientSurname",
                table: "Claims");

            migrationBuilder.DropIndex(
                name: "IX_Claims_PracticeNumber",
                table: "Claims");

            migrationBuilder.DropIndex(
                name: "IX_Claims_ProviderName",
                table: "Claims");
        }
    }
}
