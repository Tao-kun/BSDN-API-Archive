using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using BSDN_API.Models;

namespace BSDN_API.Models
{
    public class Session
    {
        public int SessionId { set; get; }
        [MaxLength(512)] public string SessionToken { set; get; }
        public DateTime ExpiresTime { set; get; }
        public int SessionUserId { set; get; }
    }
}