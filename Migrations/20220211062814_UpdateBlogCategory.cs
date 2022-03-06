using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace blogProject.Migrations
{
    public partial class UpdateBlogCategory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Content",
                table: "Category",
                newName: "Description");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Category",
                newName: "Content");
        }
    }
}
