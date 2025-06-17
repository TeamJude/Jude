using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jude.Server.Migrations
{
    /// <inheritdoc />
    public partial class add_priority_to_rules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Priority",
                table: "Rules",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Priority",
                table: "Rules");
        }
    }
}
