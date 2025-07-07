using UglyToad.PdfPig;
using NPOI.SS.UserModel;
    using NPOI.HSSF.UserModel; // Para archivos .xls (Excel 97-2003)
    using NPOI.XSSF.UserModel; // Para archivos .xlsx (Excel 2007 y posteriores)
    using System.IO;
    using System.Text; // Para usar StringBuildercsharp
    using BBtaChatbotJackApi.Context; // Ajusta el namespace si es diferente
    using BBtaChatbotJackApi.Models; // Ajusta el namespace si es diferente
    using Microsoft.EntityFrameworkCore; // Necesario para DbContext


namespace BBtaChatbotJackApi.Services
{
    public class FileProcessorService
    {
     
     
    public FileProcessorService(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<List<string>> ProcessUploadFolderAsync()
    {
        var results = new List<string>();
        var uploadFolderPath = _configuration[\"UPLOAD_FOLDER\"];

        if (string.IsNullOrEmpty(uploadFolderPath))
        {
            results.Add(\"Error: UPLOAD_FOLDER configuration is missing.\");
            return results;
        }

        if (!Directory.Exists(uploadFolderPath))
        {
            results.Add($\"Error: Upload folder not found at {uploadFolderPath}\");
            return results;
        }

        var files = Directory.GetFiles(uploadFolderPath);

        if (files.Length == 0)
        {
            results.Add(\"No files found in the upload folder.\");
            return results;
        }

        foreach (var filePath in files)
        {
            var result = await ProcessFileAsync(filePath);
            results.Add($\"Processing \\\"{Path.GetFileName(filePath)}\\\": {result}\");
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
            string errorMessage = string.Empty;

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
                    text = await ProcessWordAsync(filePath);
                    break;
                case ".pdf":
                    text = await ProcessPdfAsync(filePath);
                    break;
                default:
                    return "Error: Unsupported file format."; // Formato no soportado, salir temprano
            }

            // Verificar si hubo un error en el procesamiento específico
            if (text.StartsWith("Error:"))
            {
                return text; // Devolver el error del procesamiento específico
            }

            try
            {
                // Crear un objeto FileInfo con los datos extraídos
                var fileInfo = new FileInfo
                {
                    FileName = Path.GetFileName(filePath),
                    FilePath = filePath, // Opcional, guarda la ruta si es relevante
                    Content = text,
                    UploadDate = DateTime.UtcNow, // O usa DateTime.Now si prefieres la hora local
                    FileType = extension // Guarda la extensión del archivo
                };

                // Guardar en la base de datos
                _context.FileInfos.Add(fileInfo);
                await _context.SaveChangesAsync(); // Usa SaveChangesAsync para no bloquear el hilo

                return "File processed and information saved successfully."; // O puedes devolver el texto procesado si lo necesitas
            }
            catch (Exception ex)
            {
                // Manejar errores al guardar en la base de datos
                return $"Error saving file information to database: {ex.Message}";
            }
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

        public async Task<string> ProcessPlainAsync(string filePath)
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
                    // Here you can add logic to save the file information to the database if needed
                    return text;
                }
            }
            catch (Exception ex)
            {
                return $"Error reading PDF: {ex.Message}";
            }
        }

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
