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
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace BSDN_API.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class FollowController : ControllerBase
    {
        private readonly BSDNContext _context;

        public FollowController(BSDNContext context)
        {
            _context = context;
        }

        // GET api/follow/{follower,following}?id={user id}&offset=<offset>&limit={limit}
        [HttpGet("{type}")]
        public async Task<IActionResult> Get(
            string type,
            [FromQuery(Name = "id")] int id,
            [FromQuery(Name = "offset")] int offset,
            [FromQuery(Name = "limit")] int limit)
        {
            ModelResultList<UserFollowInfo> result;

            if (limit == 0)
            {
                limit = 20;
            }
            else if (limit < 0)
            {
                limit = 0;
            }

            if (id == 0)
            {
                result = new ModelResultList<UserFollowInfo>(404, null, "Need User Id", false, 0);
                return BadRequest(result);
            }

            List<User> users;
            List<UserFollowInfo> userFollowInfos;
            if (type == "follower")
            {
                List<UserFollow> userFollowers = await _context.UserFollows
                    .Where(uf => uf.FollowingId == id).ToListAsync();
                users = userFollowers
                    .Select(uf => _context.Users
                        .FirstOrDefault(u => uf.FollowerId == u.UserId))
                    .ToList();
            }

            else if (type == "following")
            {
                List<UserFollow> userFollowings = await _context.UserFollows
                    .Where(uf => uf.FollowerId == id).ToListAsync();
                users = userFollowings
                    .Select(uf => _context.Users
                        .FirstOrDefault(u => uf.FollowingId == u.UserId))
                    .ToList();
            }
            else
            {
                result = new ModelResultList<UserFollowInfo>(403, null, "Invalid Type", false, 0);
                return BadRequest(result);
            }

            userFollowInfos = users.Select(u => new UserFollowInfo(u)).ToList();

            int totalCount = userFollowInfos.Count;
            bool hasNext = offset + limit < totalCount;
            if (offset <= totalCount)
            {
                if (offset + limit > totalCount)
                    limit = totalCount - offset;
                userFollowInfos = userFollowInfos.GetRange(offset, limit);
            }
            else
            {
                result = new ModelResultList<UserFollowInfo>(400, null, "Index Out of Limit", false, totalCount);
                return BadRequest(result);
            }

            if (userFollowInfos.Count == 0)
            {
                result = new ModelResultList<UserFollowInfo>(
                    400, null, $"No {type}", false, totalCount);
            }
            else
            {
                userFollowInfos = userFollowInfos
                    .Select(ufi =>
                    {
                        ufi.FollowerCount = _context.UserFollows
                            .Count(uf => uf.FollowingId == ufi.UserId);
                        ufi.FollowingCount = _context.UserFollows
                            .Count(uf => uf.FollowerId == ufi.UserId);
                        return ufi;
                    }).ToList();
                result = new ModelResultList<UserFollowInfo>(200, userFollowInfos, null, hasNext, totalCount);
            }

            return Ok(result);
        }

        // POST api/follow/{user id}token={token}
        [HttpPost("{id}")]
        public async Task<IActionResult> Post(
            int id,
            [FromQuery(Name = "token")] string token)
        {
            ModelResult<UserFollowInfo> result = TokenUtils.CheckToken<UserFollowInfo>(token, _context);
            if (result != null)
            {
                return BadRequest(result);
            }

            Session sessionResult = await _context.Sessions
                .FirstOrDefaultAsync(s => s.SessionToken == token);
            User follower = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == sessionResult.SessionUserId);

            UserFollow userFollowResult = await _context.UserFollows
                .FirstOrDefaultAsync(uf => uf.FollowingId == id &&
                                           uf.FollowerId == sessionResult.SessionUserId);
            if (userFollowResult != null)
            {
                result = new ModelResult<UserFollowInfo>(400, null, "User Followed");
                return BadRequest(result);
            }

            User following = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == userFollowResult.FollowingId);

            UserFollow userFollow = new UserFollow
            {
                Follower = follower,
                FollowerId = follower.UserId,
                Following = following,
                FollowingId = following.UserId
            };

            _context.Add(userFollow);
            await _context.SaveChangesAsync();

            result = new ModelResult<UserFollowInfo>(201, null, "Add Follow Success");
            return Ok(result);
        }

        // DELETE api/follow/{user id}?token={token}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(
            int id,
            [FromQuery(Name = "token")] string token)
        {
            ModelResult<UserFollowInfo> result = TokenUtils.CheckToken<UserFollowInfo>(token, _context);
            if (result != null)
            {
                return BadRequest(result);
            }

            Session sessionResult = await _context.Sessions
                .FirstOrDefaultAsync(s => s.SessionToken == token);
            User follower = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == sessionResult.SessionUserId);

            UserFollow userFollowResult = await _context.UserFollows
                .FirstOrDefaultAsync(uf => uf.FollowingId == id &&
                                           uf.FollowerId == sessionResult.SessionUserId);
            if (userFollowResult == null)
            {
                result = new ModelResult<UserFollowInfo>(400, null, "User Not Followed");
                return BadRequest(result);
            }

            _context.Remove(userFollowResult);
            await _context.SaveChangesAsync();

            result = new ModelResult<UserFollowInfo>(201, null, "Remove Follow Success");
            return Ok(result);
        }
    }
}