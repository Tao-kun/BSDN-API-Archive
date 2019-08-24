using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using BSDN_API.Models;

namespace BSDN_API.Models
{
    public interface IArticle
    {
        int ArticleId { set; get; }
        int ViewNumber { set; get; }
        [MaxLength(256)] string Title { set; get; }
        string Content { set; get; }
        DateTime PublishDate { set; get; }
        int UserId { set; get; }
        User User { set; get; }
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
        public int UserId { set; get; }
        public User User { set; get; }
    }

    public class ArticleInfo
    {
        public int ArticleId { set; get; }
        public int ViewNumber { set; get; }
        [MaxLength(256)] public string Title { set; get; }
        public string Content { set; get; }
        public DateTime PublishDate { set; get; }

        public List<Tag> Tags { set; get; }
        public int CommentCount { set; get; }
        public int ResourceFileCount { set; get; }

        public int UserId { set; get; }
        public User User { set; get; }

        public ArticleInfo(Article article, BSDNContext context)
        {
            ArticleId = article.ArticleId;
            ViewNumber = article.ViewNumber;
            Title = article.Title;
            Content = article.Content;
            PublishDate = article.PublishDate;
            if (article.ArticleTags == null)
            {
                Tags = null;
            }
            else
            {
                Tags = article.ArticleTags.Select(
                    at => context.Tags.FirstOrDefault(t => t.TagId == at.TagId)
                ).ToList();
            }

            CommentCount = article.Comments?.Count ?? 0;
            ResourceFileCount = article.ResourceFiles?.Count ?? 0;
            UserId = article.UserId;
            User = article.User;
        }
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
        public int ArticleId { set; get; }
        public Article Article { set; get; }
    }

    public class ResourceFile
    {
        public int ResourceFileId { set; get; }
        [MaxLength(512)] public string Filename { set; get; }

        // FK_Article_ResourceFile
        public int ArticleId { set; get; }
        public Article Article { set; get; }
    }
}