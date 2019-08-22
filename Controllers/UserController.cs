using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BSDN_API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BSDN_API.Controllers
{
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
        public string Post([FromBody] JsonObject value)
        {
            // TODO: impl it
            //var userResult = _context.Users.FirstOrDefaultAsync(u => u.UserId == id);
            Console.Error.WriteLine("Post value: " + value);
            return value.ToString();
        }

        // PUT api/user/{user id}
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
            // TODO: impl it
            Console.WriteLine("ID:{0}, Value:{1}", id, value);
        }

        // DELETE api/user/{user id}
        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var userResult = _context.Users.FirstOrDefaultAsync(u => u.UserId == id);
            if (userResult == null)
            {
                return BadRequest();
            }

            _context.Users.Remove(userResult.Result);
            _context.SaveChanges();
            return Ok();
        }
    }
}