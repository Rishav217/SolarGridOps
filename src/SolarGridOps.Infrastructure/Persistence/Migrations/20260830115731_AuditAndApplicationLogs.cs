using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SolarGridOps.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AuditAndApplicationLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApplicationLogEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LogLevel = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    EventKey = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Details = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ExceptionType = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationLogEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AuditTrailEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ActionKey = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    EntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Details = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditTrailEntries", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationLogEntries_Category_EventKey",
                table: "ApplicationLogEntries",
                columns: new[] { "Category", "EventKey" });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationLogEntries_CreatedAtUtc_LogLevel",
                table: "ApplicationLogEntries",
                columns: new[] { "CreatedAtUtc", "LogLevel" });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationLogEntries_CreatedByUserId",
                table: "ApplicationLogEntries",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditTrailEntries_CreatedAtUtc_ActionKey",
                table: "AuditTrailEntries",
                columns: new[] { "CreatedAtUtc", "ActionKey" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditTrailEntries_CreatedByUserId",
                table: "AuditTrailEntries",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditTrailEntries_EntityType_EntityId",
                table: "AuditTrailEntries",
                columns: new[] { "EntityType", "EntityId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApplicationLogEntries");

            migrationBuilder.DropTable(
                name: "AuditTrailEntries");
        }
    }
}
