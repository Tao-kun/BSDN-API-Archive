using System;
using System.ComponentModel.DataAnnotations;

namespace BSDN_API.Models
{
    public class Notice
    {
        public int NoticeId { set; get; }
        public int UserId { set; get; }
        public DateTime AddTime { set; get; }
        [MaxLength(1024)] public string NoticeData { set; get; }
        [MaxLength(512)] public string ApiUrl { set; get; }

        public Notice(int userId, string noticeData, string apiUrl)
        {
            NoticeId = default;
            UserId = userId;
            AddTime = DateTime.Now;
            NoticeData = noticeData;
            ApiUrl = apiUrl;
        }
    }
}