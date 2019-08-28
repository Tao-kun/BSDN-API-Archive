using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using BSDN_API.Controllers;
using Microsoft.EntityFrameworkCore;

namespace BSDN_API.Models
{
    public interface IComment
    {
        int CommentId { set; get; }
        string Content { set; get; }
        DateTime PublishDate { set; get; }
        int ArticleId { set; get; }
        int UserId { set; get; }
    }

    public class Comment : IComment
    {
        public int CommentId { set; get; }
        public string Content { set; get; }
        public DateTime PublishDate { set; get; }

        // FK_Article_Comment
        public int ArticleId { set; get; }
        public Article Article { set; get; }

        public int UserId { set; get; }
        public User User { set; get; }
    }

    public class CommentInfo : IComment
    {
        public int CommentId { get; set; }
        public string Content { get; set; }
        public DateTime PublishDate { get; set; }
        public int ArticleId { get; set; }
        public int ArticleCommentCount { set; get; }
        public int UserId { set; get; }

        public bool IsReply { set; get; }
        public int RepliedCommentId { set; get; }

        public CommentInfo(Comment comment)
        {
            CommentId = comment.CommentId;
            Content = comment.Content;
            PublishDate = comment.PublishDate;
            ArticleId = comment.ArticleId;
            ArticleCommentCount = default;
            UserId = comment.UserId;
            IsReply = default;
            RepliedCommentId = default;
        }
    }

    public class CommentReply
    {
        // 回复
        public int CommentId { set; get; }

        public Comment Comment { set; get; }

        // 被回复评论
        public int RepliedCommentId { set; get; }
        public Comment RepliedCOmment { set; get; }
    }
}