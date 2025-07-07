using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace BBtaChatbotJackApi.Models
{
    public class FileInfoModel
    {
        public int Id { get; set; }
        public string FileName { get; set; } // Store the name of the original file
        public string OriginalFilePath { get; set; } // Optional: if you want to save the path of the original file
        public string TextChunk { get; set; } // Store the text chunk
        [Column(TypeName = "json")]
        public float[] Embedding { get; set; } // Store the embedding vector
        public DateTime UploadDate { get; set; }
        public string FileType { get; set; } // Para almacenar la extensión o tipo

        // Puedes añadir otras propiedades si son necesarias para tu modelo
    }
}