using Microsoft.EntityFrameworkCore.Migrations;

namespace PeterPedia.Migrations;

public partial class SetNameOfTableMappingTable : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_AuthorEFBookEF_author_AuthorsId",
            table: "AuthorEFBookEF");

        migrationBuilder.DropForeignKey(
            name: "FK_AuthorEFBookEF_book_BooksId",
            table: "AuthorEFBookEF");

        migrationBuilder.DropPrimaryKey(
            name: "PK_AuthorEFBookEF",
            table: "AuthorEFBookEF");

        migrationBuilder.RenameTable(
            name: "AuthorEFBookEF",
            newName: "authorbook");

        migrationBuilder.RenameIndex(
            name: "IX_AuthorEFBookEF_BooksId",
            table: "authorbook",
            newName: "IX_authorbook_BooksId");

        migrationBuilder.AddPrimaryKey(
            name: "PK_authorbook",
            table: "authorbook",
            columns: new[] { "AuthorsId", "BooksId" });

        migrationBuilder.AddForeignKey(
            name: "FK_authorbook_author_AuthorsId",
            table: "authorbook",
            column: "AuthorsId",
            principalTable: "author",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_authorbook_book_BooksId",
            table: "authorbook",
            column: "BooksId",
            principalTable: "book",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_authorbook_author_AuthorsId",
            table: "authorbook");

        migrationBuilder.DropForeignKey(
            name: "FK_authorbook_book_BooksId",
            table: "authorbook");

        migrationBuilder.DropPrimaryKey(
            name: "PK_authorbook",
            table: "authorbook");

        migrationBuilder.RenameTable(
            name: "authorbook",
            newName: "AuthorEFBookEF");

        migrationBuilder.RenameIndex(
            name: "IX_authorbook_BooksId",
            table: "AuthorEFBookEF",
            newName: "IX_AuthorEFBookEF_BooksId");

        migrationBuilder.AddPrimaryKey(
            name: "PK_AuthorEFBookEF",
            table: "AuthorEFBookEF",
            columns: new[] { "AuthorsId", "BooksId" });

        migrationBuilder.AddForeignKey(
            name: "FK_AuthorEFBookEF_author_AuthorsId",
            table: "AuthorEFBookEF",
            column: "AuthorsId",
            principalTable: "author",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_AuthorEFBookEF_book_BooksId",
            table: "AuthorEFBookEF",
            column: "BooksId",
            principalTable: "book",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    }
}
