
using System;

namespace BBtaChatbotJackApi.Models
{
    public class PdfDocuments
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public DateTime UploadDate { get; set; }
        public string Status { get; set; } 
    }
    


}