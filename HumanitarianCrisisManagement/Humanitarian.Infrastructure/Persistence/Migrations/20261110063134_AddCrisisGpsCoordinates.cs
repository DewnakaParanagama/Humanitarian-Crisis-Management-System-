using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Humanitarian.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCrisisGpsCoordinates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "Latitude",
                table: "Crises",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "Longitude",
                table: "Crises",
                type: "real",
                nullable: false,
                defaultValue: 0f);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Crises");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Crises");
        }
    }
}
