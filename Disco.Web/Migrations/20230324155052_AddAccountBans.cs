using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Disco.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountBans : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "accountBans",
                columns: table => new
                {
                    accountBanId = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    bannedAccountId = table.Column<long>(type: "INTEGER", nullable: false),
                    reason = table.Column<string>(type: "TEXT", nullable: false),
                    createdAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_accountBans", x => x.accountBanId);
                });

            migrationBuilder.CreateTable(
                name: "accountDiscordBans",
                columns: table => new
                {
                    accountDiscordBanId = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    discordId = table.Column<long>(type: "INTEGER", nullable: false),
                    bannedAccountId = table.Column<long>(type: "INTEGER", nullable: true),
                    createdAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_accountDiscordBans", x => x.accountDiscordBanId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_accountBans_bannedAccountId",
                table: "accountBans",
                column: "bannedAccountId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_accountDiscordBans_discordId",
                table: "accountDiscordBans",
                column: "discordId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "accountBans");

            migrationBuilder.DropTable(
                name: "accountDiscordBans");
        }
    }
}
