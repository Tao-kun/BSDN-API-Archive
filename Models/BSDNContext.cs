using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using BSDN_API.Models;

namespace BSDN_API.Models
{
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
                .WithMany(u => u.Articles)
                .HasForeignKey(a => a.UserId);

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
                .WithMany(a => a.Comments)
                .HasForeignKey(c => c.ArticleId);

            // FK_Comment_CommentReply_Reply
            modelBuilder.Entity<CommentReply>()
                .HasKey(cr => new {cr.CommentId, cr.RepliedCommentId});
            modelBuilder.Entity<CommentReply>()
                .HasIndex(cr => cr.CommentId)
                .IsUnique();

            // FK_Article_ResourceFile
            modelBuilder.Entity<ResourceFile>()
                .HasOne(rf => rf.Article)
                .WithMany(a => a.ResourceFiles)
                .HasForeignKey(rf => rf.ArticleId);

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

            modelBuilder.Entity<Tag>()
                .HasIndex(t => t.TagName)
                .IsUnique();

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
        public DbSet<ArticleTag> ArticleTags { set; get; }
        public DbSet<Comment> Comments { set; get; }
        public DbSet<CommentReply> CommentReplies { set; get; }
        public DbSet<ResourceFile> ResourceFiles { set; get; }
        public DbSet<Session> Sessions { set; get; }
        public DbSet<UserFollow> UserFollows { set; get; }
        public DbSet<Notice> Notices { set; get; }
    }
}