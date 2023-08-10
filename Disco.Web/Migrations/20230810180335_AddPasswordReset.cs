using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Disco.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddPasswordReset : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "accountResetPasswords",
                columns: table => new
                {
                    accountResetPasswordId = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    accountId = table.Column<long>(type: "INTEGER", nullable: false),
                    token = table.Column<string>(type: "TEXT", nullable: false),
                    rawResetData = table.Column<string>(type: "TEXT", nullable: true),
                    status = table.Column<int>(type: "INTEGER", nullable: false),
                    method = table.Column<int>(type: "INTEGER", nullable: false),
                    accountDiscordId = table.Column<long>(type: "INTEGER", nullable: true),
                    accountMatrixId = table.Column<long>(type: "INTEGER", nullable: true),
                    createdAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_accountResetPasswords", x => x.accountResetPasswordId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_accountResetPasswords_token",
                table: "accountResetPasswords",
                column: "token",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "accountResetPasswords");
        }
    }
}
