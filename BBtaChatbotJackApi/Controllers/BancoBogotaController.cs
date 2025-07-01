using Microsoft.AspNetCore.Mvc;
using BBtaChatbotJackApi.Services; // Add this using statement
using System.Threading.Tasks;
using BBtaChatbotJackApi.Context;
using BBtaChatbotJackApi.Models; // Add this using statement

namespace BBtaChatbotJackApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BancoBogotaController : ControllerBase
    {
        private readonly BancoBogotaFunciones _chatbotFunctions;
        private readonly AppDbContext _context; // Add this line

        public BancoBogotaController(BancoBogotaFunciones chatbotFunctions, AppDbContext context) // Modify constructor
        {
            _chatbotFunctions = chatbotFunctions;
            _context = context; // Assign injected context
        }


        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "Hello", "from", "BBtaChatbotJackApi" };
        }

        [HttpPost("ask")] // New endpoint for chatbot requests
        public async Task<IActionResult> Ask([FromBody] ChatRequest request)
        {
            // You would typically handle file uploads and get the file path here
            // For demonstration purposes, we'll assume a file path is provided in the request
            var chatSessionId = await GetOrCreateChatSessionId(request.SessionId); // Modify to await

            var pdfFilePath = request.PdfFilePath;
            var userQuestion = request.Question;

            var pdfText = _chatbotFunctions.ReadPdf(pdfFilePath);

            if (pdfText.StartsWith("Error:"))
            {
                return BadRequest(new { error = pdfText });
            }

            var topic = await _chatbotFunctions.GetTopicFromQuestion(chatSessionId, pdfText, userQuestion);

            return Ok(new { topic = topic });
        }

        [HttpPost("askQuestion")]
        public async Task<IActionResult> AskQuetion([FromBody] ChatRequest request)
        {
            // Get or create chat session (implement this method)
            var chatSessionId = await GetOrCreateChatSessionId(request.SessionId); // Modify to await

            // Retrieve file information from the database
            var fileInfo = await _context.FileInfos.FindAsync(request.FileId);
            if (fileInfo == null)
            {
                return NotFound(new { error = "File not found." });
            }

            var fileText = _chatbotFunctions.ReadFile(fileInfo.FilePath, fileInfo.FileType); // Call ReadFile

            if (fileText.StartsWith("Error:"))
            {
                // Update file status to error in the database if needed
                return BadRequest(new { error = fileText });
            }

            var topic = await _chatbotFunctions.GetTopicFromQuestion(chatSessionId, fileText, request.Question); // Pass chatSessionId

            return Ok(new { topic = topic, sessionId = chatSessionId }); // Return session ID
        }

        // Example method to get or create a session ID (needs AppDbContext injection)
        private async Task<int> GetOrCreateChatSessionId(int? sessionId)
        {
            if (sessionId.HasValue)
            {
                // In a real app, check if this session ID exists in the DB
                // If it exists, return it; otherwise, create a new one and return its ID
                var existingSession = await _context.ChatSessions.FindAsync(sessionId.Value);
                if (existingSession != null)
                {
                    return existingSession.Id;
                }
            }

            // Create a new chat session in the database
            var newSession = new ChatSession { StartTime = DateTime.UtcNow };
            _context.ChatSessions.Add(newSession);
            await _context.SaveChangesAsync(); // Save to get the new session's ID
            return newSession.Id;
        }


    }

      
}
