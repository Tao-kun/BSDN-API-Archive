using Microsoft.EntityFrameworkCore.Migrations;

namespace BSDN_API.Migrations
{
    public partial class FixSession : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sessions_Users_SessionUserId",
                table: "Sessions");

            migrationBuilder.DropIndex(
                name: "IX_Sessions_SessionUserId",
                table: "Sessions");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Sessions_SessionUserId",
                table: "Sessions",
                column: "SessionUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Sessions_Users_SessionUserId",
                table: "Sessions",
                column: "SessionUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
