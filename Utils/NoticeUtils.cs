using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BSDN_API.Models;
using Microsoft.EntityFrameworkCore;

namespace BSDN_API.Utils
{
    public static class NoticeUtils
    {
        public static async Task CreateArticleNotice(Article article, BSDNContext context)
        {
            User userResult = await context.Users.FirstOrDefaultAsync(u => u.UserId == article.UserId);
            List<Notice> notices = await context.UserFollows
                .Where(uf => uf.FollowingId == userResult.UserId)
                .Select(uf => uf.FollowerId)
                .Select(uid => new Notice(uid, $@"New Article|{article.Title}|{article.ArticleId}",
                    $@"/api/article/{article.ArticleId}"))
                .ToListAsync();
            foreach (var notice in notices)
            {
                await context.Notices.AddAsync(notice);
            }

            await context.SaveChangesAsync();
        }

        public static async Task CreateFollowNotice(UserFollow userFollow, BSDNContext context)
        {
            User follower = await context.Users.FirstOrDefaultAsync(u => u.UserId == userFollow.FollowerId);
            User following = await context.Users.FirstOrDefaultAsync(u => u.UserId == userFollow.FollowingId);
            Notice notice = new Notice(following.UserId, $@"New Follower|{follower.Nickname}|{follower.UserId}", null);
            await context.AddAsync(notice);
            await context.SaveChangesAsync();
        }

        public static async Task CreateCommentNotice(Comment comment, BSDNContext context, int userId, string type)
        {
            if (userId == 0)
                return;
            Notice notice = new Notice(userId, $@"New {type}", $@"/api/article/{comment.ArticleId}");
            await context.Notices.AddAsync(notice);
            await context.SaveChangesAsync();
        }
    }
}