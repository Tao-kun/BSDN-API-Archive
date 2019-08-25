using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BSDN_API.Models;
using BSDN_API.Utils;
using Microsoft.AspNetCore.Razor.Language;

namespace BSDN_API.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class TagController : ControllerBase
    {
        private readonly BSDNContext _context;

        public TagController(BSDNContext context)
        {
            _context = context;
        }

        // GET api/tag
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            ModelResultList<TagInfo> result;
            List<TagInfo> tagResult = _context.Tags.ToList()
                .Select(t =>
                {
                    t.ArticleTags = _context.ArticleTags
                        .Where(at => at.TagId == t.TagId).ToList();
                    return t;
                })
                .Select(t => new TagInfo(t)).ToList();
            if (tagResult.Count == 0)
            {
                result = new ModelResultList<TagInfo>(404, null, "No Tag Exists", false, 0);
            }
            else
            {
                result = new ModelResultList<TagInfo>(200, tagResult, null, false, tagResult.Count);
            }

            return Ok(result);
        }

        // TODO: GET api/tag/{tag id}
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            ModelResult<TagInfo> result;
            Tag tagResult = await _context.Tags.FirstOrDefaultAsync(t => t.TagId == id);
            if (tagResult == null)
            {
                result = new ModelResult<TagInfo>(404, null, "Tag Not Exists");
                return BadRequest(result);
            }
            else
            {
                tagResult.ArticleTags = _context.ArticleTags.Where(at => at.TagId == tagResult.TagId).ToList();
                result = new ModelResult<TagInfo>(200, new TagInfo(tagResult), null);
                return Ok(result);
            }
        }

        // POST api/tag?token={token}
        [HttpPost]
        public async Task<IActionResult> Post(
            [FromBody] Tag tag, 
            [FromQuery(Name = "token")] string token)
        {
            ModelResult<Tag> result = TokenUtils.CheckToken<Tag>(token, _context);
            if (result != null)
            {
                return BadRequest(result);
            }

            Tag tagResult = await _context.Tags
                .FirstOrDefaultAsync(t => t.TagName == tag.TagName);
            if (tagResult != null)
            {
                result = new ModelResult<Tag>(409, tagResult, "Tag Exists");
                return BadRequest(result);
            }

            await _context.Tags.AddAsync(tag);
            await _context.SaveChangesAsync();

            result = new ModelResult<Tag>(201, tag, "Tag Created");
            return Ok(result);
        }

        // PUT api/tag/{tag id}?token={token}
        [HttpPut]
        public async Task<IActionResult> Put(
            int id,
            [FromBody] Tag tag,
            [FromQuery(Name = "token")] string token)
        {
            ModelResult<Tag> result = TokenUtils.CheckToken<Tag>(token, _context);
            if (result != null)
            {
                return BadRequest(result);
            }

            Tag tagResult = await _context.Tags.FirstOrDefaultAsync(t => t.TagId == id);
            if (tagResult == null)
            {
                result = new ModelResult<Tag>(405, null, "Tag Not Found");
                return BadRequest(result);
            }

            if (id != tag.TagId)
            {
                result = new ModelResult<Tag>(405, null, "Cannot Modify TagId");
                return BadRequest(result);
            }

            _context.Entry(tag).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            result = new ModelResult<Tag>(200, null, "Tag Modified");
            return Ok(result);
        }

        // DELETE api/tag/{tag id}?token={token}
        [HttpDelete("id")]
        public async Task<IActionResult> Delete(
            int id, 
            [FromQuery(Name = "token")] string token)
        {
            ModelResult<Tag> result = TokenUtils.CheckToken<Tag>(token, _context);
            if (result != null)
            {
                return BadRequest(result);
            }

            Tag tagResult = await _context.Tags.FirstOrDefaultAsync(t => t.TagId == id);
            if (tagResult == null)
            {
                result = new ModelResult<Tag>(405, null, "Tag Not Found");
                return BadRequest(result);
            }

            _context.Tags.Remove(tagResult);
            await _context.SaveChangesAsync();

            result = new ModelResult<Tag>(200, tagResult, "Tag Eleted");
            return Ok(result);
        }
    }
}