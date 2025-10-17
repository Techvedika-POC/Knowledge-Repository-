using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using KnowLedger_Synaptix.Services.Interfaces;
using KnowledgeSynaptix.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace KnowLedger_Synaptix.Services.Implementations
{
    /// <summary>
    /// Service to generate embeddings from various file types such as text, PDF, DOCX, and images.
    /// </summary>
    public class FileEmbeddingService : IFileEmbeddingService
    {
        private readonly IEmbeddingService _textEmbeddingService;

        public FileEmbeddingService(IEmbeddingService textEmbeddingService)
        {
            _textEmbeddingService = textEmbeddingService ?? throw new ArgumentNullException(nameof(textEmbeddingService));
        }

        /// <summary>
        /// Generates an embedding for a given file based on its MIME type.
        /// Supported types: text/plain, application/pdf, DOCX, PNG, JPEG.
        /// </summary>
        /// <param name="filePath">Path to the file.</param>
        /// <param name="mimeType">MIME type of the file.</param>
        /// <returns>List of floats representing the embedding.</returns>
        /// <exception cref="FileNotFoundException">Thrown if the file does not exist.</exception>
        /// <exception cref="NotSupportedException">Thrown if the MIME type is unsupported.</exception>
        public async Task<List<float>> GenerateEmbeddingAsync(string filePath, string mimeType)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                throw new FileNotFoundException("File not found.", filePath);

            string textContent = mimeType switch
            {
                "text/plain" => await File.ReadAllTextAsync(filePath),
                "application/pdf" => ExtractTextFromPdf(filePath),
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document" => ExtractTextFromDocx(filePath),
                "image/png" => await ExtractTextFromImage(filePath),
                "image/jpeg" => await ExtractTextFromImage(filePath),
                _ => throw new NotSupportedException($"Unsupported MIME type: {mimeType}")
            };

            // Generate embedding using the text embedding service
            float[] embeddingArray = await _textEmbeddingService.GetEmbeddingAsync(textContent);
            return embeddingArray.ToList();
        }

        /// <summary>
        /// Extracts all text from a PDF file.
        /// </summary>
        private string ExtractTextFromPdf(string filePath)
        {
            using var reader = new PdfReader(filePath);
            using var pdfDoc = new PdfDocument(reader);
            var text = "";

            for (int i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
            {
                text += PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(i)) + "\n";
            }

            return text;
        }

        /// <summary>
        /// Extracts all text from a DOCX (Word) document.
        /// </summary>
        private string ExtractTextFromDocx(string filePath)
        {
            using var doc = WordprocessingDocument.Open(filePath, false);
            var body = doc.MainDocumentPart?.Document?.Body;
            return body?.InnerText ?? string.Empty;
        }

        /// <summary>
        /// Placeholder method for OCR extraction from images.
        /// Currently returns an empty string until OCR is implemented.
        /// </summary>
        private async Task<string> ExtractTextFromImage(string filePath)
        {
            return await Task.FromResult(string.Empty);
        }
    }
}
