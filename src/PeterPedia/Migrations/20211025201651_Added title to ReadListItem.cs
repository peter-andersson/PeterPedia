using Microsoft.EntityFrameworkCore.Migrations;

namespace PeterPedia.Migrations;

public partial class AddedtitletoReadListItem : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "Title",
            table: "readlist",
            type: "TEXT",
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Title",
            table: "readlist");
    }
}
