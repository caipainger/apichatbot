using UglyToad.PdfPig;
using NPOI.SS.UserModel;
using NPOI.HSSF.UserModel; // Para archivos .xls (Excel 97-2003)
using NPOI.XSSF.UserModel; // Para archivos .xlsx (Excel 2007 y posteriores)
using System.IO;
using System.Text; // Para usar StringBuildercsharp
using BBtaChatbotJackApi.Context; // Ajusta el namespace si es diferente
using BBtaChatbotJackApi.Models; // Ajusta el namespace si es diferente
using Microsoft.EntityFrameworkCore; // Necesario para DbContext
using BBtaChatbotJackApi.Services;


namespace BBtaChatbotJackApi.Services
{
    public class FileProcessorService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly EmbeddingService _embeddingService;
        private readonly NamedEntityRecognitionService _nerService; // Añadir este
        private readonly IntentDetectionService _intentService;   // Añadir este

        public FileProcessorService(
        AppDbContext context,
        IConfiguration configuration,
        EmbeddingService embeddingService,
        NamedEntityRecognitionService nerService, // Añadir este parámetro
        IntentDetectionService intentService     // Añadir este parámetro
        )
        {
            _context = context;
            _configuration = configuration;
            _embeddingService = embeddingService;
            _nerService = nerService;             // Inicializar campo
            _intentService = intentService;       // Inicializar campo
        }

        public async Task<List<string>> ProcessUploadFolderAsync()
        {
            var results = new List<string>();
            var uploadFolderPath = _configuration["UPLOAD_FOLDER"];

            if (string.IsNullOrEmpty(uploadFolderPath))
            {
                results.Add("Error: UPLOAD_FOLDER configuration is missing.");
                return results;
            }

            if (!Directory.Exists(uploadFolderPath))
            {
                results.Add($"Error: Upload folder not found at {uploadFolderPath}");
                return results;
            }

            var files = Directory.GetFiles(uploadFolderPath);

            if (files.Length == 0)
            {
                results.Add("No files found in the upload folder.");
                return results;
            }

            foreach (var filePath in files)
            {
                var result = await ProcessFileAsync(filePath);
                results.Add(result); // Add the result message for each file
            }

            return results;
        }

        public async Task<string> ProcessFileAsync(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return "Error: File path is null or empty.";
            }

            string extension = Path.GetExtension(filePath).ToLowerInvariant();
            string text = string.Empty;

            switch (extension)
            {
                case ".txt":
                    text = await ProcessPlainAsync(filePath);
                    break;
                case ".xls":
                case ".xlsx":
                    text = await ProcessExcelAsync(filePath);
                    break;
                case ".csv":
                    text = await ProcessCsvAsync(filePath);
                    break;
                case ".doc":
                case ".docx":
                    // Implement Word processing or handle as unsupported
                    text = "Error: Word document processing not fully implemented."; // Placeholder
                    break;
                case ".pdf":
                    text = await ProcessPdfAsync(filePath);
                    break;
                default:
                    return "Error: Unsupported file format.";
            }

            // Verificar si hubo un error en el procesamiento específico
            if (text.StartsWith("Error:"))
            {
                return text;
            }

            // Simple text chunking (split by paragraphs or a fixed size)
            // This is a basic example; you might need a more sophisticated chunking strategy
            var textChunks = SplitTextIntoChunks(text, 1000); // Example: Split into chunks of ~1000 characters
            var chunkIndex = 0;

            if (textChunks.Count == 0)
            {
                return "Warning: No processable text found in the file.";
            }

