using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using BSDN_API.Models;

namespace BSDN_API.Models
{
    public interface ITag
    {
        int TagId { set; get; }
        [MaxLength(64)] string TagName { set; get; }
    }

    public class Tag : ITag
    {
        public int TagId { set; get; }
        [MaxLength(64)] public string TagName { set; get; }

        public List<ArticleTag> ArticleTags { set; get; }
    }

    public class TagInfo : ITag
    {
        public int TagId { set; get; }
        [MaxLength(64)] public string TagName { set; get; }
        public int ArticleCount { set; get; }

        public TagInfo(Tag tag)
        {
            TagId = tag.TagId;
            TagName = tag.TagName;
            ArticleCount = tag.ArticleTags?.Count ?? 0;
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
}