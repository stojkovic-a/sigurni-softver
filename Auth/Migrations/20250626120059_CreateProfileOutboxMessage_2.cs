using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Auth.Migrations
{
    /// <inheritdoc />
    public partial class CreateProfileOutboxMessage_2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "OccuredAt",
                schema: "identity",
                table: "CreateProfileOutboxMessages",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "ProcessedAt",
                schema: "identity",
                table: "CreateProfileOutboxMessages",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "Succeeded",
                schema: "identity",
                table: "CreateProfileOutboxMessages",
                type: "boolean",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OccuredAt",
                schema: "identity",
                table: "CreateProfileOutboxMessages");

            migrationBuilder.DropColumn(
                name: "ProcessedAt",
                schema: "identity",
                table: "CreateProfileOutboxMessages");

            migrationBuilder.DropColumn(
                name: "Succeeded",
                schema: "identity",
                table: "CreateProfileOutboxMessages");
        }
    }
}
