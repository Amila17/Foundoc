using System.Collections.Generic;
using System.Linq;
using Foundoc.Manager.Models;
using Foundoc.Manager.Services;
using Nancy;
using Nancy.ModelBinding;
using Newtonsoft.Json;

namespace Foundoc.Manager.Modules
{
    public class FoundocManagerApiModule : NancyModule
    {
        private readonly IFoundocManagerService _foundocManagerService;

        public FoundocManagerApiModule(IFoundocManagerService foundocManagerService)
        {
            _foundocManagerService = foundocManagerService;

            Get["foundocmanager/api/documents", true] = async (x, ctx) => Response.AsJson(await _foundocManagerService.GetDocumentTypesAsync());

            Get["foundocmanager/api/document/{documentType}/indexes", true] = async (x, ctx) =>
            {
                var keysAndIndexes = await _foundocManagerService.GetIndexesAndKeysAsync(x.documentType);
                return FormatterExtensions.AsJson(Response, keysAndIndexes);
            };

            Get["foundocmanager/api/document/{documentType}/index/{indexName}/fields", true] = async (x, ctx) =>
            {
                var fields = await _foundocManagerService.GetIndexFieldsAsync(x.documentType, x.indexName);

                if (fields == null)
                {
                    return Response.AsJson("", HttpStatusCode.BadRequest);
                }

                return FormatterExtensions.AsJson(Response, fields);
            };

            Get["foundocmanager/api/document/{documentType}/metadata", true] = async (x, ctx) =>
            {
                var metadata = await _foundocManagerService.GetMetadataAsync(x.documentType);
                return FormatterExtensions.AsJson(Response, metadata);
            };

            Post["foundocmanager/api/document/{documentType}/index/{index}/search", true] = async (x, ctx) =>
            {
                var fields = this.Bind<IEnumerable<Field>>();

                var result = await _foundocManagerService.GetDocumentRecords(x.documentType, x.index, fields);

                if (result != null)
                {
                    var value = JsonConvert.SerializeObject(result, Formatting.Indented);

                    var documents = new List<Document>
                    {
                        new Document
                        {
                            DocumentId = ExtractId(x.documentType, fields),
                            DocumentType = x.documentType,
                            Value = value
                        }
                    };

                    return Response.AsJson(documents);
                }

                return Response.AsJson("", HttpStatusCode.NotFound);
            };

            Post["foundocmanager/api/document/{documentType}/index/{index}/delete", true] = async (x, ctx) =>
            {
                var fields = this.Bind<IEnumerable<Field>>();

                var result = await _foundocManagerService.DeleteDocumentRecords(x.documentType, x.index, fields);

                if (result != null)
                {
                    return FormatterExtensions.AsJson(Response, result);
                }

                return Response.AsJson("", HttpStatusCode.NotFound);
            };

            Post["foundocmanager/api/document/{documentType}/update", true] = async (x, ctx) =>
            {
                var document = this.Bind<Document>();

                var item = await _foundocManagerService.SaveDocumentAsync(document);

                if (item != null)
                {
                    return Response.AsJson(item);
                }

                return Response.AsJson("", HttpStatusCode.NotFound);
            };

            
        }

        private string ExtractId(string documentType, IEnumerable<Field> fields)
        {
            var idElements = new List<string>
            {
                documentType
            };
            
            idElements.AddRange(fields.Select(field => field.Value).ToList());

            var extractedId = string.Join("/", idElements);

            return extractedId;
        }     
    }
}
