using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BSDN_API.Models;
using BSDN_API.Utils;

namespace BSDN_API.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class CommentController : ControllerBase
    {
        private readonly BSDNContext _context;

        public CommentController(BSDNContext context)
        {
            _context = context;
        }

        // GET api/comment?id={article id}&offset={offset}&limit={limit}
        [HttpGet]
        public async Task<IActionResult> Get(
            [FromQuery(Name = "id")] int id,
            [FromQuery(Name = "offset")] int offset,
            [FromQuery(Name = "limit")] int limit
        )
        {
            ModelResultList<CommentInfo> result;
            if (limit == 0)
            {
                limit = 10;
            }
            else if (limit < 0)
            {
                limit = 0;
            }

            if (id == 0)
            {
                result = new ModelResultList<CommentInfo>(400, null,
                    "No Article Id", false, 0, null);
                return BadRequest(result);
            }

            List<CommentInfo> commentInfos = _context.Comments
                .Where(c => c.ArticleId == id)
                .Select(c => new CommentInfo(c)).ToList()
                .Select(ci =>
                {
                    ci.ArticleCommentCount = _context.Comments.Count(c => c.ArticleId == ci.ArticleId);
                    return ci;
                }).ToList();
            int totalCount = commentInfos.Count;
            bool hasNext = offset + limit < totalCount;

            var nextUrl = hasNext
                ? $@"/api/comment?id={id}&limit={limit}&offset={limit + offset}"
                : null;

            if (offset <= totalCount)
            {
                if (offset + limit > totalCount)
                    limit = totalCount - offset;
                commentInfos = commentInfos.GetRange(offset, limit);
            }
            else
            {
                result = new ModelResultList<CommentInfo>(400, null,
                    "Index Out of Range", hasNext, totalCount, nextUrl);
                return BadRequest(result);
            }

            if (commentInfos.Count == 0)
            {
                result = new ModelResultList<CommentInfo>(404, null,
                    "No Comment Exists", hasNext, totalCount, nextUrl);
            }
            else
            {
                commentInfos = commentInfos.Select(ci =>
                {
                    CommentReply commentReply = _context.CommentReplies
                        .FirstOrDefault(cr => cr.CommentId == ci.CommentId);
                    if (commentReply != null)
                    {
                        ci.IsReply = true;
                        ci.RepliedCommentId = commentReply.RepliedCommentId;
                    }
                    else
                    {
                        ci.IsReply = false;
                        ci.RepliedCommentId = 0;
                    }

                    return ci;
                }).ToList();
                result = new ModelResultList<CommentInfo>(200, commentInfos,
                    null, hasNext, totalCount, nextUrl);
            }

            return Ok(result);
        }

        // GET api/comment/{comment id}
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            ModelResult<CommentInfo> result;
            Comment commentResult = await _context.Comments
                .FirstOrDefaultAsync(c => c.CommentId == id);
            if (commentResult == null)
            {
                result = new ModelResult<CommentInfo>(404, null, "Comment Not Exists");
                return BadRequest(result);
            }

            CommentInfo commentInfo = new CommentInfo(commentResult);
            CommentReply commentReply = await _context.CommentReplies
                .FirstOrDefaultAsync(cr => cr.CommentId == commentInfo.CommentId);
            if (commentReply != null)
            {
                commentInfo.IsReply = true;
                commentInfo.RepliedCommentId = commentReply.RepliedCommentId;
            }
            else
            {
                commentInfo.IsReply = false;
                commentInfo.RepliedCommentId = 0;
            }

            result = new ModelResult<CommentInfo>(400, commentInfo, null);
            return Ok(result);
        }

        // POST api/comment/article/{article id}?token={token}
        // POST api/comment/reply/{comment id}?token={token}
        [HttpPost("{type}/{id}")]
        public async Task<IActionResult> Post(
            string type,
            int id,
            [FromBody] Comment comment,
            [FromQuery(Name = "token")] string token)
        {
            // 先评论是否有正文
            // 再检查Token是否有效
            // 再检查评论类型
            // 再检查文章/评论是否存在
            int replyCommentId = 0;
            Comment commentResult = null;

            ModelResult<CommentInfo> result = TokenUtils.CheckToken<CommentInfo>(token, _context);
            if (result != null)
            {
                return BadRequest(result);
            }

            if (comment.Content == null || comment.CommentId != 0)
            {
                result = new ModelResult<CommentInfo>(400, new CommentInfo(comment), "Invalid Comment");
                return BadRequest(result);
            }

            comment.PublishDate = DateTime.Now;

            if (type == "article" || type == "reply")
            {
                Article articleResult;
                if (type == "article")
                {
                    if (comment.ArticleId == 0)
                        comment.ArticleId = id;
                    if (id == 0)
                        id = comment.ArticleId;

                    articleResult = await _context.Articles
                        .FirstOrDefaultAsync(a => a.ArticleId == id);
                }
                else // if (type == "reply")
                {
                    commentResult = await _context.Comments
                        .FirstOrDefaultAsync(c => c.CommentId == id);
                    if (commentResult == null)
                    {
                        result = new ModelResult<CommentInfo>(404, null, "Comment to Reply Not Exists");
                        return BadRequest(result);
                    }

                    replyCommentId = commentResult.CommentId;
                    articleResult = await _context.Articles
                        .FirstOrDefaultAsync(a => a.ArticleId == commentResult.ArticleId);

                    comment.ArticleId = articleResult.ArticleId;
                }

                Session sessionResult = await _context.Sessions
                    .FirstOrDefaultAsync(s => s.SessionToken == token);

                if (articleResult == null)
                {
                    result = new ModelResult<CommentInfo>(404, null, "Article Not Exists");
                    return BadRequest(result);
                }

                comment.UserId = sessionResult.SessionUserId;
                comment.User = await _context.Users
                    .FirstOrDefaultAsync(u => u.UserId == comment.UserId);

                comment.ArticleId = id;
                comment.Article = articleResult;

                await _context.AddAsync(comment);
                await _context.SaveChangesAsync();

                result = new ModelResult<CommentInfo>(201, new CommentInfo(comment), "Commented");
            }
            else
            {
                result = new ModelResult<CommentInfo>(405, null, "Undefined Comment Type");
                return BadRequest(result);
            }

            if (type == "reply" && replyCommentId != 0)
            {
                await _context.AddAsync(new CommentReply
                {
                    CommentId = comment.CommentId,
                    RepliedCommentId = replyCommentId
                });
                await _context.SaveChangesAsync();
                result.Message = "Replied";
            }

            int noticeUserId = 0;
            if (type == "article")
            {
                noticeUserId = comment.UserId;
            }
            else
            {
                if (commentResult != null)
                    noticeUserId = commentResult.UserId;
            }

            await NoticeUtils.CreateCommentNotice(comment, _context, noticeUserId, type);

            return Ok(result);
        }

        // DELETE api/comment?id={comment id}&token={token}
        public async Task<IActionResult> Delete(
            [FromQuery(Name = "id")] int id,
            [FromQuery(Name = "token")] string token)
        {
            ModelResult<CommentInfo> result = TokenUtils.CheckToken<CommentInfo>(token, _context);
            if (result != null)
            {
                return BadRequest(result);
            }

            Session sessionResult = await _context.Sessions
                .FirstOrDefaultAsync(s => s.SessionToken == token);
            Comment commentResult = await _context.Comments
                .FirstOrDefaultAsync(c => c.CommentId == id);

            if (commentResult == null)
            {
                result = new ModelResult<CommentInfo>(404, null, "Comment Not Exists");
                return BadRequest(result);
            }

            if (commentResult.UserId == sessionResult.SessionUserId)
            {
                _context.Remove(commentResult);
                await _context.SaveChangesAsync();

                result = new ModelResult<CommentInfo>(200, new CommentInfo(commentResult), "Comment Deleted");
                return Ok(result);
            }

            result = new ModelResult<CommentInfo>(405, null, "Cannot Delete Others' Comment");
            return BadRequest(result);
        }
    }
}