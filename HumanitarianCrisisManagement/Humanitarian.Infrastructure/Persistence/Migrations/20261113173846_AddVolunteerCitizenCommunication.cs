using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Humanitarian.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddVolunteerCitizenCommunication : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AssignedVolunteerId",
                table: "AssistanceRequests",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DirectMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SenderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReceiverId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MessageBody = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DirectMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DirectMessages_AssistanceRequests_RequestId",
                        column: x => x.RequestId,
                        principalTable: "AssistanceRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssistanceRequests_AssignedVolunteerId",
                table: "AssistanceRequests",
                column: "AssignedVolunteerId");

            migrationBuilder.CreateIndex(
                name: "IX_DirectMessages_RequestId",
                table: "DirectMessages",
                column: "RequestId");

            migrationBuilder.AddForeignKey(
                name: "FK_AssistanceRequests_Users_AssignedVolunteerId",
                table: "AssistanceRequests",
                column: "AssignedVolunteerId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AssistanceRequests_Users_AssignedVolunteerId",
                table: "AssistanceRequests");

            migrationBuilder.DropTable(
                name: "DirectMessages");

            migrationBuilder.DropIndex(
                name: "IX_AssistanceRequests_AssignedVolunteerId",
                table: "AssistanceRequests");

            migrationBuilder.DropColumn(
                name: "AssignedVolunteerId",
                table: "AssistanceRequests");
        }
    }
}
