namespace BBtaChatbotJackApi.Services
{
    public interface IApiKeyValidatorService
    {
        bool IsValid(string apiKey);
    }

    public class ApiKeyValidatorService : IApiKeyValidatorService
    {
        // In a real application, you would fetch the valid API key from configuration or a secure store
        private const string VALIDAPIKEY = "YOUR_SECRET_API_KEY"; // Replace with your actual secret key

        public bool IsValid(string apiKey)
        {
            // In a real application, you might want to use a more secure comparison method
            return VALIDAPIKEY.Equals(apiKey);
        }
    }
}
