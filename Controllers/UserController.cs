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
    public class UserController : ControllerBase
    {
        private readonly BSDNContext _context;

        public UserController(BSDNContext context)
        {
            _context = context;
        }

        // GET api/user?offset={offset}&limit={limit}&sort={sort type id}&keyword={keyword}
        // TODO: 搜索
        [HttpGet]
        public async Task<IActionResult> Get(
            [FromQuery(Name = "offset")] int offset,
            [FromQuery(Name = "limit")] int limit)
        {
            ModelResultList<UserInfo> result;
            if (limit == 0)
            {
                limit = 20;
            }
            else if (limit < 0)
            {
                limit = 0;
            }

            List<UserInfo> userInfos = await _context.Users.Select(u => new UserInfo(u)).ToListAsync();
            int totalCount = userInfos.Count;
            bool hasNext = offset + limit < totalCount;

            if (offset <= totalCount)
            {
                if (offset + limit > totalCount)
                    limit = totalCount - offset;
                userInfos = userInfos.GetRange(offset, limit);
            }
            else
            {
                result = new ModelResultList<UserInfo>(
                    400, null, "Index Out of Index", false, totalCount);
                return BadRequest(result);
            }

            if (userInfos.Count == 0)
            {
                result = new ModelResultList<UserInfo>(404, null, "No User Exists", hasNext, totalCount);
            }
            else
            {
                result = new ModelResultList<UserInfo>(200, userInfos, null, hasNext, totalCount);
            }

            return Ok(result);
        }

        // GET api/user/{user id}
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            ModelResult<User> result;
            var userResult = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == id);
            if (userResult == null)
            {
                result = new ModelResult<User>(404, null, "User Not Exists");
                return BadRequest(result);
            }

            result = new ModelResult<User>(200, userResult, "User Exists");
            return Ok(result);
        }

        // POST api/user
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] User user)
        {
            ModelResult<User> result;

            if (user.Email == null ||
                user.Nickname == null ||
                user.PasswordHash == null ||
                user.UserId != 0)
            {
                result = new ModelResult<User>(400, user, "Invalid User Info");
                return BadRequest(result);
            }

            var userResult = await _context.Users
                .FirstOrDefaultAsync(u => u.Nickname == user.Nickname ||
                                          u.Email == user.Email);
            if (userResult != null)
            {
                result = new ModelResult<User>(409, null, "User Exists");
                return BadRequest(result);
            }

            if (user.SignDate == DateTime.MinValue)
            {
                user.SignDate = DateTime.Now;
            }

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            result = new ModelResult<User>(201, user, "User Created");
            return Ok(result);
        }

        // PUT api/user/{user id}?token={token}
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(
            int id,
            [FromBody] User user,
            [FromQuery(Name = "token")] string token)
        {
            ModelResult<User> result = TokenUtils.CheckToken<User>(token, _context);
            if (result != null)
            {
                return BadRequest(result);
            }

            User userResult = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == id);
            Session sessionResult = await _context.Sessions
                .FirstOrDefaultAsync(s => s.SessionToken == token);
            if (userResult == null || userResult.UserId != sessionResult.SessionUserId)
            {
                result = new ModelResult<User>(405, null, "User Not Exists or Token not suit");
                return BadRequest(result);
            }

            if (id != user.UserId)
            {
                result = new ModelResult<User>(405, null, "Cannot Modify UserId");
                return BadRequest(result);
            }

            _context.Entry(user).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            result = new ModelResult<User>(200, null, "User Modified");
            return Ok(result);
        }

        // DELETE api/user/{user id}?token={token}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(
            int id,
            [FromQuery(Name = "token")] string token)
        {
            ModelResult<User> result = TokenUtils.CheckToken<User>(token, _context);
            if (result != null)
            {
                return BadRequest(result);
            }

            User userResult = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == id);
            Session sessionResult = await _context.Sessions
                .FirstOrDefaultAsync(s => s.SessionToken == token);
            if (userResult == null || userResult.UserId != sessionResult.SessionUserId)
            {
                result = new ModelResult<User>(405, null, "User Not Exists or Token not suit");
                return BadRequest(result);
            }

            _context.Users.Remove(userResult);
            await _context.SaveChangesAsync();

            result = new ModelResult<User>(200, userResult, "User Deleted");
            return Ok(result);
        }
    }
}