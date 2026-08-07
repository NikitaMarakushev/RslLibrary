using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RslLibrary.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftDeleteToBooksAndCopies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "copies",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "books",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "copies");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "books");
        }
    }
}
