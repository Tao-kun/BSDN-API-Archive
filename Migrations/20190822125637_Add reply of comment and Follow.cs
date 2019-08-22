using Microsoft.EntityFrameworkCore.Migrations;

namespace BSDN_API.Migrations
{
    public partial class AddreplyofcommentandFollow : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ReplyCommentCommentId",
                table: "Comments",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UserFollow",
                columns: table => new
                {
                    FollowerId = table.Column<int>(nullable: false),
                    FollowingId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserFollow", x => new { x.FollowerId, x.FollowingId });
                    table.ForeignKey(
                        name: "FK_UserFollow_Users_FollowerId",
                        column: x => x.FollowerId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserFollow_Users_FollowingId",
                        column: x => x.FollowingId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Comments_ReplyCommentCommentId",
                table: "Comments",
                column: "ReplyCommentCommentId");

            migrationBuilder.CreateIndex(
                name: "IX_UserFollow_FollowingId",
                table: "UserFollow",
                column: "FollowingId");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Comments_ReplyCommentCommentId",
                table: "Comments",
                column: "ReplyCommentCommentId",
                principalTable: "Comments",
                principalColumn: "CommentId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Comments_ReplyCommentCommentId",
                table: "Comments");

            migrationBuilder.DropTable(
                name: "UserFollow");

            migrationBuilder.DropIndex(
                name: "IX_Comments_ReplyCommentCommentId",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "ReplyCommentCommentId",
                table: "Comments");
        }
    }
}
