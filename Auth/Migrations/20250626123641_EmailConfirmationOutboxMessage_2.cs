using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Auth.Migrations
{
    /// <inheritdoc />
    public partial class EmailConfirmationOutboxMessage_2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_EmailConfirmationOutboxMessage",
                schema: "identity",
                table: "EmailConfirmationOutboxMessage");

            migrationBuilder.RenameTable(
                name: "EmailConfirmationOutboxMessage",
                schema: "identity",
                newName: "EmailConfirmationOutboxMessages",
                newSchema: "identity");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EmailConfirmationOutboxMessages",
                schema: "identity",
                table: "EmailConfirmationOutboxMessages",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_EmailConfirmationOutboxMessages",
                schema: "identity",
                table: "EmailConfirmationOutboxMessages");

            migrationBuilder.RenameTable(
                name: "EmailConfirmationOutboxMessages",
                schema: "identity",
                newName: "EmailConfirmationOutboxMessage",
                newSchema: "identity");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EmailConfirmationOutboxMessage",
                schema: "identity",
                table: "EmailConfirmationOutboxMessage",
                column: "Id");
        }
    }
}
