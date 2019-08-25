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
    public class SessionController : ControllerBase
    {
        private readonly BSDNContext _context;

        public SessionController(BSDNContext context)
        {
            _context = context;
        }

        // GET api/session?token={token}
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery(Name = "token")] string token)
        {
            ModelResult<Session> result;
            Session sessionResult = await _context.Sessions.FirstOrDefaultAsync(s => s.SessionToken == token);
            if (sessionResult == null)
            {
                result = new ModelResult<Session>(404, null, "Token Not Exists");
                return BadRequest(result);
            }
            else
            {
                result = new ModelResult<Session>(200, sessionResult, null);
                return Ok(result);
            }
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

            string token;
            for (;;)
            {
                token = TokenUtils.GenerateSessionToken(userResult, _context);
                if (token != null)
                    break;
            }

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

        // PUT api/session?token={token}
        [HttpPut]
        public async Task<IActionResult> Put([FromQuery(Name = "token")] string token)
        {
            ModelResult<Session> result;
            Session sessionResult = await _context.Sessions.FirstOrDefaultAsync(s => s.SessionToken == token);
            if (sessionResult == null)
            {
                result = new ModelResult<Session>(404, null, "Token Not Exists");
                return BadRequest(result);
            }
            else
            {
                sessionResult.ExpiresTime = DateTime.Now.AddDays(1);
                _context.Entry(sessionResult).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                result = new ModelResult<Session>(200, sessionResult, null);
                return Ok(result);
            }
        }

        // DELETE api/session?token={token}
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
            
            result = new ModelResult<Session>(200, sessionResult, "Logout");
            return Ok(result);
        }
    }
}