            try
            {
                var fileProcessingResults = new List<string>();

                foreach (var chunk in textChunks)
                {
                    // Generate embedding for the chunk
                    float[] embedding = await _embeddingService.GenerateEmbeddingAsync(chunk);

                    // Create a new FileInfo object for the chunk
                    var fileInfoChunk = new Models.FileInfoModel // Use Models.FileInfo to avoid ambiguity
                    {
                        FileName = Path.GetFileName(filePath),
                        FilePath = filePath,
                        OriginalFilePath = filePath,
                        TextChunk = chunk,
                        Embedding = embedding,
                        UploadDate = DateTime.UtcNow,
                        FileType = extension
                    };
                    var identifiedEntities = await _nerService.IdentifyEntitiesAsync(chunk);
                    var detectedIntent = await _intentService.DetectIntentAsync(chunk);

                    // TODO: Añadir using statement para la librería de serialización JSON si aún no está presente
                    // Ejemplo: using Newtonsoft.Json;

                    // Serializar los resultados a string JSON
                    var embeddingJson = Newtonsoft.Json.JsonConvert.SerializeObject(embedding);
                    var entitiesJson = Newtonsoft.Json.JsonConvert.SerializeObject(identifiedEntities);
                    var intentJson = Newtonsoft.Json.JsonConvert.SerializeObject(detectedIntent); // O solo detectedIntent si ya es string
                    var documentChunk = new DocumentChunk
                    {
                        OriginalFilePath = filePath,
                        // Puedes añadir ChunkIndex si decides implementarlo en SplitTextIntoChunks
                        ChunkIndex = textChunks.IndexOf(chunk), // Ejemplo simple de ChunkIndex
                        Content = chunk,
                        EmbeddingVector = embeddingJson, // Guardar el embedding serializado
                        IdentifiedEntities = entitiesJson, // Placeholder por ahora
                        IdentifiedIntents = intentJson, // Placeholder por ahora
                        ProcessedTimestamp = DateTime.UtcNow
                    };
                    // Add the chunk info to the database context
                    _context.FileInform.Add(fileInfoChunk);
                    _context.DocumentChunks.Add(documentChunk);
                    fileProcessingResults.Add($"Processed chunk from {Path.GetFileName(filePath)}");
                    fileProcessingResults.Add($"Processed chunk {documentChunk.ChunkIndex} from {Path.GetFileName(filePath)}"); // Actualizar mensaje
                    chunkIndex++;
                }

                // Save all chunk info for this file to the database
                await _context.SaveChangesAsync();

                return $"Successfully processed {textChunks.Count} chunks from file: {Path.GetFileName(filePath)}";
            }
            catch (Exception ex)
            {
                return $"Error processing or saving chunks for {Path.GetFileName(filePath)}: {ex.Message}";
            }
        }

        // Add a simple text chunking method (you can refine this)
        private List<string> SplitTextIntoChunks(string text, int maxChunkSize)
        {
            var chunks = new List<string>();
            if (string.IsNullOrEmpty(text))
            {
                return chunks;
            }

            // Simple split by paragraphs (adjust delimiters as needed)
            var paragraphs = text.Split(new[] { "\\r\\n\\r\\n", "\\n\\n" }, StringSplitOptions.RemoveEmptyEntries);

            var currentChunk = new StringBuilder();
            foreach (var paragraph in paragraphs)
            {
                if (currentChunk.Length + paragraph.Length + Environment.NewLine.Length > maxChunkSize)
                {
                    if (currentChunk.Length > 0)
                    {
                        chunks.Add(currentChunk.ToString());
                        currentChunk.Clear();
                    }
                    // If a single paragraph is larger than maxChunkSize, you might need to split it further
                    // For simplicity here, we just add the large paragraph as a single chunk
                    if (paragraph.Length > maxChunkSize)
                    {
                        chunks.Add(paragraph);
                    }
                    else
                    {
                        currentChunk.Append(paragraph + Environment.NewLine);
                    }
                }
                else
                {
                    currentChunk.Append(paragraph + Environment.NewLine);
                }
            }

            if (currentChunk.Length > 0)
            {
                chunks.Add(currentChunk.ToString());
            }

            return chunks;
        }



        public async Task<string> ProcessPdfAsync(string filePath)
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
                    return text;
                }
            }
            catch (Exception ex)
            {
                return $"Error reading PDF file: {ex.Message}";
            }
        }

        public async Task<string> ProcessPlainAsync(string filePath)
        {
            if (!System.IO.File.Exists(filePath))
            {
                return "Error: File not found.";
            }
            try
            {
                var text = await System.IO.File.ReadAllTextAsync(filePath);
                return text;
            }
            catch (Exception ex)
            {
                return $"Error reading TXT file: {ex.Message}";
            }
        }

        public async Task<string> ProcessExcelAsync(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return "Error: File not found.";
            }

            try
            {
                IWorkbook workbook;
                using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    // Determinar el tipo de archivo de Excel y abrir el workbook
                    if (filePath.EndsWith(".xls"))
                    {
                        workbook = new HSSFWorkbook(fileStream);
                    }
                    else if (filePath.EndsWith(".xlsx"))
                    {
                        workbook = new XSSFWorkbook(fileStream);
                    }
                    else
                    {
                        return "Error: Unsupported Excel file format. Only .xls and .xlsx are supported.";
                    }
                }

                StringBuilder excelData = new StringBuilder();

                // Recorrer las hojas del libro
                for (int i = 0; i < workbook.NumberOfSheets; i++)
                {
                    ISheet sheet = workbook.GetSheetAt(i);
                    if (sheet != null)
                    {
                        excelData.AppendLine($"Sheet: {sheet.SheetName}");

                        // Recorrer las filas de la hoja
                        for (int rowNum = sheet.FirstRowNum; rowNum <= sheet.LastRowNum; rowNum++)
                        {
                            IRow row = sheet.GetRow(rowNum);
                            if (row == null) continue;

                            // Recorrer las celdas de la fila
                            for (int cellNum = row.FirstCellNum; cellNum < row.LastCellNum; cellNum++)
                            {
                                ICell cell = row.GetCell(cellNum);
                                if (cell == null)
                                {
                                    excelData.Append("\\t"); // Separar celdas con tabulación
                                    continue;
                                }

                                // Obtener el valor de la celda según su tipo
                                switch (cell.CellType)
                                {
                                    case CellType.String:
                                        excelData.Append(cell.StringCellValue + "\\t");
                                        break;
                                    case CellType.Numeric:
                                        // Aquí puedes formatear los números si es necesario
                                        excelData.Append(cell.NumericCellValue + "\\t");
                                        break;
                                    case CellType.Boolean:
                                        excelData.Append(cell.BooleanCellValue + "\\t");
                                        break;
                                    case CellType.Formula:
                                        // Evaluar la fórmula o obtener la fórmula como texto
                                        try
                                        {
                                            excelData.Append(cell.StringCellValue + "\\t"); // Intenta obtener el valor de la fórmula como string
                                        }
                                        catch
                                        {
                                            excelData.Append(cell.CellFormula + "\\t"); // Si falla, obtiene la fórmula como texto
                                        }
                                        break;
                                    case CellType.Blank:
                                        excelData.Append("\\t");
                                        break;
                                    default:
                                        excelData.Append("\\t");
                                        break;
                                }
                            }
                            excelData.AppendLine(); // Nueva línea para la siguiente fila
                        }
                        excelData.AppendLine(); // Línea en blanco entre hojas para mayor claridad
                    }
                }

                // Aquí puedes añadir lógica para guardar la información del archivo en la base de datos si es necesario
                return excelData.ToString();
            }
            catch (Exception ex)
            {
                return $"Error reading Excel file: {ex.Message}";
            }
        }

        //public async Task<string> ProcessPlainAsync(string filePath)
        //{
        //    if (!System.IO.File.Exists(filePath))
        //    {
        //        return "Error: File not found.";
        //    }
        //    try
        //    {
        //        using (var document = PdfDocument.Open(filePath))
        //        {
        //            var text = string.Join(" ", document.GetPages().Select(page => page.Text));
        //            // Here you can add logic to save the file information to the database if needed
        //            return text;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return $"Error reading PDF: {ex.Message}";
        //    }
        //}

        public async Task<string> ProcessCsvAsync(string filePath)
        {
            if (!System.IO.File.Exists(filePath))
            {
                return "Error: File not found.";
            }
            try
            {
                var lines = await System.IO.File.ReadAllLinesAsync(filePath);
                StringBuilder csvData = new StringBuilder();

                foreach (var line in lines)
                {
                    // Simple split by comma, considers no escaped commas within fields
                    var fields = line.Split(',');
                    csvData.AppendLine(string.Join("\t", fields)); // Separate fields with tab and lines with newline
                }
                // Here you can add logic to save the file information to the database if needed
                return csvData.ToString();
            }
            catch (Exception ex)
            {
                return $"Error reading CSV file: {ex.Message}";
            }
        }
    }
}
