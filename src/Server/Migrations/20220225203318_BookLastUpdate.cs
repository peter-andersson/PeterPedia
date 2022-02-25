using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PeterPedia.Server.Migrations
{
    public partial class BookLastUpdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastUpdated",
                table: "book",
                type: "TEXT",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastUpdated",
                table: "book");
        }
    }
}
