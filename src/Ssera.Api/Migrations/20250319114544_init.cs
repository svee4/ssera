﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ssera.Api.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EventArchive",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: true),
                    Link = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventArchive", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ImageArchive",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FileId = table.Column<string>(type: "TEXT", nullable: false),
                    Member = table.Column<int>(type: "INTEGER", nullable: false),
                    TopLevelKind = table.Column<int>(type: "INTEGER", nullable: false),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Tags = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImageArchive", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkerHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    WorkerName = table.Column<string>(type: "TEXT", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Message = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkerHistory", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventArchive_Date",
                table: "EventArchive",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_EventArchive_Title",
                table: "EventArchive",
                column: "Title");

            migrationBuilder.CreateIndex(
                name: "IX_EventArchive_Type",
                table: "EventArchive",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_WorkerHistory_Timestamp",
                table: "WorkerHistory",
                column: "Timestamp");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventArchive");

            migrationBuilder.DropTable(
                name: "ImageArchive");

            migrationBuilder.DropTable(
                name: "WorkerHistory");
        }
    }
}
