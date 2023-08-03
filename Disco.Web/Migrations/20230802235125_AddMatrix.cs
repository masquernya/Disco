using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Disco.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddMatrix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "accountMatrix",
                columns: table => new
                {
                    accountMatrixId = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    accountId = table.Column<long>(type: "INTEGER", nullable: false),
                    name = table.Column<string>(type: "TEXT", nullable: false),
                    domain = table.Column<string>(type: "TEXT", nullable: false),
                    avatarUrl = table.Column<string>(type: "TEXT", nullable: true),
                    createdAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_accountMatrix", x => x.accountMatrixId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "accountMatrix");
        }
    }
}
