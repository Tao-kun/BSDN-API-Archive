using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BSDN_API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        // GET api/user
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            // TODO: 分页
            var users = _context.Users;
            ModelResult<DbSet<User>> result = new ModelResult<DbSet<User>>(200, users, null);
            return Ok(result);
        }

        // GET api/user/{user id}
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            ModelResult<User> result;
            var userResult = await _context.Users.FirstOrDefaultAsync(u => u.UserId == id);
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

            result = new ModelResult<User>(201, user, "Created");
            return Ok(result);
        }

        // PUT api/user/{user id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, User user)
        {
            // TODO: impl it and add result
            if (id != user.UserId)
            {
                return BadRequest();
            }

            _context.Entry(user).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Ok();
        }

        // DELETE api/user/{user id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id, [FromQuery(Name = "token")] string token)
        {
            ModelResult<User> result;
            if (token == null)
            {
                result = new ModelResult<User>(405, null, "");
                return BadRequest(new JsonResult(result));
            }

            Session session = await _context.Sessions
                .FirstOrDefaultAsync(s => s.SessionToken == token);
            if (session == null)
            {
                result = new ModelResult<User>(404, null, "Token Not Exists");
                return BadRequest(result);
            }

            if (session.ExpiresTime < DateTime.Now)
            {
                result = new ModelResult<User>(405, null, "Token Expires");
                return BadRequest(result);
            }

            User userResult = await _context.Users.FirstOrDefaultAsync(u => u.UserId == session.SessionUserId);
            if (userResult == null)
            {
                result = new ModelResult<User>(404, null, "User Not Exists or Token not suit");
                return BadRequest(result);
            }

            _context.Users.Remove(userResult);
            await _context.SaveChangesAsync();
            result = new ModelResult<User>(200, null, "User Deleted");
            return Ok(result);
        }
    }
}