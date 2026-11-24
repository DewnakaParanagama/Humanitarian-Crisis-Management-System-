using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Humanitarian.Infrastructure.Persistence.Migrations;

public partial class AddAssistanceRequestAIAnalysis : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "AISummary",
            table: "AssistanceRequests",
            type: "nvarchar(4000)",
            maxLength: 4000,
            nullable: true);

        migrationBuilder.AlterColumn<decimal>(
            name: "AIConfidenceScore",
            table: "AssistanceRequests",
            type: "decimal(5,2)",
            precision: 5,
            scale: 2,
            nullable: true,
            oldClrType: typeof(decimal),
            oldType: "decimal(5,4)",
            oldPrecision: 5,
            oldScale: 4);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<decimal>(
            name: "AIConfidenceScore",
            table: "AssistanceRequests",
            type: "decimal(5,4)",
            precision: 5,
            scale: 4,
            nullable: true,
            oldClrType: typeof(decimal),
            oldType: "decimal(5,2)",
            oldPrecision: 5,
            oldScale: 2);

        migrationBuilder.DropColumn(
            name: "AISummary",
            table: "AssistanceRequests");
    }
}
