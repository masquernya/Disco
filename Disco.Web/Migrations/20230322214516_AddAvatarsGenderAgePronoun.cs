using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Disco.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddAvatarsGenderAgePronoun : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_accountTags_accounts_accountId",
                table: "accountTags");

            migrationBuilder.AddColumn<int>(
                name: "age",
                table: "accounts",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "gender",
                table: "accounts",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "pronouns",
                table: "accounts",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "avatarUrl",
                table: "accountDiscords",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "accountAvatars",
                columns: table => new
                {
                    accountAvatarId = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    accountId = table.Column<long>(type: "INTEGER", nullable: false),
                    url = table.Column<string>(type: "TEXT", nullable: false),
                    source = table.Column<int>(type: "INTEGER", nullable: false),
                    createdAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_accountAvatars", x => x.accountAvatarId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_accountAvatars_accountId",
                table: "accountAvatars",
                column: "accountId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "accountAvatars");

            migrationBuilder.DropColumn(
                name: "age",
                table: "accounts");

            migrationBuilder.DropColumn(
                name: "gender",
                table: "accounts");

            migrationBuilder.DropColumn(
                name: "pronouns",
                table: "accounts");

            migrationBuilder.DropColumn(
                name: "avatarUrl",
                table: "accountDiscords");

            migrationBuilder.AddForeignKey(
                name: "FK_accountTags_accounts_accountId",
                table: "accountTags",
                column: "accountId",
                principalTable: "accounts",
                principalColumn: "accountId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
