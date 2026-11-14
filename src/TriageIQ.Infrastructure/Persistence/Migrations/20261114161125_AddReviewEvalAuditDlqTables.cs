using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TriageIQ.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddReviewEvalAuditDlqTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Action = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Entity = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    EntityId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    Details = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DlqMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ServiceBusMessageId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    PayloadJson = table.Column<string>(type: "nvarchar(max)", maxLength: 8000, nullable: false),
                    ErrorReason = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    DeliveryCount = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ReplayedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DiscardedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DlqMessages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EvalRuns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PromptVersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DatasetName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Mode = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Total = table.Column<int>(type: "int", nullable: false),
                    CategoryAccuracy = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MacroF1 = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    JsonValidityRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InjectionCatchRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AvgCostUsd = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    P95LatencyMs = table.Column<int>(type: "int", nullable: false),
                    Passed = table.Column<bool>(type: "bit", nullable: false),
                    ReportJson = table.Column<string>(type: "nvarchar(max)", maxLength: 8000, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EvalRuns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReviewActions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TicketId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReviewerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Action = table.Column<int>(type: "int", nullable: false),
                    EditedCategory = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    EditedPriority = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    EditedReply = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Comment = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReviewActions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_CreatedAt",
                table: "AuditLogs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_DlqMessages_CreatedAt",
                table: "DlqMessages",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_EvalRuns_CreatedAt",
                table: "EvalRuns",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewActions_CreatedAt",
                table: "ReviewActions",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewActions_ReviewerId",
                table: "ReviewActions",
                column: "ReviewerId");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewActions_TicketId",
                table: "ReviewActions",
                column: "TicketId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "DlqMessages");

            migrationBuilder.DropTable(
                name: "EvalRuns");

            migrationBuilder.DropTable(
                name: "ReviewActions");
        }
    }
}
