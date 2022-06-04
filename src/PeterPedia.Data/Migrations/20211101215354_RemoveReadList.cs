using Microsoft.EntityFrameworkCore.Migrations;

namespace PeterPedia.Data.Migrations;

public partial class RemoveReadList : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "readlist");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "readlist",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                Added = table.Column<DateTime>(type: "TEXT", nullable: false),
                Title = table.Column<string>(type: "TEXT", nullable: true),
                Url = table.Column<string>(type: "TEXT", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_readlist", x => x.Id));
    }
}
