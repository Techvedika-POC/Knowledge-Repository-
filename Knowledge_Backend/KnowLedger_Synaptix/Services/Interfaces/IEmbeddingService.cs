using System.Threading.Tasks;

namespace KnowLedger_Synaptix.Services.Interfaces
{
    public interface IEmbeddingService
    {
        /// <summary>
        /// Generates an embedding vector for the given text.
        /// </summary>
        /// <param name="text">The input text to embed.</param>
        /// <returns>A float array representing the embedding vector.</returns>
        Task<float[]> GetEmbeddingAsync(string text);
    }
}
