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
        [HttpGet]
        public async Task<IActionResult> Get(
            [FromQuery(Name = "offset")] int offset,
            [FromQuery(Name = "limit")] int limit,
            [FromQuery(Name = "sort")] int sort,
            [FromQuery(Name = "keyword")] string keyword)
        {
            // 排序相关：
            // 0     -> 不排序，直接返回查询结果（默认）
            // 1     -> 按照结果中的用户ID升序
            // 2     -> 按照结果中的用户ID降序
            // 3     -> 按照结果中的用户注册日期升序
            // 4     -> 按照结果中的用户注册日期降序
            // 5     -> 按照结果中的用户昵称名升序
            // 6     -> 按照结果中的用户昵称名降序
            // other -> 不排序，直接返回查询结果（同0）
            ModelResultList<UserInfo> result;
            if (limit == 0)
            {
                limit = 10;
            }
            else if (limit < 0)
            {
                limit = 0;
            }

            List<UserInfo> userInfos = await _context.Users.Select(u => new UserInfo(u)).ToListAsync();
            if (keyword != null)
                userInfos = userInfos
                    .Where(ui => ui.Nickname.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) > 0)
                    .ToList();

            int totalCount = userInfos.Count;
            if (totalCount == 0)
            {
                result = new ModelResultList<UserInfo>(404, null,
                    "No User Exists", false, totalCount, null);
                return Ok(result);
            }

            bool hasNext = offset + limit < totalCount;

            string nextUrl = hasNext
                ? $@"/api/user?sort={sort}&keyword={keyword}&limit={limit}&offset={limit + offset}"
                : null;

            switch (sort)
            {
                case 0:
                    break;
                case 1:
                    userInfos.Sort((u1, u2) => u1.UserId - u2.UserId);
                    break;
                case 2:
                    userInfos.Sort((u1, u2) => u2.UserId - u1.UserId);
                    break;
                case 3:
                    userInfos.Sort((u1, u2) => DateTime.Compare(u1.SignDate, u2.SignDate));
                    break;
                case 4:
                    userInfos.Sort((u1, u2) => DateTime.Compare(u2.SignDate, u1.SignDate));
                    break;
                case 5:
                    userInfos.Sort((u1, u2) => string.Compare(u1.Nickname, u2.Nickname, StringComparison.Ordinal));
                    break;
                case 6:
                    userInfos.Sort((u1, u2) => string.Compare(u2.Nickname, u1.Nickname, StringComparison.Ordinal));
                    break;
            }

            if (offset <= totalCount)
            {
                if (offset + limit > totalCount)
                    limit = totalCount - offset;
                userInfos = userInfos.GetRange(offset, limit);
            }
            else
            {
                result = new ModelResultList<UserInfo>(400, null,
                    "Index Out of Index", false, totalCount, nextUrl);
                return BadRequest(result);
            }

            if (userInfos.Count == 0)
            {
                result = new ModelResultList<UserInfo>(404, null,
                    "No User Exists", hasNext, totalCount, nextUrl);
            }
            else
            {
                userInfos = userInfos.Select(ui =>
                {
                    ui.UserFollowerCount = _context.UserFollows.Count(uf => uf.FollowingId == ui.UserId);
                    ui.UserFollowingCount = _context.UserFollows.Count(uf => uf.FollowerId == ui.UserId);
                    ui.ArticleCount = _context.Articles.Count(a => a.UserId == ui.UserId);
                    return ui;
                }).ToList();

                result = new ModelResultList<UserInfo>(200, userInfos,
                    null, hasNext, totalCount, nextUrl);
            }

            return Ok(result);
        }

        // GET api/user/{user id}
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            ModelResult<UserInfo> result;
            var userResult = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == id);
            if (userResult == null)
            {
                result = new ModelResult<UserInfo>(404, null, "User Not Exists");
                return BadRequest(result);
            }

            UserInfo userInfo = new UserInfo(userResult);
            userInfo.UserFollowerCount = _context.UserFollows.Count(uf => uf.FollowingId == userInfo.UserId);
            userInfo.UserFollowingCount = _context.UserFollows.Count(uf => uf.FollowerId == userInfo.UserId);
            userInfo.ArticleCount = _context.Articles.Count(a => a.UserId == userInfo.UserId);

            result = new ModelResult<UserInfo>(200, userInfo, "User Exists");
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

            userResult.Email = user.Email;
            userResult.Nickname = user.Nickname;
            userResult.PasswordHash = user.PasswordHash;
            userResult.AvatarUrl = user.AvatarUrl;
            userResult.Intro = user.Intro;

            _context.Entry(userResult).State = EntityState.Modified;
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