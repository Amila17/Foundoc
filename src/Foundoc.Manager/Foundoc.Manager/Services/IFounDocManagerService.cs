using System.Collections.Generic;
using System.Threading.Tasks;
using Foundoc.Manager.Models;

namespace Foundoc.Manager.Services
{
    public interface IFoundocManagerService
    {
        Task<Document> SaveDocumentAsync(Document document);

        Task<IEnumerable<string>> GetDocumentTypesAsync();

        Task<IEnumerable<string>> GetIndexesAndKeysAsync(string documentType);

        Task<IEnumerable<string>> GetIndexFieldsAsync(string documentType, string indexName);

        Task<IDictionary<string, string>> GetMetadataAsync(string documentTypeName);

        //For phase one, the GetDocumentRecords will only return one item as FounDoc only allow unique contraint indexes.
        Task<object> GetDocumentRecords(string documentTypeName, string indexName, IEnumerable<Field> fields);

        Task DeleteDocumentRecords(string documentTypeName, string indexName, IEnumerable<Field> fields);
    }
}
