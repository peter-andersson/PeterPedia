using Microsoft.EntityFrameworkCore.Migrations;

namespace PeterPedia.Data.Migrations;

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
