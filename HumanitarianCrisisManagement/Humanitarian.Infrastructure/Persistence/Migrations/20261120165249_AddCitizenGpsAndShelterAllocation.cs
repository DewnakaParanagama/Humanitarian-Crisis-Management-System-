using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Humanitarian.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCitizenGpsAndShelterAllocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AssignedShelterId",
                table: "AssistanceRequests",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AssignedShelterName",
                table: "AssistanceRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "AssistanceRequests",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "AssistanceRequests",
                type: "float",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AssistanceRequests_AssignedShelterId",
                table: "AssistanceRequests",
                column: "AssignedShelterId");

            migrationBuilder.AddForeignKey(
                name: "FK_AssistanceRequests_Shelters_AssignedShelterId",
                table: "AssistanceRequests",
                column: "AssignedShelterId",
                principalTable: "Shelters",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AssistanceRequests_Shelters_AssignedShelterId",
                table: "AssistanceRequests");

            migrationBuilder.DropIndex(
                name: "IX_AssistanceRequests_AssignedShelterId",
                table: "AssistanceRequests");

            migrationBuilder.DropColumn(
                name: "AssignedShelterId",
                table: "AssistanceRequests");

            migrationBuilder.DropColumn(
                name: "AssignedShelterName",
                table: "AssistanceRequests");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "AssistanceRequests");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "AssistanceRequests");
        }
    }
}
