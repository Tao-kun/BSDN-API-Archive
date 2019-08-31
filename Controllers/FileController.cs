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

                string hash = FileHash(file);
                string extension = Path.GetExtension(file.FileName);
                string filename = $@"{hash}{extension}";

                string filePath = $@"{uploadFolder}/{filename}";
                using (FileStream fs = System.IO.File.Create(filePath))
                {
                    file.CopyTo(fs);
                    await fs.FlushAsync();
                }

                Session sessionResult = await _context.Sessions.FirstOrDefaultAsync(s => s.SessionToken == token);
                User userResult =
                    await _context.Users.FirstOrDefaultAsync(u => u.UserId == sessionResult.SessionUserId);
                int userId = userResult.UserId;

                UploadFile uploadFileResult = await _context.UploadFiles
                    .FirstOrDefaultAsync(f => f.FileName == $@"{filename}");
                if (uploadFileResult == null)
                {
                    await _context.UploadFiles.AddAsync(new UploadFile
                    {
                        FileName = $@"{filename}",
                        UploaderId = userId
                    });
                    await _context.SaveChangesAsync();
                }

                result = new ModelResult<string>(200, $@"/file/{filename}", null);
                return Ok(result);
            }
            else
            {
                result = new ModelResult<string>(400, null, "Empty File");
                return BadRequest(result);
            }
        }

        private string FileHash(IFormFile file)
        {
            MemoryStream stream = new MemoryStream();
            file.OpenReadStream().CopyTo(stream);

            byte[] bytes = MD5.Create().ComputeHash(stream.ToArray());
            return BitConverter.ToString(bytes).Replace("-", string.Empty).ToLower();
        }
    }
}