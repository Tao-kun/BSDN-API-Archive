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
    public class NoticeController : ControllerBase
    {
        private readonly BSDNContext _context;

        public NoticeController(BSDNContext context)
        {
            _context = context;
        }

        // GET api/notice?token={token}&offset={offset}&limit={limit}
        // 按照时间排序
        [HttpGet]
        public async Task<IActionResult> Get(
            [FromQuery(Name = "token")] string token,
            [FromQuery(Name = "offset")] int offset,
            [FromQuery(Name = "limit")] int limit)
        {
            ModelResultList<Notice> result = null;

            if (limit == 0)
            {
                limit = 10;
            }
            else if (limit < 0)
            {
                limit = 0;
            }

            if (token == null)
            {
                result = new ModelResultList<Notice>(405, null, "Need Token", false, 0, null);
                return BadRequest(result);
            }

            Session sessionResult = _context.Sessions
                .FirstOrDefault(s => s.SessionToken == token);
            if (sessionResult == null)
            {
                result = new ModelResultList<Notice>(405, null, "Token not Exists", false, 0, null);
                return BadRequest(result);
            }

            if (sessionResult.ExpiresTime < DateTime.Now)
            {
                result = new ModelResultList<Notice>(405, null, "Token Expires", false, 0, null);
                return BadRequest(result);
            }

            User userResult = await _context.Users.FirstOrDefaultAsync(u => u.UserId == sessionResult.SessionUserId);
            if (userResult == null)
            {
                result = new ModelResultList<Notice>(405, null, "User not Exists", false, 0, null);
                return BadRequest(result);
            }

            List<Notice> notices = await _context.Notices.Where(n => n.UserId == userResult.UserId).ToListAsync();
            int totalCount = notices.Count;
            bool hasNext = offset + limit < totalCount;

            string nextUrl = hasNext
                ? $@"/api/notice?token={token}&offset={offset}&limit={limit}"
                : null;


            notices.Sort((n1, n2) => DateTime.Compare(n1.AddTime, n2.AddTime));
            if (offset <= totalCount)
            {
                if (offset + limit > totalCount)
                    limit = totalCount - offset;
                notices = notices.GetRange(offset, limit);
            }
            else
            {
                result = new ModelResultList<Notice>(400, null, "Index Out of Range", hasNext, totalCount, nextUrl);
                return BadRequest(result);
            }

            if (notices.Count == 0)
            {
                result = new ModelResultList<Notice>(404, null, "No Notice Exists", hasNext, totalCount, nextUrl);
            }
            else
            {
                result = new ModelResultList<Notice>(200, notices, null, hasNext, totalCount, nextUrl);
            }

            return Ok(result);
        }

        // GET api/notice/{id}?token=token
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(
            int id,
            [FromQuery(Name = "token")] string token)
        {
            ModelResult<Notice> result = TokenUtils.CheckToken<Notice>(token, _context);
            if (result != null)
            {
                return BadRequest(result);
            }

            Session sessionResult = await _context.Sessions.FirstOrDefaultAsync(s => s.SessionToken == token);
            User userResult = await _context.Users.FirstOrDefaultAsync(u => u.UserId == sessionResult.SessionUserId);
            Notice noticeResult = await _context.Notices.FirstOrDefaultAsync(n => n.UserId == id);

            if (userResult == null)
            {
                result = new ModelResult<Notice>(404, null, "User Not Exists");
                return BadRequest(result);
            }

            if (userResult.UserId != noticeResult.UserId)
            {
                result = new ModelResult<Notice>(405, null, "Cannnot See Others' Notice");
                return BadRequest(result);
            }

            result = new ModelResult<Notice>(200, noticeResult, null);
            return Ok(result);
        }

        // DELETE api/notice?id={notice id}&token={token}
        [HttpDelete]
        public async Task<IActionResult> Delete(
            [FromQuery(Name = "id")] int id,
            [FromQuery(Name = "token")] string token)
        {
            ModelResult<Notice> result = TokenUtils.CheckToken<Notice>(token, _context);
            if (result != null)
            {
                return BadRequest(result);
            }

            Session sessionResult = await _context.Sessions.FirstOrDefaultAsync(s => s.SessionToken == token);
            User userResult = await _context.Users.FirstOrDefaultAsync(u => u.UserId == sessionResult.SessionUserId);
            Notice noticeResult = await _context.Notices.FirstOrDefaultAsync(n => n.UserId == id);

            if (userResult == null)
            {
                result = new ModelResult<Notice>(404, null, "User Not Exists");
                return BadRequest(result);
            }

            if (userResult.UserId != noticeResult.UserId)
            {
                result = new ModelResult<Notice>(405, null, "Cannnot See Others' Notice");
                return BadRequest(result);
            }

            _context.Notices.Remove(noticeResult);
            await _context.SaveChangesAsync();

            result = new ModelResult<Notice>(200, noticeResult, "Notice Deleted");
            return Ok(result);
        }
    }
}