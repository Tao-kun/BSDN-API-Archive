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
    public class UserController : ControllerBase
    {
        private readonly BSDNContext _context;

        public UserController(BSDNContext context)
        {
            _context = context;
        }

        // GET api/user?start={start user index}&offset={offset}&sort={sort type id}&keyword={keyword}
        // TODO: 搜索
        [HttpGet]
        public async Task<IActionResult> Get(
            [FromQuery(Name = "start")] int start,
            [FromQuery(Name = "offset")] int offset)
        {
            ModelResultList<User> result;
            // TODO: 分页
            if (offset == 0)
            {
                offset = 20;
            }

            List<User> users = await _context.Users.ToListAsync();
            // TODO: has next
            bool hasNext = false;
            if (users.Count == 0)
            {
                result = new ModelResultList<User>(404, users, "No User Exists", hasNext);
            }
            else
            {
                result = new ModelResultList<User>(200, users, null, hasNext);
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
        public async Task<IActionResult> Delete(int id, [FromQuery(Name = "token")] string token)
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