using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jude.Server.Migrations
{
    /// <inheritdoc />
    public partial class add_resoning_log : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AgentReasoning",
                table: "Claims");

            migrationBuilder.AddColumn<List<string>>(
                name: "AgentReasoningLog",
                table: "Claims",
                type: "text[]",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AgentReasoningLog",
                table: "Claims");

            migrationBuilder.AddColumn<string>(
                name: "AgentReasoning",
                table: "Claims",
                type: "text",
                nullable: true);
        }
    }
}
