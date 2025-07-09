using System;
using System.Collections.Generic;

namespace BBtaChatbotJackApi.Models
{
    public class DocumentChunk
    {
        public int Id { get; set; }
        public string OriginalFilePath { get; set; }
        public int ChunkIndex { get; set; }
        public string Content { get; set; }
        public string EmbeddingVector { get; set; }
        public string IdentifiedEntities { get; set; }
        public string IdentifiedIntents { get; set; }
        public DateTime ProcessedTimestamp { get; set; }
    }
}