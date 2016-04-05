var app = angular.module('foundoc', ['ngToast']);

app.controller('managerCtrl', function ($scope, $log, ngToast, documentService) {

    $scope.documentSelected = false;

    documentService.getDocumentTypes()
    .then(function (data) {
        if (data.status === 200) {

            $scope.documents = [];

            $scope.documents.push({ 'name': 'Select Document' });

            data.data.forEach(function (item) {
                $scope.documents.push({ 'name': item, 'selectable': true });
            });

            $scope.selectedDocument = $scope.documents[0];
        }
    });

    $scope.selectDocument = function () {

        var documentSelected = $scope.selectedDocument.selectable;

        $log.debug($scope.selectedDocument);

        if (documentSelected) {
            var getIndexes = documentService.getIndexes($scope.selectedDocument.name);
            getIndexes.then(function (indexes) {
                if (indexes.status === 200) {
                    //clear existing
                    $scope.indexSelected = false;
                    $scope.indexes = [];
                    $scope.fields = [];

                    $scope.indexes.push({ 'name': 'Search By' });

                    indexes.data.forEach(function (item) {
                        $scope.indexes.push({ 'name': item, 'selectable': true });
                    });

                    $scope.selectedIndex = $scope.indexes[0];

                    $scope.documentSelected = true;

                    $scope.loadMetaData();
                }
            });
        } else {
            $scope.indexes = [];
            $scope.documentSelected = false;
        }
    }

    $scope.loadMetaData = function () {
        documentService.getMetaData($scope.selectedDocument.name)
            .then(function (response) {
                if (response.status === 200) {
                    $scope.metaData = response.data;
                }
            });
    }

    $scope.selectIndex = function () {

        var indexSelected = $scope.selectedIndex.selectable;

        $log.debug('Index selected', indexSelected);

        if (indexSelected) {
            var getFields = documentService.getFields($scope.selectedDocument.name, $scope.selectedIndex.name);
            getFields.then(function (fields) {
                if (fields.status === 200) {
                    $scope.fields = [];

                    $log.debug(fields.data);

                    fields.data.forEach(function (item) {
                        $scope.fields.push({ 'name': item });
                    });

                    $scope.indexSelected = true;
                }
            });
        } else {
            $scope.indexSelected = false;
        }
    }

    $scope.search = function() {
        $log.debug('Search for ', $scope.fields);

        documentService.search($scope.selectedDocument.name, $scope.selectedIndex.name, $scope.fields)
            .success(function(data) {
                $log.debug('message', data);

                $scope.results = [];

                data.forEach(function(result) {
                    $scope.results.push(result);
                });

                //if we only have one result select it
                if ($scope.results.length === 1) {
                    $scope.selectedResult = $scope.results[0];
                }

                ngToast.create('Results Loaded');

                $log.debug($scope.selectedResult);

            }).error(function(data, status) {
                $log.debug('no results');
                if (status === 404) {

                    $scope.results = [];
                    $scope.selectedResult = null;

                    ngToast.create({
                        'content': 'No Resuls',
                        'class': 'warning'
                    });
                }
            });
    };

    $scope.delete = function() {
        $log.debug('Delete items with ', $scope.fields);

        documentService.delete($scope.selectedDocument.name, $scope.selectedIndex.name, $scope.fields)
            .success(function(data) {
                $log.debug('message', data);

                ngToast.create('Results Deleted');

                $log.debug($scope.selectedResult);

            }).error(function(data, status) {
                $log.debug('no results');
                if (status === 404) {

                    $scope.results = [];
                    $scope.selectedResult = null;

                    ngToast.create({
                        'content': 'No Resuls',
                        'class': 'warning'
                    });
                }
            });
    };

    $scope.selectResult = function () {
        $log.debug('selected', $scope.selectedResult);
    }

    $scope.save = function () {

        $log.debug('Save document', $scope.selectedResult.value);

        if (isJsonString($scope.selectedResult.value)) {
            documentService.save($scope.selectedDocument.name, $scope.selectedResult)
                .success(function (data, status) {
                    ngToast.create('Document Saved');
                });
        } else {
            ngToast.create({
                'content': 'Document is invalid Json.  Try using <a href="http://jsonlint.com/">Json lint</a> to validate it.',
                'class': 'danger',
                'timeout': 20000
            });
        }
    }
});


function isJsonString(str) {
    try {
        JSON.parse(str);
    } catch (e) {
        return false;
    }
    return true;
}


