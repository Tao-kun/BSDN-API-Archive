using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using BSDN_API.Models;

namespace BSDN_API.Models
{
    public interface IUser
    {
        int UserId { set; get; }

        [MaxLength(256)] string Email { set; get; }
        [MaxLength(256)] string PasswordHash { set; get; }
        [MaxLength(256)] string Nickname { set; get; }
        DateTime SignDate { set; get; }
    }

    public class User : IUser
    {
        public int UserId { set; get; }

        [MaxLength(256)] public string Email { set; get; }
        [MaxLength(256)] public string PasswordHash { set; get; }
        [MaxLength(256)] public string Nickname { set; get; }
        public DateTime SignDate { set; get; }

        public List<Article> Articles { set; get; }
        public List<UserFollow> UserFollowers { set; get; }
        public List<UserFollow> UserFollowings { set; get; }
    }

    public class UserInfo : IUser
    {
        public int UserId { set; get; }

        [MaxLength(256)] public string Email { set; get; }
        [MaxLength(256)] public string PasswordHash { set; get; }
        [MaxLength(256)] public string Nickname { set; get; }
        public DateTime SignDate { set; get; }

        public int ArticleCount { set; get; }
        public int UserFollowerCount { set; get; }
        public int UserFollowingCount { set; get; }

        public UserInfo(User user)
        {
            UserId = user.UserId;
            Email = user.Email;
            PasswordHash = user.PasswordHash;
            Nickname = user.Nickname;
            SignDate = user.SignDate;
            ArticleCount = user.Articles?.Count ?? 0;
            UserFollowerCount = user.UserFollowers?.Count ?? 0;
            UserFollowingCount = user.UserFollowings?.Count ?? 0;
        }
    }

    public class UserFollow
    {
        public int FollowerId { set; get; }
        public User Follower { set; get; }

        public int FollowingId { set; get; }
        public User Following { set; get; }
    }
}