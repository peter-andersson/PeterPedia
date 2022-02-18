using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PeterPedia.Server.Migrations
{
    public partial class SubscriptionGroup : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Group",
                table: "subscription",
                type: "TEXT",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Group",
                table: "subscription");
        }
    }
}
