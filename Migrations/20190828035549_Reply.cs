using Microsoft.EntityFrameworkCore.Migrations;

namespace BSDN_API.Migrations
{
    public partial class Reply : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CommentReplies",
                columns: table => new
                {
                    CommentId = table.Column<int>(nullable: false),
                    RepliedCommentId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommentReplies", x => new { x.CommentId, x.RepliedCommentId });
                    table.ForeignKey(
                        name: "FK_CommentReplies_Comments_CommentId",
                        column: x => x.CommentId,
                        principalTable: "Comments",
                        principalColumn: "CommentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CommentReplies_Comments_RepliedCommentId",
                        column: x => x.RepliedCommentId,
                        principalTable: "Comments",
                        principalColumn: "CommentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CommentReplies_CommentId",
                table: "CommentReplies",
                column: "CommentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CommentReplies_RepliedCommentId",
                table: "CommentReplies",
                column: "RepliedCommentId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CommentReplies");
        }
    }
}
