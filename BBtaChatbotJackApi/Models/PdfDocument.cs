
using System;

namespace BBtaChatbotJackApi.Models
{
    public class PdfDocument
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public DateTime UploadDate { get; set; }
        public string Status { get; set; } 
    }
    

    public class FileInfo
    {
        public int Id { get; set; }
        public string FileName { get; set; } = "";
        public string FilePath { get; set; } = "";
        public string FileType { get; set; } = "";
        public long FileSize { get; set; }
        public DateTime UploadDate { get; set; }
        public string Status { get; set; } 

    }

}