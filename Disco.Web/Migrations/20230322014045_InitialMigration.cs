using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Disco.Web.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "accountDescriptions",
                columns: table => new
                {
                    accountDescriptionId = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    accountId = table.Column<long>(type: "INTEGER", nullable: false),
                    description = table.Column<string>(type: "TEXT", nullable: true),
                    updatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_accountDescriptions", x => x.accountDescriptionId);
                });

            migrationBuilder.CreateTable(
                name: "accountDiscords",
                columns: table => new
                {
                    accountDiscordId = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    accountId = table.Column<long>(type: "INTEGER", nullable: false),
                    discordId = table.Column<long>(type: "INTEGER", nullable: false),
                    name = table.Column<string>(type: "TEXT", nullable: false),
                    tag = table.Column<int>(type: "INTEGER", nullable: false),
                    createdAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_accountDiscords", x => x.accountDiscordId);
                });

            migrationBuilder.CreateTable(
                name: "accountPasswords",
                columns: table => new
                {
                    accountPasswordId = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    accountId = table.Column<long>(type: "INTEGER", nullable: false),
                    hash = table.Column<string>(type: "TEXT", nullable: false),
                    createdAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_accountPasswords", x => x.accountPasswordId);
                });

            migrationBuilder.CreateTable(
                name: "accountRelationships",
                columns: table => new
                {
                    accountRelationshipId = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    accountId = table.Column<long>(type: "INTEGER", nullable: false),
                    otherAccountId = table.Column<long>(type: "INTEGER", nullable: false),
                    relationship = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_accountRelationships", x => x.accountRelationshipId);
                });

            migrationBuilder.CreateTable(
                name: "accounts",
                columns: table => new
                {
                    accountId = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    username = table.Column<string>(type: "TEXT", nullable: false),
                    displayName = table.Column<string>(type: "TEXT", nullable: false),
                    createdAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_accounts", x => x.accountId);
                });

            migrationBuilder.CreateTable(
                name: "accountSessions",
                columns: table => new
                {
                    accountSessionId = table.Column<string>(type: "TEXT", nullable: false),
                    accountId = table.Column<long>(type: "INTEGER", nullable: false),
                    createdAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_accountSessions", x => x.accountSessionId);
                });

            migrationBuilder.CreateTable(
                name: "accountTags",
                columns: table => new
                {
                    accountTagId = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    accountId = table.Column<long>(type: "INTEGER", nullable: false),
                    tag = table.Column<string>(type: "TEXT", nullable: false),
                    displayTag = table.Column<string>(type: "TEXT", nullable: false),
                    createdAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_accountTags", x => x.accountTagId);
                    table.ForeignKey(
                        name: "FK_accountTags_accounts_accountId",
                        column: x => x.accountId,
                        principalTable: "accounts",
                        principalColumn: "accountId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_accountDescriptions_description",
                table: "accountDescriptions",
                column: "description",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_accountPasswords_accountId",
                table: "accountPasswords",
                column: "accountId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_accountRelationships_accountId",
                table: "accountRelationships",
                column: "accountId");

            migrationBuilder.CreateIndex(
                name: "IX_accountTags_accountId",
                table: "accountTags",
                column: "accountId");

            migrationBuilder.CreateIndex(
                name: "IX_accountTags_accountId_tag",
                table: "accountTags",
                columns: new[] { "accountId", "tag" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "accountDescriptions");

            migrationBuilder.DropTable(
                name: "accountDiscords");

            migrationBuilder.DropTable(
                name: "accountPasswords");

            migrationBuilder.DropTable(
                name: "accountRelationships");

            migrationBuilder.DropTable(
                name: "accountSessions");

            migrationBuilder.DropTable(
                name: "accountTags");

            migrationBuilder.DropTable(
                name: "accounts");
        }
    }
}
