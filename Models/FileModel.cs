using System.ComponentModel.DataAnnotations;

namespace BSDN_API.Models
{
    public class UploadFile
    {
        public int UploadFileId { set; get; }
        [MaxLength(1024)] public string FileName { set; get; }
        public int UploaderId { set; get; }
    }

    public class UploadInfo
    {
        public int Status { set; get; }
        public string FileUrl { set; get; }

        public UploadInfo(int status, string fileUrl)
        {
            Status = status;
            FileUrl = fileUrl;
        }
    }
}