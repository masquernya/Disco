using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Disco.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddMatrixSpaces : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "matrixSpaceAdmins",
                columns: table => new
                {
                    matrixSpaceAdminId = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    matrixSpaceId = table.Column<long>(type: "INTEGER", nullable: false),
                    matrixUserId = table.Column<string>(type: "TEXT", nullable: false),
                    createdAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_matrixSpaceAdmins", x => x.matrixSpaceAdminId);
                });

            migrationBuilder.CreateTable(
                name: "matrixSpaces",
                columns: table => new
                {
                    matrixSpaceId = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    name = table.Column<string>(type: "TEXT", nullable: false),
                    description = table.Column<string>(type: "TEXT", nullable: true),
                    memberCount = table.Column<int>(type: "INTEGER", nullable: false),
                    imageId = table.Column<long>(type: "INTEGER", nullable: true),
                    invite = table.Column<string>(type: "TEXT", nullable: false),
                    is18Plus = table.Column<bool>(type: "INTEGER", nullable: false),
                    createdAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_matrixSpaces", x => x.matrixSpaceId);
                });

            migrationBuilder.CreateTable(
                name: "matrixSpaceTags",
                columns: table => new
                {
                    matrixSpaceTagId = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    matrixSpaceId = table.Column<long>(type: "INTEGER", nullable: false),
                    tag = table.Column<string>(type: "TEXT", nullable: false),
                    displayTag = table.Column<string>(type: "TEXT", nullable: false),
                    createdAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_matrixSpaceTags", x => x.matrixSpaceTagId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "matrixSpaceAdmins");

            migrationBuilder.DropTable(
                name: "matrixSpaces");

            migrationBuilder.DropTable(
                name: "matrixSpaceTags");
        }
    }
}
