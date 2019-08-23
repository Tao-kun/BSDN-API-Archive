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
            var users = _context.Users;
            return new JsonResult(users);
        }

        // GET api/user/{user id}
        [HttpGet("{id}")]
        public ActionResult Get(int id)
        {
            var userResult = _context.Users.FirstOrDefaultAsync(u => u.UserId == id);
            if (userResult == null)
            {
                return BadRequest();
            }

            return new JsonResult(userResult.Result);
        }

        // POST api/user
        [HttpPost]
        public async Task<IActionResult> Post(User user)
        {
            var userResult = await _context.Users
                .FirstOrDefaultAsync(u => u.Nickname == user.Nickname ||
                                          u.Email == user.Email);
            if (userResult != null)
            {
                return BadRequest();
            }

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            return Ok();
        }

        // PUT api/user/{user id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, User user)
        {
            // TODO: impl it
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
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var userResult = await _context.Users.FirstOrDefaultAsync(u => u.UserId == id);
            if (userResult == null)
            {
                return BadRequest();
            }

            _context.Users.Remove(userResult);
            _context.SaveChanges();
            return Ok();
        }
    }
}