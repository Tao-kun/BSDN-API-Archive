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
        [MaxLength(512)] string Intro { set; get; }
        [MaxLength(512)] string AvatarUrl { set; get; }
        DateTime SignDate { set; get; }
    }

    public class User : IUser
    {
        public int UserId { set; get; }

        [MaxLength(256)] public string Email { set; get; }
        [MaxLength(256)] public string PasswordHash { set; get; }
        [MaxLength(256)] public string Nickname { set; get; }
        public DateTime SignDate { set; get; }
        [MaxLength(512)] public string Intro { get; set; }
        [MaxLength(512)] public string AvatarUrl { get; set; }

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
        [MaxLength(512)] public string Intro { get; set; }
        [MaxLength(512)] public string AvatarUrl { get; set; }

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
            UserFollowerCount = default;
            UserFollowingCount = default;
            AvatarUrl = user.AvatarUrl;
            Intro = Intro;
        }
    }

    public class UserFollow
    {
        // 主动方、关注者
        public int FollowerId { set; get; }
        public User Follower { set; get; }

        // 被动方、被关注者
        public int FollowingId { set; get; }
        public User Following { set; get; }
    }

    public class UserFollowInfo
    {
        public int UserId { set; get; }
        public string NickName { set; get; }
        public int FollowerCount { set; get; }
        public int FollowingCount { set; get; }

        public UserFollowInfo(User user)
        {
            UserId = user.UserId;
            NickName = user.Nickname;
            FollowerCount = default;
            FollowingCount = default;
        }
    }
}