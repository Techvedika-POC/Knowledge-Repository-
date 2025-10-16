public interface IFileEmbeddingService
{
    Task<List<float>> GenerateEmbeddingAsync(string filePath, string mimeType);
}
