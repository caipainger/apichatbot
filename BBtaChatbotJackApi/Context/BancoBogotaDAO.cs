csharp
    public string ReadPdf(string filePath)
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

                // Save PDF information to the database
                var pdfDocument = new Models.PdfDocument
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
