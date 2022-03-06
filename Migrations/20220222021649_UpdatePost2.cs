using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace blogProject.Migrations
{
    public partial class UpdatePost2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "postImg",
                table: "Post");

            migrationBuilder.AddColumn<string>(
                name: "ImageFile",
                table: "Post",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageFile",
                table: "Post");

            migrationBuilder.AddColumn<string>(
                name: "postImg",
                table: "Post",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
