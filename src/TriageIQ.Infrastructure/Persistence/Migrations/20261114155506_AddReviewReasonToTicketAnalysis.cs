using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TriageIQ.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddReviewReasonToTicketAnalysis : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReviewReason",
                table: "TicketAnalyses",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReviewReason",
                table: "TicketAnalyses");
        }
    }
}
