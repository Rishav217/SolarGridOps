using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SolarGridOps.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InstallationClosureApproval : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_InstallationSessions_ProjectId",
                table: "InstallationSessions");

            migrationBuilder.AddColumn<DateTime>(
                name: "ClosureApprovedAtUtc",
                table: "InstallationSessions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ClosureApprovedByUserId",
                table: "InstallationSessions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClosureNotes",
                table: "InstallationSessions",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ClosureRequestedAtUtc",
                table: "InstallationSessions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ClosureRequestedByUserId",
                table: "InstallationSessions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ClosureStatus",
                table: "InstallationSessions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "CustomerSignatureBase64",
                table: "InstallationSessions",
                type: "nvarchar(max)",
                maxLength: 8000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerSignatureName",
                table: "InstallationSessions",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_InstallationSessions_ProjectId_ClosureStatus",
                table: "InstallationSessions",
                columns: new[] { "ProjectId", "ClosureStatus" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_InstallationSessions_ProjectId_ClosureStatus",
                table: "InstallationSessions");

            migrationBuilder.DropColumn(
                name: "ClosureApprovedAtUtc",
                table: "InstallationSessions");

            migrationBuilder.DropColumn(
                name: "ClosureApprovedByUserId",
                table: "InstallationSessions");

            migrationBuilder.DropColumn(
                name: "ClosureNotes",
                table: "InstallationSessions");

            migrationBuilder.DropColumn(
                name: "ClosureRequestedAtUtc",
                table: "InstallationSessions");

            migrationBuilder.DropColumn(
                name: "ClosureRequestedByUserId",
                table: "InstallationSessions");

            migrationBuilder.DropColumn(
                name: "ClosureStatus",
                table: "InstallationSessions");

            migrationBuilder.DropColumn(
                name: "CustomerSignatureBase64",
                table: "InstallationSessions");

            migrationBuilder.DropColumn(
                name: "CustomerSignatureName",
                table: "InstallationSessions");

            migrationBuilder.CreateIndex(
                name: "IX_InstallationSessions_ProjectId",
                table: "InstallationSessions",
                column: "ProjectId");
        }
    }
}
