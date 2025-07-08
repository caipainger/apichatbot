using System.Threading.Tasks;
using System.Collections.Generic;

namespace BBtaChatbotJackApi.Services
{
    public class EmbeddingService
    {
        public EmbeddingService()
        {
            // Inicialización si es necesaria
        }

        public async Task<float[]> GenerateEmbeddingAsync(string text)
        {
            return await Task.FromResult(new float[1536]); // Placeholder embedding
        }

        public async Task<List<float[]>> GenerateEmbeddingsAsync(List<string> texts)
        {
            var embeddings = new List<float[]>();
            foreach (var text in texts)
            {
                embeddings.Add(await GenerateEmbeddingAsync(text));
            }
            return embeddings;
        }
    }
}