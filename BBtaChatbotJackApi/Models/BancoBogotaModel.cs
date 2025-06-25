namespace BBtaChatbotJackApi.Models
{
    public class BancoBogotaModel
    {
        // Add properties here
    }
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
