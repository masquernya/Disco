using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Disco.Web.Migrations
{
    /// <inheritdoc />
    public partial class DescriptionUniqueIdFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_accountDescriptions_description",
                table: "accountDescriptions");

            migrationBuilder.CreateIndex(
                name: "IX_accountDescriptions_accountId",
                table: "accountDescriptions",
                column: "accountId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_accountDescriptions_accountId",
                table: "accountDescriptions");

            migrationBuilder.CreateIndex(
                name: "IX_accountDescriptions_description",
                table: "accountDescriptions",
                column: "description",
                unique: true);
        }
    }
}
