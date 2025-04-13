using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ssera.Api.Migrations
{
    /// <inheritdoc />
    public partial class Addindexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ImageArchiveTag_Tag",
                table: "ImageArchiveTag",
                column: "Tag");

            migrationBuilder.CreateIndex(
                name: "IX_ImageArchive_Date",
                table: "ImageArchive",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_ImageArchive_Member",
                table: "ImageArchive",
                column: "Member");

            migrationBuilder.CreateIndex(
                name: "IX_ImageArchive_TopLevelKind",
                table: "ImageArchive",
                column: "TopLevelKind");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ImageArchiveTag_Tag",
                table: "ImageArchiveTag");

            migrationBuilder.DropIndex(
                name: "IX_ImageArchive_Date",
                table: "ImageArchive");

            migrationBuilder.DropIndex(
                name: "IX_ImageArchive_Member",
                table: "ImageArchive");

            migrationBuilder.DropIndex(
                name: "IX_ImageArchive_TopLevelKind",
                table: "ImageArchive");
        }
    }
}
