using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace BSDN_API.Models
{
    public class User
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

    public class UserFollow
    {
        public int FollowerId { set; get; }
        public User Follower { set; get; }

        public int FollowingId { set; get; }
        public User Following { set; get; }
    }

    public class Article
    {
        public int ArticleId { set; get; }
        public int ViewNumber { set; get; }
        [MaxLength(256)] public string Title { set; get; }
        public string Content { set; get; }
        public DateTime PublishDate { set; get; }

        public List<ArticleTag> ArticleTags { set; get; }
        public List<Comment> Comments { set; get; }
        public List<ResourceFile> ResourceFiles { set; get; }

        // FK_User_Article
        public User User { set; get; }
    }

    public class ArticleTag
    {
        // 多对多
        // FK_Article_ArticleTag
        public int ArticleId { set; get; }
        public Article Article { set; get; }

        // FK_Tag_ArticleTag
        public int TagId { set; get; }
        public Tag Tag { set; get; }
    }

    public class Tag
    {
        public int TagId { set; get; }
        [MaxLength(64)] public string TagName { set; get; }

        public List<ArticleTag> ArticleTags { set; get; }
    }

    public class Comment
    {
        public int CommentId { set; get; }
        public string Content { set; get; }
        public DateTime PublishDate { set; get; }

        public Comment ReplyComment { set; get; }

        // FK_Article_Comment
        public Article Article { set; get; }
    }

    public class ResourceFile
    {
        public int ResourceFileId { set; get; }
        [MaxLength(512)] public string Filename { set; get; }

        // FK_Article_ResourceFile
        public Article Article { set; get; }
    }

    public class Session
    {
        public int SessionId { set; get; }
        [MaxLength(512)] public string SessionToken { set; get; }
        public DateTime ExpiresTime { set; get; }
        public int SessionUserId { set; get; }
    }

    public class BSDNContext : DbContext
    {
        public BSDNContext(DbContextOptions<BSDNContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // FK_User_Article
            modelBuilder.Entity<Article>()
                .HasOne(a => a.User)
                .WithMany(u => u.Articles);

            // Unique Nickname and Email
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Nickname)
                .IsUnique();

            // FK_Article_Comment
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Article)
                .WithMany(a => a.Comments);

            // Reply other comments
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.ReplyComment)
                .WithMany();

            // FK_Article_ResourceFile
            modelBuilder.Entity<ResourceFile>()
                .HasOne(rf => rf.Article)
                .WithMany(a => a.ResourceFiles);

            // FK_Article_ArticleTag_Tag
            modelBuilder.Entity<ArticleTag>()
                .HasKey(at => new {at.ArticleId, at.TagId});
            modelBuilder.Entity<ArticleTag>()
                .HasOne(at => at.Article)
                .WithMany(a => a.ArticleTags)
                .HasForeignKey(at => at.ArticleId);
            modelBuilder.Entity<ArticleTag>()
                .HasOne(at => at.Tag)
                .WithMany(t => t.ArticleTags)
                .HasForeignKey(at => at.TagId);

            // FK_Follower_UserFollow_Following
            modelBuilder.Entity<UserFollow>()
                .HasKey(uf => new {uf.FollowerId, uf.FollowingId});
            modelBuilder.Entity<UserFollow>()
                .HasOne(uf => uf.Following)
                .WithMany(fg => fg.UserFollowings)
                .HasForeignKey(uf => uf.FollowingId);
            modelBuilder.Entity<UserFollow>()
                .HasOne(uf => uf.Follower)
                .WithMany(fer => fer.UserFollowers)
                .HasForeignKey(uf => uf.FollowerId);

            modelBuilder.Entity<Session>()
                .HasIndex(s => s.SessionToken)
                .IsUnique();
        }

        // DbSet
        public DbSet<User> Users { set; get; }
        public DbSet<Article> Articles { set; get; }
        public DbSet<Tag> Tags { set; get; }
        public DbSet<Comment> Comments { set; get; }
        public DbSet<ResourceFile> ResourceFiles { set; get; }
        public DbSet<Session> Sessions { set; get; }
    }
}