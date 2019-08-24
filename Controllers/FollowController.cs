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
    public class FollowController : ControllerBase
    {
        private readonly BSDNContext _context;

        public FollowController(BSDNContext context)
        {
            _context = context;
        }

        // GET api/follow?id={user id}&offset=<offset>&limit={limit}
        // POST api/follow?token={token}
        // PUT api/follow?token={token}
        // DELETE api/follow/{user id}?token={token}
    }
}