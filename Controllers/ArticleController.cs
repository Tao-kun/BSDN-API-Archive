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
    public class ArticleController : ControllerBase
    {
        private readonly BSDNContext _context;

        public ArticleController(BSDNContext context)
        {
            _context = context;
        }

        // GET api/article?offset={offset}&limit={limit}&sort={sort type id}&tag={tag id}&keyword={keyword}
        // GET api/article?id={user id}&offset={offset}&limit={limit}&sort={sort type id}&tag={tag id}
        [HttpGet]
        public async Task<IActionResult> Get(
            [FromQuery(Name = "id")] int userId,
            [FromQuery(Name = "offset")] int offset,
            [FromQuery(Name = "limit")] int limit,
            [FromQuery(Name = "sort")] int sort,
            [FromQuery(Name = "tag")] int tagId,
            [FromQuery(Name = "keyword")] string keyword)
        {
            // 排序相关：
            // 0     -> 不排序，直接返回查询结果（默认）
            // 1     -> 按照文章的作者ID升序
            // 2     -> 按照文章的作者ID降序
            // 3     -> 按照文章发表日期升序
            // 4     -> 按照的文章发表日期降序
            // 5     -> 按照标题升序
            // 6     -> 按照标题降序
            // 7     -> 按照浏览量升序
            // 8     -> 按照浏览量降序
            // 9     -> 按照评论量升序
            // 10    -> 按照评论量降序
            // other -> 不排序，直接返回查询结果（同0）

            ModelResultList<ArticleInfo> result;
            if (limit == 0)
            {
                limit = 10;
            }
            else if (limit < 0)
            {
                limit = 0;
            }

            IQueryable<Article> articleQuery;

            if (userId != 0)
            {
                articleQuery = _context.Articles
                    .Where(a => a.UserId == userId);
            }
            else if (keyword != null)
            {
                articleQuery = _context.Articles
                    .Where(a => a.Content.Contains(keyword) ||
                                a.Title.Contains(keyword));
            }
            else
            {
                articleQuery = _context.Articles;
            }

            if (tagId != 0)
            {
                articleQuery = articleQuery
                    .Where(a => a.ArticleTags.Exists(at => at.TagId == tagId));
            }

            List<ArticleInfo> articleInfos = articleQuery
                .Select(a => new ArticleInfo(a, _context)).ToList();
            int totalCount = articleInfos.Count;

            if (totalCount == 0)
            {
                result = new ModelResultList<ArticleInfo>(404, null,
                    "No Article Exists", false, totalCount, null);
                return Ok(result);
            }

            bool hasNext = offset + limit < totalCount;
            string nextUrl;
            if (hasNext)
            {
                nextUrl =
                    $@"/api/article?id={userId}&keyword={keyword}&tag={tagId}&offset={limit + offset}&limit={limit}";
            }
            else
            {
                nextUrl = null;
            }

            articleInfos = articleInfos.Select(ai =>
            {
                ai.CommentCount = _context.Comments.Count(c => c.ArticleId == ai.ArticleId);
                return ai;
            }).ToList();

            switch (sort)
            {
                case 1:
                    articleInfos.Sort((a1, a2) => a1.UserId - a2.UserId);
                    break;
                case 2:
                    articleInfos.Sort((a1, a2) => a2.UserId - a1.UserId);
                    break;
                case 3:
                    articleInfos.Sort((a1, a2) => DateTime.Compare(a1.PublishDate, a2.PublishDate));
                    break;
                case 4:
                    articleInfos.Sort((a1, a2) => DateTime.Compare(a2.PublishDate, a1.PublishDate));
                    break;
                case 5:
                    articleInfos.Sort((a1, a2) => string.CompareOrdinal(a1.Title, a2.Title));
                    break;
                case 6:
                    articleInfos.Sort((a1, a2) => string.CompareOrdinal(a2.Title, a1.Title));
                    break;
                case 7:
                    articleInfos.Sort((a1, a2) => a1.ViewNumber - a2.ViewNumber);
                    break;
                case 8:
                    articleInfos.Sort((a1, a2) => a2.ViewNumber - a1.ViewNumber);
                    break;
                case 9:
                    articleInfos.Sort((a1, a2) => a1.CommentCount - a2.CommentCount);
                    break;
                case 10:
                    articleInfos.Sort((a1, a2) => a2.CommentCount - a1.CommentCount);
                    break;
            }

            if (offset <= totalCount)
            {
                if (offset + limit > totalCount)
                    limit = totalCount - offset;
                articleInfos = articleInfos.GetRange(offset, limit);
            }
            else
            {
                result = new ModelResultList<ArticleInfo>(400, null,
                    "Index Out of Limit", false, totalCount, nextUrl);
                return BadRequest(result);
            }

            if (articleInfos.Count == 0)
            {
                result = new ModelResultList<ArticleInfo>(404, null,
                    "No Article Exists", hasNext, totalCount, nextUrl);
            }
            else
            {
                result = new ModelResultList<ArticleInfo>(200, articleInfos, null, hasNext, totalCount, nextUrl);
            }

            return Ok(result);
        }

        // GET api/article/{article id}?token={token}
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id, [FromQuery(Name = "token")] string token)
        {
            ModelResult<ArticleInfo> result;
            var articleResult = await _context.Articles
                .FirstOrDefaultAsync(a => a.ArticleId == id);
            if (articleResult == null)
            {
                result = new ModelResult<ArticleInfo>(404, null, "Article Not Exists");
                return BadRequest(result);
            }

            if (token != null && TokenUtils.CheckToken<ArticleInfo>(token, _context) == null)
            {
                // Token有效，点击量+1
                articleResult.ViewNumber += 1;
                _context.Entry(articleResult).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }

            articleResult.User = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == articleResult.UserId);
            articleResult.ArticleTags = await _context.ArticleTags
                .Where(at => at.ArticleId == articleResult.ArticleId).ToListAsync();
            ArticleInfo articleInfo = new ArticleInfo(articleResult, _context);
            articleInfo.CommentCount = _context.Comments
                .Count(c => c.ArticleId == articleInfo.ArticleId);
            result = new ModelResult<ArticleInfo>(200, articleInfo, "Article Exists");
            return Ok(result);
        }

        // POST api/article?token={token}
        [HttpPost]
        public async Task<IActionResult> Post(
            [FromBody] Article article,
            [FromQuery(Name = "token")] string token)
        {
            ModelResult<ArticleInfo> result = TokenUtils.CheckToken<ArticleInfo>(token, _context);
            if (result != null)
            {
                return BadRequest(result);
            }

            Session sessionResult = await _context.Sessions
                .FirstOrDefaultAsync(s => s.SessionToken == token);
            article.User = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == sessionResult.SessionUserId);

            if (article.PublishDate == DateTime.MinValue)
            {
                article.PublishDate = DateTime.Now;
            }

            if (article.ArticleId != 0)
            {
                result = new ModelResult<ArticleInfo>(400, null, "Invalid Article");
                return BadRequest(result);
            }

            if (article.Title == null ||
                article.Content == null)
            {
                result = new ModelResult<ArticleInfo>(400, null, "Article Need Title or Content");
                return BadRequest(result);
            }

            _context.Articles.Add(article);
            await _context.SaveChangesAsync();

            result = new ModelResult<ArticleInfo>(201, new ArticleInfo(article, _context), "Article Created");
            return Ok(result);
        }

        // PUT api/article/{id}?token={token}
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(
            int id,
            [FromBody] Article article,
            [FromQuery(Name = "token")] string token)
        {
            ModelResult<Article> result = TokenUtils.CheckToken<Article>(token, _context);
            if (result != null)
            {
                return BadRequest(result);
            }

            Article articleResult = await _context.Articles
                .FirstOrDefaultAsync(a => a.ArticleId == id);
            if (articleResult == null)
            {
                result = new ModelResult<Article>(405, null, "Article Not Exists");
                return BadRequest(result);
            }

            User userResult = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == articleResult.UserId);
            Session sessionResult = await _context.Sessions
                .FirstOrDefaultAsync(s => s.SessionToken == token);
            if (userResult == null ||
                userResult.UserId != sessionResult.SessionUserId)
            {
                result = new ModelResult<Article>(405, null, "User Not Exists or Token not suit");
                return BadRequest(result);
            }

            articleResult.Title = article.Title;
            articleResult.Content = article.Content;

            _context.Entry(articleResult).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            result = new ModelResult<Article>(200, null, "Article Modified");
            return Ok(result);
        }

        // DELETE api/article?id={article id}?token={token}
        [HttpDelete]
        public async Task<IActionResult> Delete(
            [FromQuery(Name = "id")] int id,
            [FromQuery(Name = "token")] string token)
        {
            ModelResult<Article> result = TokenUtils.CheckToken<Article>(token, _context);
            if (result != null)
            {
                return BadRequest(result);
            }

            Article articleResult = await _context.Articles
                .FirstOrDefaultAsync(a => a.ArticleId == id);
            if (articleResult == null)
            {
                result = new ModelResult<Article>(405, null, "Article Not Exists");
                return BadRequest(result);
            }

            User userResult = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == articleResult.UserId);
            Session sessionResult = await _context.Sessions
                .FirstOrDefaultAsync(s => s.SessionToken == token);
            if (userResult == null ||
                userResult.UserId != sessionResult.SessionUserId)
            {
                result = new ModelResult<Article>(405, null, "User Not Exists or Token not suit");
                return BadRequest(result);
            }

            _context.Articles.Remove(articleResult);
            await _context.SaveChangesAsync();

            result = new ModelResult<Article>(200, articleResult, "Article Deleted");
            return Ok(result);
        }
    }
}