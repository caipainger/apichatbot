using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

using BBtaChatbotJackApi.Services; // Import the Services namespace
namespace BBtaChatbotJackApi.Middleware
{
    public class ApiKeyMiddleware
    {
        private readonly RequestDelegate _next;
        private const string APIKEYNAME = "X-API-Key"; // Or whatever your API key header name is

        public ApiKeyMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IApiKeyValidatorService apiKeyValidatorService) // Inject the service
        {
            if (!context.Request.Headers.TryGetValue(APIKEYNAME, out var extractedApiKey))
            {
                context.Response.StatusCode = 401; // Unauthorized
                await context.Response.WriteAsync("API Key was not provided.");
                return;
            }
            if (!apiKeyValidatorService.IsValid(extractedApiKey)) // Use the service to validate
            {
                context.Response.StatusCode = 401; // Unauthorized
                await context.Response.WriteAsync("Invalid API Key.");
                return;
            }
            await _next(context);
        }
    }
}
