
using System;
using System.Collections.Generic;

namespace BBtaChatbotJackApi.Models
{
    public class ChatSession
    {
        public int Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
    }

    public class ChatRequest
    {
        public int? SessionId { get; set; }
        public string PdfFilePath { get; set; } = "";
        public int FileId { get; set; }
        public string Question { get; set; } = "";
    }

  
}