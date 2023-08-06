using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Disco.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddUserUploadedImages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "url",
                table: "accountAvatars",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AddColumn<long>(
                name: "userUploadedImageId",
                table: "accountAvatars",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "images",
                columns: table => new
                {
                    userUploadedImageId = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    accountId = table.Column<long>(type: "INTEGER", nullable: false),
                    sha256Hash = table.Column<string>(type: "TEXT", nullable: false),
                    originalSha256Hash = table.Column<string>(type: "TEXT", nullable: false),
                    status = table.Column<int>(type: "INTEGER", nullable: false),
                    format = table.Column<int>(type: "INTEGER", nullable: false),
                    sizeX = table.Column<int>(type: "INTEGER", nullable: false),
                    sizeY = table.Column<int>(type: "INTEGER", nullable: false),
                    fileSize = table.Column<long>(type: "INTEGER", nullable: false),
                    createdAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_images", x => x.userUploadedImageId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_images_sha256Hash",
                table: "images",
                column: "sha256Hash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "images");

            migrationBuilder.DropColumn(
                name: "userUploadedImageId",
                table: "accountAvatars");

            migrationBuilder.AlterColumn<string>(
                name: "url",
                table: "accountAvatars",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);
        }
    }
}
