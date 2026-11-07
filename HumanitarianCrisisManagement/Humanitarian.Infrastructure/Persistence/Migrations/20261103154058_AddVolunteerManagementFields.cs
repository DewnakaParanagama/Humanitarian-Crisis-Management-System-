using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Humanitarian.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddVolunteerManagementFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsVerified",
                table: "VolunteerProfiles",
                newName: "HasVehicle");

            migrationBuilder.AddColumn<string>(
                name: "AvailabilityDetails",
                table: "VolunteerProfiles",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "VehicleType",
                table: "VolunteerProfiles",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VerificationStatus",
                table: "VolunteerProfiles",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvailabilityDetails",
                table: "VolunteerProfiles");

            migrationBuilder.DropColumn(
                name: "VehicleType",
                table: "VolunteerProfiles");

            migrationBuilder.DropColumn(
                name: "VerificationStatus",
                table: "VolunteerProfiles");

            migrationBuilder.RenameColumn(
                name: "HasVehicle",
                table: "VolunteerProfiles",
                newName: "IsVerified");
        }
    }
}
