using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PeterPedia.Migrations
{
    public partial class IgnoreInOverview : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IgnoreInOverView",
                table: "category",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IgnoreInOverView",
                table: "category");
        }
    }
}
