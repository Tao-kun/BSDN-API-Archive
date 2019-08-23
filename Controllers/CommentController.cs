using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BSDN_API.Models;

namespace BSDN_API.Controllers
{
    public class CommentController : ControllerBase
    {
        private readonly BSDNContext _context;

        public CommentController(BSDNContext context)
        {
            _context = context;
        }

        // GET api/comment?id={article id}&start={start index index}&offset={offset}
        // POST api/comment?token={token}
        // POST api/comment/{comment id}?token={token}
        // DELETE api/comment/{comment id}?token={token}
    }
}