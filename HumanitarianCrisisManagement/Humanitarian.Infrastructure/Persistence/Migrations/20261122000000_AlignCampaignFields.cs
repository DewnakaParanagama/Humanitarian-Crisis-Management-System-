using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Humanitarian.Infrastructure.Persistence.Migrations;

public partial class AlignCampaignFields : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            IF COL_LENGTH('dbo.Campaigns', 'Description') IS NULL
            BEGIN
                ALTER TABLE [dbo].[Campaigns]
                ADD [Description] nvarchar(max) NOT NULL
                DEFAULT N'';
            END;

            IF COL_LENGTH('dbo.Campaigns', 'EndDate') IS NULL
            BEGIN
                ALTER TABLE [dbo].[Campaigns]
                ADD [EndDate] datetime2 NULL;
            END;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            IF COL_LENGTH('dbo.Campaigns', 'EndDate') IS NOT NULL
                ALTER TABLE [dbo].[Campaigns] DROP COLUMN [EndDate];

            IF COL_LENGTH('dbo.Campaigns', 'Description') IS NOT NULL
                ALTER TABLE [dbo].[Campaigns] DROP COLUMN [Description];
            """);
    }
}