
using System;

namespace BBtaChatbotJackApi.Models
{
    public class Message
    {
        public int Id { get; set; }
        public int ChatSessionId { get; set; } // Foreign key to ChatSession
        public string Sender { get; set; } // e.g., "User", "AI"
        public string Content { get; set; }
        public DateTime Timestamp { get; set; }
        public string IdentifiedTopic { get; set; } // To store the topic identified by AI

        public virtual ChatSession ChatSession { get; set; } // Navigation property back to ChatSession
    }
}