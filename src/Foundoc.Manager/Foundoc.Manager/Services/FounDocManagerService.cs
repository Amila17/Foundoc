using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Foundoc.Core;
using Foundoc.Manager.Models;
using Newtonsoft.Json;

namespace Foundoc.Manager.Services
{
    public class FoundocManagerService : IFoundocManagerService
    {
        private readonly IDocumentProvider _documentProvider;
        private readonly IFoundocDocumentStore _documentStore;
        private readonly IIndexProvider _indexProvider;

        public FoundocManagerService(IFoundocDocumentStore documentStore)
        {
            _documentStore = documentStore;
            _documentProvider = documentStore.DocumentProvider;
            _indexProvider = documentStore.IndexProvider;
        }

        public async Task<Document> SaveDocumentAsync(Document document)
        {
            var documentType = _documentProvider.KeyDefinitions.FirstOrDefault(
                x => String.Equals(x.Key.Name, document.DocumentType, StringComparison.InvariantCultureIgnoreCase)).Key;

            var deserializedDocument = DeserializeDocument(document, documentType);

            StoreResult result;
            await _documentStore.OpenTransactionAsync(async transaction =>
                {
                    var storeMethod = transaction.GetType()
                        .GetMethod("StoreAsync")
                        .MakeGenericMethod(new[] { documentType });

                    result = await (Task<StoreResult>)storeMethod.Invoke(transaction, new[]
                        {
                            deserializedDocument
                        });
                });

            return document;
        }

        public Task<IEnumerable<string>> GetDocumentTypesAsync()
        {
            var documentTypes = new List<string>();
            documentTypes.AddRange(_indexProvider.IndexDefinitions.Select(x => x.Key.Name));
            documentTypes.AddRange(_documentProvider.KeyDefinitions.Select(x => x.Key.Name));

            return Task.FromResult(documentTypes.Distinct().AsEnumerable());
        }

        public Task<IEnumerable<string>> GetIndexesAndKeysAsync(string documentType)
        {
            var index = new KeyValuePair<Type, Dictionary<string, object>>();

            if (_indexProvider.IndexDefinitions.Keys.Count(x => String.Equals(x.Name, documentType, StringComparison.InvariantCultureIgnoreCase)) > 0)
            {
                index =
                    _indexProvider.IndexDefinitions.FirstOrDefault(
                        x => String.Equals(x.Key.Name, documentType, StringComparison.InvariantCultureIgnoreCase));
            }

            var keysAndIndexes = new List<string> { "ById" };

            if (index.Value != null)
            {
                keysAndIndexes.AddRange(index.Value.Keys);
            }

            return Task.FromResult(keysAndIndexes.AsEnumerable());
        }

        public async Task<IEnumerable<string>> GetIndexFieldsAsync(string documentType, string indexName)
        {
            if (String.Equals(indexName, "byid", StringComparison.InvariantCultureIgnoreCase))
            {
                var keyValues = _documentProvider.KeyDefinitions.FirstOrDefault(x => String.Equals(x.Key.Name, documentType, StringComparison.InvariantCultureIgnoreCase)).Value;

                return await GetExpressionProperties(keyValues, "Expression");
            }

            var indexValues = _indexProvider.IndexDefinitions.FirstOrDefault(x => String.Equals(x.Key.Name, documentType, StringComparison.InvariantCultureIgnoreCase)).Value;

            if (indexValues != null)
            {
                var selectedIndexValue = indexValues.FirstOrDefault(x => String.Equals(x.Key, indexName, StringComparison.InvariantCultureIgnoreCase)).Value;

                return await GetExpressionProperties(selectedIndexValue, "Map");
            }

            return await Task.FromResult(default(IEnumerable<string>));
        }

        public async Task<IDictionary<string, string>> GetMetadataAsync(string documentTypeName)
        {
            var metaData = new Dictionary<string, string>();

            var indexes = await GetIndexesAndKeysAsync(documentTypeName);
            var documentCount = await GetDocumentCount(documentTypeName);

            metaData.Add("Indexes", indexes.Count().ToString(CultureInfo.InvariantCulture));
            metaData.Add("Count", documentCount.ToString(CultureInfo.InvariantCulture));

            return metaData;
        }

        public async Task<object> GetDocumentRecords(string documentTypeName, string indexName,
            IEnumerable<Field> fields)
        {
            IEnumerable<MemberInfo> expressionMemberInfos;

            Type documentType;

            object result = null;

            if (String.Equals("ById", indexName, StringComparison.InvariantCultureIgnoreCase))
            {
                var keyDef = _documentProvider.KeyDefinitions.FirstOrDefault(x => String.Equals(x.Key.Name, documentTypeName, StringComparison.InvariantCultureIgnoreCase));

                expressionMemberInfos = await GetExpressionMemberInfo(keyDef.Value, "Expression");

                documentType = keyDef.Key;

                var keyDictionary = GenerateDocumentKey(fields, expressionMemberInfos);

                await _documentStore.OpenTransactionAsync(async transaction =>
                    {
                        var method = transaction.GetType().GetMethod("LoadAsync", new[] { typeof(IDictionary<string, object>) })
                            .MakeGenericMethod(new[] { documentType });

                        result = await (Task<object>)method.Invoke(transaction, new object[] { keyDictionary });
                    });
            }
            else
            {
                var indexDef = _indexProvider.IndexDefinitions.FirstOrDefault(x => String.Equals(x.Key.Name, documentTypeName, StringComparison.InvariantCultureIgnoreCase));

                if (indexDef.Key != null)
                {
                    var indexExpression = indexDef.Value.FirstOrDefault(x => String.Equals(x.Key, indexName, StringComparison.InvariantCultureIgnoreCase)).Value;

                    expressionMemberInfos = await GetExpressionMemberInfo(indexExpression, "Map");

                    documentType = indexDef.Key;

                    var keyDictionary = GenerateDocumentKey(fields, expressionMemberInfos);

                    await _documentStore.OpenTransactionAsync(async transaction =>
                        {
                            var method = transaction.GetType().GetMethod("GetByIndex", new[] { typeof(string), typeof(IDictionary<string, object>) })
                                .MakeGenericMethod(new[] { documentType });

                            result = await (Task<object>)method.Invoke(transaction, new object[] { indexName, keyDictionary });
                        });
                }
            }

            return result;
        }

