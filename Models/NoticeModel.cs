using System;
using System.ComponentModel.DataAnnotations;

namespace BSDN_API.Models
{
    public interface INotice
    {
        int NoticeId { set; get; }
        int UserId { set; get; }
        int ArticleId { set; get; }
        string NoticeData { set; get; }
        DateTime AddTime { set; get; }
    }

    public class Notice : INotice
    {
        public int NoticeId { set; get; }
        public int UserId { set; get; }
        public int ArticleId { set; get; }
        [MaxLength(1024)] public string NoticeData { set; get; }
        public DateTime AddTime { set; get; }

        public Notice(int userId, int articleId, string noticeData)
        {
            NoticeId = default;
            UserId = userId;
            ArticleId = articleId;
            NoticeData = noticeData;
            AddTime = DateTime.Now;
        }
    }
}