<<<<<<< HEAD

 using AllMiniLmL6V2Sharp;
=======
using System.Threading.Tasks;
using System.Collections.Generic;
 using AllMiniLmL6V2Sharp;
using Tensorflow; // Add this using statement
using static Tensorflow.Binding; // Add this using statement
>>>>>>> b2fb8b33c7ea4efb29b679e9009ce2bb8be5a236

namespace BBtaChatbotJackApi.Services
{
    public class EmbeddingService
    {
        private Session _session; // Add private field for TensorFlow session
        private Graph _graph;
        private Operation _inputOp;
        private Operation _outputOp;
        private object _tokenizer;
        private readonly AllMiniLmL6V2Embedder _model;

        public EmbeddingService()
        {
            // Inicialización si es necesaria
            tf.compat.v1.disable_eager_execution(); // Ensure eager execution is disabled for session
            _session = tf.Session(); // Initialize TensorFlow session
            _graph = new Graph().as_default();
            _model = new AllMiniLmL6V2Embedder();
            // var modelPath = "ruta/a/tu/modelo/bert"; // Debes tener el modelo descargado localmente
            // (_inputOp, _outputOp) = LoadBertModel(modelPath, _graph);
            var placeholder = tf.placeholder(tf.float32, shape: (-1, 128), name: "input_placeholder");
            var output_op = tf.identity(placeholder, name: "output_identity"); // Operación de salida simple

            _session.run(tf.global_variables_initializer());
        }

<<<<<<< HEAD
        //public async Task<float[]> GenerateEmbeddingAsync(string text)
        //{
        //    return await Task.FromResult(new float[768]); // Placeholder embedding using a common BERT embedding size
        //}
=======
        public async Task<float[]> GenerateEmbeddingAsync(string text)
        {
            return await Task.FromResult(new float[768]); // Placeholder embedding using a common BERT embedding size
        }
>>>>>>> b2fb8b33c7ea4efb29b679e9009ce2bb8be5a236

        // public async Task<List<float[]>> GenerateEmbeddingsAsync(List<string> texts)
        // {
        //     var embeddings = new List<float[]>();
        //     var tokenizedInput = TokenizeText(text);
        //     var inputTensors = PrepareInputTensors(tokenizedInput);
        //     // TODO: Implementar la tokenización del texto
        // // Esto dependerá del tokenizador que elijas y cómo lo inicialices.
        // // var tokenizedInput = TokenizeText(text);

        // // TODO: Preparar los tensores de entrada para el modelo
        // // Esto implica convertir los tokens y otros datos necesarios (attention masks, segment IDs)
        // // en tensores de TensorFlow.
        // // var inputTensors = PrepareInputTensors(tokenizedInput);

        // // TODO: Ejecutar la inferencia del modelo
        // // Esto implica usar _session.run() para pasar los tensores de entrada
        // // a las operaciones de entrada del modelo y obtener las salidas.
        // // var outputTensors = _session.run(_outputOp, new FeedItem(_inputOp, inputTensorData));

        // // TODO: Extraer el vector de embedding de las salidas del modelo
        // // Para BERT, a menudo se toma el vector del token [CLS] y/o se realiza pooling.
        // // var embedding = ExtractEmbeddingVector(outputTensors);
        // return await Task.Run(() => _model.Predict(text));
        // // Placeholder para que compile:
        // Console.WriteLine($"Generating placeholder embedding for: {text}");
        // //return 
        // await Task.FromResult(new float[768]); // Ajusta el tamaño según el modelo BERT real
    
        //     foreach (var text in texts)
        //     {
        //         embeddings.Add(await GenerateEmbeddingAsync(text));
        //     }
        //     return embeddings;
        // }
         public async Task<float[]> GenerateEmbeddingAsync(string text)
        {
             return await Task.Run(() => _model.GenerateEmbedding(text)); // Usar GenerateEmbedding
        }

        public async Task<List<float[]>> GenerateEmbeddingsAsync(List<string> texts)
        {
             // Usa el método GenerateEmbeddings si existe
             return await Task.Run(() => _model.GenerateEmbeddings(texts).ToList()); // Usar GenerateEmbeddings
        }

        public void Dispose()
        {
            _model?.Dispose();
        }
    }
}