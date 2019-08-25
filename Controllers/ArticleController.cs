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
            ModelResultList<ArticleInfo> result;
            // TODO: 排序、分类
            if (limit == 0)
            {
                limit = 20;
            }
            else if (limit < 0)
            {
                limit = 0;
            }

            IQueryable<Article> articleQuery;

            if (userId != 0)
            {
                articleQuery = _context.Articles
                    .Where(a => a.User.UserId == userId);
            }
            else if (keyword != null)
            {
                // TODO: 搜索、分词
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

            List<ArticleInfo> articleInfos = articleQuery.ToList()
                .Select(a =>
                {
                    a.User = _context.Users.FirstOrDefault(u => u.UserId == a.UserId);
                    return a;
                })
                .Select(a => new ArticleInfo(a, _context)).ToList();
            int totalCount = articleInfos.Count;
            bool hasNext = offset + limit < totalCount;

            if (offset <= totalCount)
            {
                if (offset + limit > totalCount)
                    limit = totalCount - offset;
                articleInfos = articleInfos.GetRange(offset, limit);
            }
            else
            {
                result = new ModelResultList<ArticleInfo>(400, null, "Index Out of Limit", false, totalCount);
                return BadRequest(result);
            }

            if (articleInfos.Count == 0)
            {
                result = new ModelResultList<ArticleInfo>(404, null, "No Article Exists", hasNext, totalCount);
            }
            else
            {
                articleInfos = articleInfos
                    .Select(ai =>
                    {
                        ai.TagInfos = _context.ArticleTags
                            .Where(at => at.ArticleId == ai.ArticleId)
                            .Select(at => new TagInfo(at.Tag)).ToList();
                        return ai;
                    }).ToList();
                result = new ModelResultList<ArticleInfo>(200, articleInfos, null, hasNext, totalCount);
            }

            return Ok(result);
        }

        // GET api/article/{article id}
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            ModelResult<Article> result;
            var articleResult = await _context.Articles
                .FirstOrDefaultAsync(a => a.ArticleId == id);
            if (articleResult == null)
            {
                result = new ModelResult<Article>(404, articleResult, "Article Not Exists");
                return BadRequest(result);
            }

            result = new ModelResult<Article>(200, articleResult, "Article Exists");
            return Ok(result);
        }

        // POST api/article?token={token}
        [HttpPost]
        public async Task<IActionResult> Post(
            [FromBody] Article article,
            [FromQuery(Name = "token")] string token)
        {
            ModelResult<Article> result = TokenUtils.CheckToken<Article>(token, _context);
            if (result != null)
            {
                return BadRequest(result);
            }

            Session sessionResult = await _context.Sessions
                .FirstOrDefaultAsync(s => s.SessionToken == token);
            article.User = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == sessionResult.SessionUserId);
            article.UserId = article.User.UserId;

            if (article.PublishDate == DateTime.MinValue)
            {
                article.PublishDate = DateTime.Now;
            }

            if (article.ArticleId != 0)
            {
                result = new ModelResult<Article>(400, null, "Invalid Article");
                return BadRequest(result);
            }

            if (article.Title == null ||
                article.Content == null)
            {
                result = new ModelResult<Article>(400, null, "Article Need Title or Content");
                return BadRequest(result);
            }

            _context.Articles.Add(article);
            await _context.SaveChangesAsync();

            result = new ModelResult<Article>(201, null, "Article Created");
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
                .FirstOrDefaultAsync(u => u.UserId == article.User.UserId);
            Session sessionResult = await _context.Sessions
                .FirstOrDefaultAsync(s => s.SessionToken == token);
            if (userResult == null ||
                userResult.UserId != sessionResult.SessionUserId)
            {
                result = new ModelResult<Article>(405, null, "User Not Exists or Token not suit");
                return BadRequest(result);
            }

            if (id != article.ArticleId)
            {
                result = new ModelResult<Article>(405, null, "Cannot Modify ArticleId");
                return BadRequest(result);
            }

            if (article.PublishDate == DateTime.MinValue)
            {
                article.PublishDate = articleResult.PublishDate;
            }

            if (article.ViewNumber == 0)
            {
                article.ViewNumber = articleResult.ViewNumber;
            }

            if (article.Comments == null || article.Comments.Count == 0)
            {
                article.Comments = articleResult.Comments;
            }

            if (article.ArticleTags == null || article.ArticleTags.Count == 0)
            {
                article.ArticleTags = articleResult.ArticleTags;
            }

            if (article.ResourceFiles == null || article.ResourceFiles.Count == 0)
            {
                article.ResourceFiles = articleResult.ResourceFiles;
            }

            _context.Entry(article).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            result = new ModelResult<Article>(200, null, "Article Modified");
            return Ok(result);
        }

        // DELETE api/article/{id}?token={token}
        [HttpDelete("id")]
        public async Task<IActionResult> Delete(
            int id,
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
                .FirstOrDefaultAsync(u => u.UserId == articleResult.User.UserId);
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