        public async Task DeleteDocumentRecords(string documentTypeName, string indexName, IEnumerable<Field> fields)
        {
            IEnumerable<MemberInfo> expressionMemberInfos;

            Type documentType;

            if (String.Equals("ById", indexName, StringComparison.InvariantCultureIgnoreCase))
            {
                var keyDef =
                    _documentProvider.KeyDefinitions.FirstOrDefault(
                        x => String.Equals(x.Key.Name, documentTypeName, StringComparison.InvariantCultureIgnoreCase));

                expressionMemberInfos = await GetExpressionMemberInfo(keyDef.Value, "Expression");

                documentType = keyDef.Key;

                var field = fields.Single(f => String.Equals("Id", f.Name, StringComparison.InvariantCultureIgnoreCase));

                var ids = field.Value.Split(',').Select(x => x.Trim());

                foreach (var id in ids)
                {
                    var siteCodeField = fields.Single(f => String.Equals("SiteCode", f.Name, StringComparison.InvariantCultureIgnoreCase));
                    var idField = new Field
                    {
                        Name = "Id",
                        Value = id
                    };

                    var generatedFields = new List<Field>()
                    {
                        siteCodeField,
                        idField
                    };

                    var keyDictionary = GenerateDocumentKey(generatedFields, expressionMemberInfos);

                    await _documentStore.OpenTransactionAsync(async transaction =>
                    {
                        var method = transaction.GetType()
                            .GetMethod("DeleteAsync", new[] { typeof(IDictionary<string, object>) })
                            .MakeGenericMethod(new[] { documentType });

                        await (Task<StoreResult>)method.Invoke(transaction, new object[] { keyDictionary });
                    });

                }
            }
        }

        private async Task<long> GetDocumentCount(string documentTypeName)
        {
            long count = 0;

            var keyDef = _documentProvider.KeyDefinitions.FirstOrDefault(x => String.Equals(x.Key.Name, documentTypeName, StringComparison.InvariantCultureIgnoreCase));

            var documentType = keyDef.Key;

            await _documentStore.OpenTransactionAsync(async transaction =>
                {
                    var method = transaction.GetType()
                        .GetMethod("Count")
                        .MakeGenericMethod(new[] { documentType });

                    count = await (Task<long>)method.Invoke(transaction, null);
                });

            return count;
        }

        private static object DeserializeDocument(Document document, Type documentType)
        {
            try
            {
                var deserializedDocument = JsonConvert.DeserializeObject(document.Value, documentType);
                return deserializedDocument;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    string.Format("Could not convert document to Type: {0} Error: {1}", documentType.Name, ex.Message),
                    ex);
            }
        }

        private Dictionary<string, object> GenerateDocumentKey(IEnumerable<Field> fields,
            IEnumerable<MemberInfo> expressionMemberInfos)
        {
            var keyDictionary = new Dictionary<string, object>();

            foreach (var field in fields)
            {
                var type = expressionMemberInfos.FirstOrDefault(x => String.Equals(field.Name, x.Name, StringComparison.InvariantCultureIgnoreCase)) as PropertyInfo;

                if (type != null)
                {
                    var converter = TypeDescriptor.GetConverter(type.PropertyType);

                    keyDictionary.Add(field.Name, converter.ConvertFrom(field.Value));
                }
            }

            return keyDictionary;
        }

        private async Task<IEnumerable<string>> GetExpressionProperties(object expression, string expressionPropertyName)
        {
            var members = new List<string>();

            var expressionMembers = await GetExpressionMemberInfo(expression, expressionPropertyName);

            if (expressionMembers != null)
            {
                members = expressionMembers.Select(x => x.Name).ToList();
            }

            return members;
        }

        private Task<IEnumerable<MemberInfo>> GetExpressionMemberInfo(object expression, string expressionPropertyName)
        {
            var memberInfo = new List<MemberInfo>();

            var expressionValue = expression.GetType().GetProperty(expressionPropertyName).GetValue(expression);

            if (expressionValue != null)
            {
                var expressionBody = expressionValue.GetType().GetProperty("Body").GetValue(expressionValue);

                if (expressionBody != null)
                {
                    var expressionMembers = expressionBody.GetType().GetProperty("Members").GetValue(expressionBody) as IEnumerable<MemberInfo>;

                    if (expressionMembers != null)
                    {
                        memberInfo = expressionMembers.ToList();
                    }
                }
            }

            return Task.FromResult<IEnumerable<MemberInfo>>(memberInfo);
        }
    }
}