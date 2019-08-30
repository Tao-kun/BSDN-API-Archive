using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BSDN_API.Models;
using BSDN_API.Utils;
using Microsoft.AspNetCore.Http;

namespace BSDN_API.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private static readonly string uploadFolder = "data";
        private readonly BSDNContext _context;

        public FileController(BSDNContext context)
        {
            _context = context;
        }

        // POST /api/file?token={token}
        [HttpPost]
        public async Task<IActionResult> Post(IFormFile file, [FromQuery(Name = "token")] string token)
        {
            ModelResult<string> result = TokenUtils.CheckToken<string>(token, _context);
            if (result != null)
            {
                return BadRequest(result);
            }

            if (file != null)
            {
                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }

                MemoryStream ms = new MemoryStream();
                file.OpenReadStream().CopyTo(ms);

                MD5 md5 = MD5.Create();
                byte[] data = md5.ComputeHash(ms);
                StringBuilder stringBuilder = new StringBuilder();
                foreach (var ch in data)
                {
                    stringBuilder.Append(ch.ToString("x2"));
                }

                string hash = stringBuilder.ToString();

                string filename = $@"{hash}";
                string extension = Path.GetExtension(file.FileName);

                string filePath = $@"{uploadFolder}/{filename}{extension}";
                using (FileStream fs = System.IO.File.Create(filePath))
                {
                    file.CopyTo(fs);
                    await fs.FlushAsync();
                }

                Session sessionResult = await _context.Sessions.FirstOrDefaultAsync(s => s.SessionToken == token);
                User userResult =
                    await _context.Users.FirstOrDefaultAsync(u => u.UserId == sessionResult.SessionUserId);
                int userId = userResult.UserId;

                await _context.Files.AddAsync(new UploadFile
                {
                    FileName = filename,
                    UploaderId = userId
                });
                await _context.SaveChangesAsync();

                result = new ModelResult<string>(200, $@"/file/{filename}", null);
                return Ok(result);
            }
            else
            {
                result = new ModelResult<string>(400, null, "Empty File");
                return BadRequest(result);
            }
        }
    }
}