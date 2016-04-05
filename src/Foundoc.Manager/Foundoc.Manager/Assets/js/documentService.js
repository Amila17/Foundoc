app.service('documentService', function (apiClient) {

    this.getMetaData = function (documentType) {
        var url = "/foundocmanager/api/document/" + documentType + "/metadata";
        return apiClient.get(url);
    }

    this.getDocumentTypes = function () {
        var url = "/foundocmanager/api/documents";
        return apiClient.get(url);
    }

    this.getIndexes = function (documentType) {
        var url = "/foundocmanager/api/document/" + documentType + "/indexes";
        return apiClient.get(url);
    }

    this.getFields = function (documentType, index) {
        var url = "/foundocmanager/api/document/" + documentType + "/index/" + index + "/fields";
        return apiClient.get(url);
    }

    this.search = function (documentType, indexName,  fields) {
        var url = "/foundocmanager/api/document/" + documentType + "/index/"+ indexName + "/search";
        return apiClient.post(url, fields);
    }

    this.delete = function (documentType, indexName, fields) {
        var url = "/foundocmanager/api/document/" + documentType + "/index/" + indexName + "/delete";
        return apiClient.post(url, fields);
    }

    this.save = function (documentType, document) {
        var url = "/foundocmanager/api/document/" + documentType + "/update";
        return apiClient.post(url, document);
    };
});