using Microsoft.EntityFrameworkCore.Migrations;

namespace BSDN_API.Migrations
{
    public partial class ModifyNotice : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ArticleId",
                table: "Notices");

            migrationBuilder.AddColumn<string>(
                name: "ApiUrl",
                table: "Notices",
                maxLength: 512,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApiUrl",
                table: "Notices");

            migrationBuilder.AddColumn<int>(
                name: "ArticleId",
                table: "Notices",
                nullable: false,
                defaultValue: 0);
        }
    }
}
