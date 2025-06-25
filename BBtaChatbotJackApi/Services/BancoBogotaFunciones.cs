using System;
using System.Threading.Tasks;
using PdfPig; // Add this using statement
using System.Linq; // Add this using statement

using OpenAI.GPT3.Managers; // Add this using statement
using OpenAI.GPT3.Model; // Add this using statement
using OpenAI.GPT3.ObjectModels.RequestModels; // Add this using statement

namespace BBtaChatbotJackApi.Services
{
    public class BancoBogotaFunciones
    {
        private readonly OpenAIService _openAIService;

    private readonly AppDbContext _context;

       
    public BancoBogotaFunciones(AppDbContext context)
    {
        _context = context;
        // Initialize OpenAI service (get API key from configuration)
        var apiKey = "YOUR_OPENAI_API_KEY"; // Replace with your actual API key from configuration
        _openAIService = new OpenAIService(new OpenAI.GPT3.OpenAiOptions()
        {
            ApiKey = apiKey
        });
    }

      

        // Add methods for PDF reading and AI interaction here

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


        public async Task<string> GetTopicFromQuestion(string pdfText, string userQuestion)
        {
            try
            {
                var completionResult = await _openAIService.Completions.CreateCompletionAsync(new CompletionCreateRequest()
                {
                    Prompt = $"Based on the following text, what is the main topic of the question?\n\nText: {pdfText}\n\nQuestion: {userQuestion}\n\nTopic:",
                    Model = Models.TextDavinciV3, // Or another suitable model
                    MaxTokens = 50 // Adjust as needed
                });

                if (completionResult.Successful)
                {
                    return completionResult.Choices.FirstOrDefault()?.Text.Trim();
                }
                else
                {
                    return $"Error from OpenAI API: {completionResult.Error?.Message}";
                }
            }
            catch (Exception ex)
            {                  
                return $"Error calling OpenAI API: {ex.Message}";
            }
        }
        csharp
    // Inside BBtaChatbotJackApi/Services/BancoBogotaFunciones.cs
    // Rename this method from ReadPdf
    public string ReadFile(string filePath, string fileType) // Update signature
    {
        if (!System.IO.File.Exists(filePath))
        {
            return "Error: File not found.";
        }

        try
        {
            switch (fileType.ToLower()) // Add switch statement
            {
                case "pdf":
                    // Use PdfPig to read PDF
                    using (var document = PdfDocument.Open(filePath))
                    {
                        return string.Join(" ", document.GetPages().Select(page => page.Text));
                    }
                case "docx":
                    // Add DOCX reading logic (using a library like DocX)
                    return "DOCX reading not implemented yet."; // Placeholder
                case "txt":
                    // Read plain text file
                    return System.IO.File.ReadAllText(filePath);
                // Add cases for other file types (doc, xml, xlsx, csv) and their reading logic
                default:
                    return $"Error: File type '{fileType}' not supported.";
            }
        }
        catch (Exception ex)
        {
            // Log the error
            return $"Error reading file: {ex.Message}";
        }
    }
    csharp
    // Inside BBtaChatbotJackApi/Services/BancoBogotaFunciones.cs
    public async Task<string> GetTopicFromQuestion(int chatSessionId, string pdfText, string userQuestion) // Update signature
    {
        try
        {
            // Retrieve the chat session
            var chatSession = await _context.ChatSessions.FindAsync(chatSessionId);
            if (chatSession == null)
            {
                return "Error: Chat session not found.";
            }

            // ... (rest of your existing code to call OpenAI API)

            if (completionResult.Successful)
            {
                var identifiedTopic = completionResult.Choices.FirstOrDefault()?.Text.Trim();

                // Save user message
                var userMessage = new Message
                {
                    ChatSessionId = chatSessionId,
                    Sender = "User",
                    Content = userQuestion,
                    Timestamp = DateTime.UtcNow,
                    IdentifiedTopic = null
                };
                _context.Messages.Add(userMessage);

                // Save AI response message
                var aiMessage = new Message
                {
                    ChatSessionId = chatSessionId,
                    Sender = "AI",
                    Content = identifiedTopic,
                    Timestamp = DateTime.UtcNow,
                    IdentifiedTopic = identifiedTopic
                };
                _context.Messages.Add(aiMessage);

                await _context.SaveChangesAsync(); // Save both messages

                return identifiedTopic;
            }
            else
            {
                // ... (error handling)
            }
        }
        catch (Exception ex)
        {
            // ... (error handling)
        }
    }


    }
}
