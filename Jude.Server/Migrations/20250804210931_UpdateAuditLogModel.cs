using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jude.Server.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAuditLogModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "ClaimReviews");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "ClaimReviews",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
