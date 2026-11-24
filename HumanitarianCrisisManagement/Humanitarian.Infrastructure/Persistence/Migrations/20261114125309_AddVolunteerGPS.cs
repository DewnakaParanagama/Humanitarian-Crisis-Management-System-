using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Humanitarian.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddVolunteerGPS : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "VolunteerProfiles",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LocationLastUpdatedUtc",
                table: "VolunteerProfiles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "VolunteerProfiles",
                type: "float",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "VolunteerProfiles");

            migrationBuilder.DropColumn(
                name: "LocationLastUpdatedUtc",
                table: "VolunteerProfiles");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "VolunteerProfiles");
        }
    }
}
