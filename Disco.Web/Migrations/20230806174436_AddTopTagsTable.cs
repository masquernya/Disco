using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Disco.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddTopTagsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "topTags",
                columns: table => new
                {
                    topTagId = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    tag = table.Column<string>(type: "TEXT", nullable: false),
                    displayTag = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_topTags", x => x.topTagId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_topTags_tag",
                table: "topTags",
                column: "tag",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "topTags");
        }
    }
}
