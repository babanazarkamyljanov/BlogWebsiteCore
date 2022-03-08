using Microsoft.EntityFrameworkCore.Migrations;

namespace BlogWebsiteCore.Migrations
{
    public partial class AddIsRedirectedColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsRedirected",
                table: "Blogs",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsRedirected",
                table: "Blogs");
        }
    }
}
