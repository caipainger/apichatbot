csharp
using System;

namespace BBtaChatbotJackApi.Models
{
    public class PdfDocument
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public DateTime UploadDate { get; set; }
        public string Status { get; set; } // e.g., "Processed", "Pending", "Error"
    }
    csharp
    // Inside BBtaChatbotJackApi/Models/FileInfo.cs (after renaming)
    public class FileInfo // Renamed from PdfDocument
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string FileType { get; set; } // Add this property
        public long FileSize { get; set; } // Add this property
        public DateTime UploadDate { get; set; }
        public string Status { get; set; }

        // ... other properties if any
    }

}