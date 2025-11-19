using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jude.Server.Migrations
{
    /// <inheritdoc />
    public partial class UpdateClaimModelFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AsAtNetworks",
                table: "Claims",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AuthNo",
                table: "Claims",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ClaimLineNo",
                table: "Claims",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Dis",
                table: "Claims",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Dl",
                table: "Claims",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DuplicateClaim",
                table: "Claims",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DuplicateClaimLine",
                table: "Claims",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Icd10Code",
                table: "Claims",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Ino",
                table: "Claims",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "PaidFromRiskAmount",
                table: "Claims",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PaidFromSavings",
                table: "Claims",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PaidFromThreshold",
                table: "Claims",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "PaperOrEdi",
                table: "Claims",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PayTo",
                table: "Claims",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "RecoveryAmount",
                table: "Claims",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "ReferringPractice",
                table: "Claims",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Rej",
                table: "Claims",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Rev",
                table: "Claims",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ScriptCode",
                table: "Claims",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "Tariff",
                table: "Claims",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AsAtNetworks",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "AuthNo",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "ClaimLineNo",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "Dis",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "Dl",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "DuplicateClaim",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "DuplicateClaimLine",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "Icd10Code",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "Ino",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "PaidFromRiskAmount",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "PaidFromSavings",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "PaidFromThreshold",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "PaperOrEdi",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "PayTo",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "RecoveryAmount",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "ReferringPractice",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "Rej",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "Rev",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "ScriptCode",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "Tariff",
                table: "Claims");
        }
    }
}
