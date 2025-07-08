using System;
using System.Threading.Tasks;
using UglyToad.PdfPig;
using System.Linq;
using Betalgo.Ranul.OpenAI; // Paquete actualizado
using Betalgo.Ranul.OpenAI.ObjectModels.RealtimeModels;
using Betalgo.Ranul.OpenAI.ObjectModels.RequestModels;
using BBtaChatbotJackApi.Context;
using Betalgo.Ranul.OpenAI.Managers;
using Betalgo.Ranul.OpenAI.ObjectModels.SharedModels;
using BBtaChatbotJackApi.Models;
using System.Text;

namespace BBtaChatbotJackApi.Services
{
    public class BancoBogotaFunciones
    {
        private readonly OpenAIService _openAIService;
        private readonly AppDbContext _context;
        private readonly EmbeddingService _embeddingService;
        private readonly BancoBogotaDAO _bancoBogotaDAO;

        public BancoBogotaFunciones(AppDbContext context, EmbeddingService embeddingService, BancoBogotaDAO bancoBogotaDAO)
        {
            _context = context;
            _embeddingService = embeddingService;
            _bancoBogotaDAO = bancoBogotaDAO;

            // Initialize OpenAI service with the new package
            var apiKey = "YOUR_OPENAI_API_KEY"; // Reemplaza con tu API key real
            _openAIService = new OpenAIService(new OpenAIOptions()
            {
                ApiKey = apiKey,
                DefaultModelId = "gpt-3.5-turbo" //Model.Gpt_3_5_Turbo // Modelo por defecto actualizado
            });
        }

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
                    var pdfDocument = new PdfDocuments
                    {
                        FileName = System.IO.Path.GetFileName(filePath),
                        FilePath = filePath,
                        UploadDate = DateTime.UtcNow,
                        Status = "Processed"
                    };
                    _context.PdfDocuments.Add(pdfDocument);
                    _context.SaveChanges();

                    return text;
                }
            }
            catch (Exception ex)
            {
                return $"Error reading PDF: {ex.Message}";
            }
        }

        public async Task<string> GetTopicFromQuestion(int? idchat, string pdfText, string userQuestion)
        {
            try
            {
                var completionResult = await _openAIService.ChatCompletion.CreateCompletion(new ChatCompletionCreateRequest
                {
                    Messages = new List<ChatMessage>
                    {
                        ChatMessage.FromSystem("You are a helpful assistant that identifies topics in text."),
                        ChatMessage.FromUser($"Based on the following text, what is the main topic of the question?\n\nText: {pdfText}\n\nQuestion: {userQuestion}")
                    },
                    Model = "gpt-3.5-turbo", // Modelo actualizado
                    MaxTokens = 50
                });

                if (completionResult.Successful)
                {
                    return completionResult.Choices.FirstOrDefault()?.Message.Content?.Trim();
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

        public string ReadFile(string filePath, string fileType)
        {
            if (!System.IO.File.Exists(filePath))
            {
                return "Error: File not found.";
            }

            try
            {
                switch (fileType.ToLower())
                {
                    case "pdf":
                        using (var document = PdfDocument.Open(filePath))
                        {
                            return string.Join(" ", document.GetPages().Select(page => page.Text));
                        }
                    case "docx":
                        return "DOCX reading not implemented yet.";
                    case "txt":
                        return System.IO.File.ReadAllText(filePath);
                    default:
                        return $"Error: File type '{fileType}' not supported.";
                }
            }
            catch (Exception ex)
            {
                return $"Error reading file: {ex.Message}";
            }
        }

        public async Task<string> GetTopicFromQuestions(int chatSessionId, string pdfText, string userQuestion)
        {
            try
            {
                var chatSession = await _context.ChatSessions.FindAsync(chatSessionId);
                if (chatSession == null)
                {
                    return "Error: Chat session not found.";
                }

                var completionResult = await _openAIService.ChatCompletion.CreateCompletion(new ChatCompletionCreateRequest
                {
                    Messages = new List<ChatMessage>
                    {
                        ChatMessage.FromSystem("You are a helpful assistant that identifies topics in text."),
                        ChatMessage.FromUser($"Based on the following text, what is the main topic of the question?\n\nText: {pdfText}\n\nQuestion: {userQuestion}")
                    },
                    Model = "gpt-3.5-turbo",
                    MaxTokens = 50
                });

                if (completionResult.Successful)
                {
                    var identifiedTopic = completionResult.Choices.FirstOrDefault()?.Message.Content?.Trim();

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

                    await _context.SaveChangesAsync();

                    return identifiedTopic;
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
        public async Task<string> GetAnswerWithContextAsync(string userQuestion)
        {
            try
            {
                // 1. Generate embedding for the user question
                float[] questionEmbedding = await _embeddingService.GenerateEmbeddingAsync(userQuestion);

                // 2. Search for relevant file chunks based on the question embedding
                List<FileInfoModel> relevantChunks = await _bancoBogotaDAO.SearchRelevantFileChunksAsync(questionEmbedding);

                // 3. Build context string from relevant chunks
                var contextBuilder = new StringBuilder();
                contextBuilder.AppendLine("Context from documents:");
                contextBuilder.AppendLine("--------------------");

                if (relevantChunks != null && relevantChunks.Any())
                {
                    foreach (var chunk in relevantChunks)
                    {
                        contextBuilder.AppendLine($"Source: {chunk.FileName}"); // Or use FilePath if more specific
                        contextBuilder.AppendLine(chunk.TextChunk);
                        contextBuilder.AppendLine("--------------------");
                    }
                }
                else
                {
                    contextBuilder.AppendLine("No relevant context found in documents.");
                    contextBuilder.AppendLine("--------------------");
                }

                // 4. Prepare messages for the OpenAI API
                var messages = new List<ChatMessage>
            {
                ChatMessage.FromSystem("You are a helpful assistant that answers questions based on the provided context. If the answer is not in the context, say that you don't have enough information."),
                ChatMessage.FromUser($"{contextBuilder.ToString()}\n\nQuestion: {userQuestion}")
            };

                // 5. Call the OpenAI API with context
                var completionResult = await _openAIService.ChatCompletion.CreateCompletion(new ChatCompletionCreateRequest
                {
                    Messages = messages,
                    Model = "gpt-3.5-turbo", // Or a more capable model if needed
                    MaxTokens = 500 // Adjust max tokens as needed for a complete answer
                });

                // 6. Return the answer
                if (completionResult.Successful)
                {
                    return completionResult.Choices.FirstOrDefault()?.Message.Content?.Trim();
                }
                else
                {
                    return $"Error from OpenAI API: {completionResult.Error?.Message}";
                }
            }
            catch (Exception ex)
            {
                return $"Error in getting answer with context: {ex.Message}";
            }
        }
    }
}