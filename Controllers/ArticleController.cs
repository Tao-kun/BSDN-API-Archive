using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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

        // GET api/article?start={start article index}&offset={offset}&sort={sort type id}&tag={tag id}&keyword={keyword}
        // GET api/article?id={user id}&start={start article index}&offset={offset}&sort={sort type id}&tag={tag id}
        [HttpGet]
        public async Task<IActionResult> Get(
            [FromQuery(Name = "id")] int userId,
            [FromQuery(Name = "start")] int start,
            [FromQuery(Name = "offset")] int offset,
            [FromQuery(Name = "sort")] int sort,
            [FromQuery(Name = "tag")] int tagId,
            [FromQuery(Name = "keyword")] string keyword)
        {
            ModelResultList<Article> result;
            // TODO: 分页、排序、分类
            if (offset == 0)
            {
                offset = 20;
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

            var articles = await articleQuery.ToListAsync();
            // TODO: has next
            bool hasNext = false;
            if (articles.Count == 0)
            {
                result = new ModelResultList<Article>(404, articles, "No Article Exists", hasNext);
            }
            else
            {
                result = new ModelResultList<Article>(200, articles, null, hasNext);
            }

            return Ok(result);
        }

        // GET api/article/{article id}
        [HttpGet("id")]
        public async Task<IActionResult> Get(int id)
        {
            ModelResult<Article> result;
            var articleResult = await _context.Articles
                .FirstOrDefaultAsync(a => a.ArticleId == id);
            if (articleResult == null)
            {
                result = new ModelResult<Article>(404, articleResult, "Artcle Not Exists");
                return BadRequest(result);
            }

            result = new ModelResult<Article>(200, articleResult, "Article Exists");
            return Ok(result);
        }

        // POST api/article?token={token}
        [HttpPost]
        public async Task<IActionResult> Post([FromQuery] Article article, [FromQuery(Name = "token")] string token)
        {
            ModelResult<Article> result = TokenUtils.CheckToken<Article>(token, _context);
            if (result != null)
            {
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

            _context.Articles.Add(article);
            await _context.SaveChangesAsync();

            result = new ModelResult<Article>(200, null, "Article Created");
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

            _context.Entry(article).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            result = new ModelResult<Article>(200, null, "Article Modified");
            return Ok(result);
        }

        // DELETE api/article/{id}?token={token}
        [HttpDelete("id")]
        public async Task<IActionResult> Delete(int id, [FromQuery(Name = "token")] string token)
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