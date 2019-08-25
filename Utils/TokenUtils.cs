using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using BSDN_API.Models;
using Microsoft.EntityFrameworkCore;

namespace BSDN_API.Utils
{
    public class TokenUtils
    {
        public static string GenerateSessionToken(User user, BSDNContext context)
        {
            var rawToken = user.Nickname + user.UserId + user.Email + DateTime.Now.ToString();
            var md5 = MD5.Create();
            var data = md5.ComputeHash(Encoding.UTF8.GetBytes(rawToken));
            var stringBuilder = new StringBuilder();
            foreach (var ch in data)
            {
                stringBuilder.Append(ch.ToString("x2"));
            }

            string token = stringBuilder.ToString();
            Session session = context.Sessions.FirstOrDefault(s => s.SessionToken == token);
            return session == null ? token : null;
        }

        public static ModelResult<T> CheckToken<T>(string token, BSDNContext context)
        {
            ModelResult<T> result;
            if (token == null)
            {
                result = new ModelResult<T>(405, default, "Need Token");
            }
            else
            {
                Session session = context.Sessions
                    .FirstOrDefault(s => s.SessionToken == token);
                if (session == null)
                {
                    result = new ModelResult<T>(405, default, "Token Not Exists");
                }
                else if (session.ExpiresTime < DateTime.Now)
                {
                    result = new ModelResult<T>(405, default, "Token Expires");
                }
                else
                {
                    result = null;
                }
            }

            return result;
        }
    }
}