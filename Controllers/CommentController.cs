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
                .Select(c => new CommentInfo(c)).ToList()
                .Select(ci =>
                {
                    ci.ArticleCommentCount = _context.Comments.Count(c => c.ArticleId == ci.ArticleId);
                    return ci;
                }).ToList();
            int totalCount = commentInfos.Count;
            bool hasNext = offset + limit < totalCount;

            string nextUrl;
            if (hasNext)
            {
                // TODO: impl it
                nextUrl = $@"/api/comment?id={id}&limit={limit}&offset={limit + offset}";
            }
            else
            {
                nextUrl = null;
            }

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
                commentInfos = commentInfos.ToList();
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

            result = new ModelResult<CommentInfo>(400, new CommentInfo(commentResult), null);
            return Ok(result);
        }

        // POST api/comment/article/{article id}?token={token}
        // TODO: POST api/comment/reply/{comment id}?token={token}
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

            if (type == "reply")
            {
                // TODO: impl it
                return BadRequest("UnImplemented");
            }
            else if (type == "article")
            {
                Session sessionResult = await _context.Sessions
                    .FirstOrDefaultAsync(s => s.SessionToken == token);
                Article articleResult = await _context.Articles
                    .FirstOrDefaultAsync(a => a.ArticleId == id);
                if (articleResult == null)
                {
                    result = new ModelResult<CommentInfo>(404, null, "Article Not Exists");
                    return BadRequest(result);
                }

                comment.UserId = sessionResult.SessionUserId;
                comment.User = await _context.Users
                    .FirstOrDefaultAsync(u => u.UserId == comment.UserId);
                if (id == 0)
                    comment.ArticleId = id;
                comment.Article = articleResult;

                await _context.AddAsync(comment);
                await _context.SaveChangesAsync();
                result = new ModelResult<CommentInfo>(201, new CommentInfo(comment), "Commented");
                return Ok(result);
            }
            else
            {
                result = new ModelResult<CommentInfo>(405, null, "Undefined Comment Type");
                return BadRequest(result);
            }
        }

        // DELETE api/comment/{comment id}?token={token}
        public async Task<IActionResult> Delete(
            int id,
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