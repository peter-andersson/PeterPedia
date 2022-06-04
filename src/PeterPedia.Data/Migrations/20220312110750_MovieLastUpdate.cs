using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PeterPedia.Data.Migrations;

public partial class MovieLastUpdate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "ETag",
            table: "movie",
            type: "TEXT",
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<DateTime>(
            name: "LastUpdate",
            table: "movie",
            type: "TEXT",
            nullable: false,
            defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "ETag",
            table: "movie");

        migrationBuilder.DropColumn(
            name: "LastUpdate",
            table: "movie");
    }
}
