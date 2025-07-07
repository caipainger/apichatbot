using System;

namespace BBtaChatbotJackApi.Models
{
    public class FileInfo
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; } // Opcional: si quieres guardar la ruta
        public string Content { get; set; } // Para almacenar el texto extraído
        public DateTime UploadDate { get; set; }
        public string FileType { get; set; } // Para almacenar la extensión o tipo

        // Puedes añadir otras propiedades si son necesarias para tu modelo
    }
}