
using System;
using System.Collections.Generic;

namespace BBtaChatbotJackApi.Models
{
    public class ChatSession
    {
        public int Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public virtual ICollection<Message> Messages { get; set; } // Navigation property to messages
    }
}