using BBtaChatbotJackApi.Models;
using Microsoft.EntityFrameworkCore;
using UglyToad.PdfPig;

namespace BBtaChatbotJackApi.Context
{
    public class BancoBogotaDAO
    {
        private readonly AppDbContext _context;

        public BancoBogotaDAO(AppDbContext context)
        {
            _context = context;
        }


        public string ReadPdf(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return "Error: File not found.";
            }

            try
            {
                using (var document = PdfDocument.Open(filePath))
                {
                    var text = string.Join(" ", document.GetPages().Select(page => page.Text));

                    // Save PDF information to the database
                    var pdfDocument = new Models.PdfDocuments
                    {
                        FileName = System.IO.Path.GetFileName(filePath),
                        FilePath = filePath,
                        UploadDate = DateTime.UtcNow,
                        Status = "Processed"
                    };
                    _context.PdfDocuments.Add(pdfDocument);
                    _context.SaveChanges(); // Save changes to the database

                    return text;
                }
            }
            catch (Exception ex)
            {
                // In a real application, you might want to log the error and handle it appropriately
                return $"Error reading PDF: {ex.Message}";
            }
        }
        public async Task<List<FileInfoModel>> SearchRelevantFileChunksAsync(float[] queryEmbedding)
        {
            var relevantChunks = await _context.FileInform
                .Take(5) // Take top N relevant chunks
                .ToListAsync(); // You'll need to order by similarity before taking

            return relevantChunks;
        }
    }
}