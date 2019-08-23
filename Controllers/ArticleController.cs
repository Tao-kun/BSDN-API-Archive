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
    // Article and upload
    public class ArticleController : ControllerBase
    {
        private readonly BSDNContext _context;

        public ArticleController(BSDNContext context)
        {
            _context = context;
        }

        // GET api/article?start={start article index}&offset={offset}&sort={sort type id}&tag={tag id}&keyword={keyword}
        // GET api/article?id={user id}&start={start article index}&offset={offset}&tag={tag id}
        // GET api/article/{article id}
        // POST api/article?token={token}
        // PUT api/article/{id}?token={token}
        // DELETE api/article/{id}?token={token}
    }
}