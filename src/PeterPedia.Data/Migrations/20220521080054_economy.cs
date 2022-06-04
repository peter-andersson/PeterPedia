using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PeterPedia.Data.Migrations;

public partial class economy : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "category",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                Name = table.Column<string>(type: "TEXT", nullable: false),
                ParentId = table.Column<int>(type: "INTEGER", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_category", x => x.Id);
                table.ForeignKey(
                    name: "FK_category_category_ParentId",
                    column: x => x.ParentId,
                    principalTable: "category",
                    principalColumn: "Id");
            });

        migrationBuilder.CreateTable(
            name: "transaction",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                Note1 = table.Column<string>(type: "TEXT", nullable: false),
                Note2 = table.Column<string>(type: "TEXT", nullable: false),
                Amount = table.Column<double>(type: "REAL", nullable: false),
                CategoryId = table.Column<int>(type: "INTEGER", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_transaction", x => x.Id);
                table.ForeignKey(
                    name: "FK_transaction_category_CategoryId",
                    column: x => x.CategoryId,
                    principalTable: "category",
                    principalColumn: "Id");
            });

        migrationBuilder.CreateIndex(
            name: "IX_category_ParentId",
            table: "category",
            column: "ParentId");

        migrationBuilder.CreateIndex(
            name: "IX_transaction_CategoryId",
            table: "transaction",
            column: "CategoryId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "transaction");

        migrationBuilder.DropTable(
            name: "category");
    }
}
