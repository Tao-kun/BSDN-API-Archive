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
    [Route("api/article/{articleId}/tag/{tagId}")]
    [ApiController]
    public class ArticleTagController : ControllerBase
    {
        private readonly BSDNContext _context;

        public ArticleTagController(BSDNContext context)
        {
            _context = context;
        }

        // POST api/article/{article id}/tag/{tag id}?token={token}
        [HttpPost]
        public async Task<IActionResult> Post(
            int articleId,
            int tagId,
            [FromQuery(Name = "token")] string token)
        {
            // 先检查Token、文章、tag是否存在且有效
            // 再检查Token拥有者和文章作者是否一人
            ModelResult<ArticleTag> result = TokenUtils.CheckToken<ArticleTag>(token, _context);

            if (result != null)
            {
                return BadRequest(result);
            }

            Session sessionResult = await _context.Sessions
                .FirstOrDefaultAsync(s => s.SessionToken == token);
            Article articleResult = await _context.Articles
                .FirstOrDefaultAsync(a => a.ArticleId == articleId);
            Tag tagResult = await _context.Tags
                .FirstOrDefaultAsync(t => t.TagId == tagId);
            if (tagResult == null || articleResult == null)
            {
                result = new ModelResult<ArticleTag>(404, null, "Article or Tag Not Exists");
                return BadRequest(result);
            }

            if (sessionResult.SessionUserId != articleResult.UserId)
            {
                result = new ModelResult<ArticleTag>(403, null, "You Cannot Add Tag for Others' Article");
                return BadRequest(result);
            }

            ArticleTag articleTagResult = await _context.ArticleTags
                .FirstOrDefaultAsync(at => at.ArticleId == articleId && at.TagId == tagId);
            if (articleTagResult != null)
            {
                result = new ModelResult<ArticleTag>(405, articleTagResult, "Article Tag Exists");
                return BadRequest(result);
            }


            ArticleTag articleTag = new ArticleTag
            {
                ArticleId = articleResult.ArticleId,
                TagId = tagResult.TagId
            };

            _context.ArticleTags.Add(articleTag);
            _context.SaveChanges();

            result = new ModelResult<ArticleTag>(201, null, "Tag Added");
            return Ok(result);
        }

        // DELETE api/article/{article id}/tag?id={tag id}?token={token}
        [HttpDelete]
        public async Task<IActionResult> Delete(
            int articleId,
            [FromQuery(Name = "id")] int tagId,
            [FromQuery(Name = "token")] string token)
        {
            ModelResult<ArticleTag> result = TokenUtils.CheckToken<ArticleTag>(token, _context);

            if (result != null)
            {
                return BadRequest(result);
            }

            Session sessionResult = await _context.Sessions
                .FirstOrDefaultAsync(s => s.SessionToken == token);
            Tag tagResult = await _context.Tags
                .FirstOrDefaultAsync(t => t.TagId == tagId);
            Article articleResult = await _context.Articles
                .FirstOrDefaultAsync(a => a.ArticleId == articleId);
            if (tagResult == null || articleResult == null)
            {
                result = new ModelResult<ArticleTag>(404, null, "Article or Tag Not Exists");
                return BadRequest(result);
            }

            if (sessionResult.SessionUserId != articleResult.UserId)
            {
                result = new ModelResult<ArticleTag>(403, null, "You Cannot Add Tag for Others' Article");
                return BadRequest(result);
            }

            ArticleTag articleTagResult = await _context.ArticleTags
                .FirstOrDefaultAsync(at => at.ArticleId == articleId && at.TagId == tagId);
            if (articleTagResult == null)
            {
                result = new ModelResult<ArticleTag>(405, null, "Article Tag Not Exists");
                return BadRequest(result);
            }

            _context.ArticleTags.Remove(articleTagResult);
            await _context.SaveChangesAsync();

            result = new ModelResult<ArticleTag>(200, articleTagResult, "Tag Deleted");
            return Ok(result);
        }
    }
}