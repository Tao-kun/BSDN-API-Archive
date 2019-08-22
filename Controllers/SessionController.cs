using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace BSDN_API.Controllers
{
    public class SessionController : ControllerBase
    {
        // POST api/session
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // DELETE api/session/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}