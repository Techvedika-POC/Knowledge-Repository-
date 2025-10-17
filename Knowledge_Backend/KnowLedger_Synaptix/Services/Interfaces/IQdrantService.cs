using System;
using System.Threading.Tasks;

namespace KnowledgeSynaptix.Services.Interfaces
{
    public interface IQdrantService
    {
        Task SaveToQdrantAsync(string id, float[] embedding, string title, string description);
    }

}
