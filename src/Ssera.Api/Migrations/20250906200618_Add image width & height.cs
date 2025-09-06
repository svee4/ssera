using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ssera.Api.Migrations
{
    /// <inheritdoc />
    public partial class Addimagewidthheight : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Height",
                table: "ImageArchive",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Width",
                table: "ImageArchive",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Height",
                table: "ImageArchive");

            migrationBuilder.DropColumn(
                name: "Width",
                table: "ImageArchive");
        }
    }
}
