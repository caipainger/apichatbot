using UglyToad.PdfPig;

namespace BBtaChatbotJackApi.Services
{
    public class FileProcessorService
    {
        public FileProcessorService() { }

        public async Task<string> ProcessPdfAsync(string filePath)
        {
            if (!System.IO.File.Exists(filePath))
            {
                return "Error: File not found.";
            }
            try
            {
                using (var document = PdfDocument.Open(filePath))
                {
                    var text = string.Join(" ", document.GetPages().Select(page => page.Text));
                    // Here you can add logic to save the file information to the database if needed
                    return text;
                }
            }
            catch (Exception ex)
            {
                return $"Error reading PDF: {ex.Message}";
            }
        }

        public async Task<string> ProcessExcelAsync(string filePath)
        {
            if (!System.IO.File.Exists(filePath))
            {
                return "Error: File not found.";
            }
            try
            {
                using (var document = PdfDocument.Open(filePath))
                {
                    var text = string.Join(" ", document.GetPages().Select(page => page.Text));
                    // Here you can add logic to save the file information to the database if needed
                    return text;
                }
            }
            catch (Exception ex)
            {
                return $"Error reading PDF: {ex.Message}";
            }
        }

        public async Task<string> ProcessPlainAsync(string filePath)
        {
            if (!System.IO.File.Exists(filePath))
            {
                return "Error: File not found.";
            }
            try
            {
                using (var document = PdfDocument.Open(filePath))
                {
                    var text = string.Join(" ", document.GetPages().Select(page => page.Text));
                    // Here you can add logic to save the file information to the database if needed
                    return text;
                }
            }
            catch (Exception ex)
            {
                return $"Error reading PDF: {ex.Message}";
            }
        }

        public async Task<string> ProcessCsvAsync(string filePath)
        {
            if (!System.IO.File.Exists(filePath))
            {
                return "Error: File not found.";
            }
            try
            {
                using (var document = PdfDocument.Open(filePath))
                {
                    var text = string.Join(" ", document.GetPages().Select(page => page.Text));
                    // Here you can add logic to save the file information to the database if needed
                    return text;
                }
            }
            catch (Exception ex)
            {
                return $"Error reading PDF: {ex.Message}";
            }
        }
    }
}
