using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BSDN_API.Models;
using Microsoft.AspNetCore.Internal;
using Microsoft.EntityFrameworkCore;

namespace BSDN_API.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class SessionController : ControllerBase
    {
        private readonly BSDNContext _context;

        public SessionController(BSDNContext context)
        {
            _context = context;
        }

        // POST api/session
        [HttpPost]
        public async Task<IActionResult> Post(User user)
        {
            ModelResult<Session> result;
            User userResult;
            if (user.Email != null)
            {
                userResult = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == user.Email);
            }
            else if (user.Nickname != null)
            {
                userResult = await _context.Users
                    .FirstOrDefaultAsync(u => u.Nickname == user.Nickname);
            }
            else
            {
                userResult = await _context.Users
                    .FirstOrDefaultAsync(u => u.UserId == user.UserId);
            }

            if (userResult == null || userResult.PasswordHash != user.PasswordHash)
            {
                result = new ModelResult<Session>(409, null, "User not Exists or Password wrong");
                return BadRequest(result);
            }

            string token = SessionUtils.GenerateSession(userResult);
            Session session = new Session
            {
                SessionToken = token,
                SessionUserId = userResult.UserId,
                ExpiresTime = DateTime.Now.AddDays(1)
            };
            await _context.Sessions.AddAsync(session);
            await _context.SaveChangesAsync();

            result = new ModelResult<Session>(201, session, "Session Created");
            return Ok(result);
        }

        // DELETE api/session/5
        [HttpDelete]
        public async Task<IActionResult> Delete([FromQuery(Name = "token")] string token)
        {
            ModelResult<Session> result;
            if (token == null)
            {
                result = new ModelResult<Session>(401, null, "Token Required");
                return BadRequest(result);
            }

            Session sessionResult = await _context.Sessions
                .FirstOrDefaultAsync(s => s.SessionToken == token);

            if (sessionResult == null)
            {
                result = new ModelResult<Session>(404, null, "Invalid Token or Session");
                return BadRequest(result);
            }

            _context.Sessions.Remove(sessionResult);
            await _context.SaveChangesAsync();
            result = new ModelResult<Session>(200, null, "Logout");
            return Ok(result);
        }
    }

    public static class SessionUtils
    {
        public static string GenerateSession(User user)
        {
            var rawToken = user.Nickname + user.UserId + user.Email + DateTime.Now.ToString();
            var md5 = MD5.Create();
            var data = md5.ComputeHash(Encoding.UTF8.GetBytes(rawToken));
            var stringBuilder = new StringBuilder();
            foreach (var ch in data)
            {
                stringBuilder.Append(ch.ToString("x2"));
            }

            return stringBuilder.ToString();
        }
    }
}