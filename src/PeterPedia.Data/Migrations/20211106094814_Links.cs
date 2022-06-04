using Microsoft.EntityFrameworkCore.Migrations;

namespace PeterPedia.Data.Migrations;

public partial class Links : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "link",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                Title = table.Column<string>(type: "TEXT", nullable: false),
                Url = table.Column<string>(type: "TEXT", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_link", x => x.Id));
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "link");
    }
}
