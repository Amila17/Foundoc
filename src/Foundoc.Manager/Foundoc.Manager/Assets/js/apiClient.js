app.service('apiClient', function ($http, $log, ngToast) {

    this.get = function (url) {
        $log.debug('apiClient get', url);
        var promise = $http({
            method: "GET",
            url: url,
        }).success(function (data, status, headers, config) {
            $log.debug('apiClient post response:', status, data);

        }).error(function (data, status, headers, config) {
            $log.debug('apiClient post error:', status, data);

            if (status == 500) {
                $log.debug('Server Error:', data.errorMessage);
                $log.debug(data.fullException);
                ngToast.create({
                    'content': data.errorMessage,
                    'class': 'danger',
                    'timeout': 20000
                });
            }
        });

        return promise;
    };

    this.post = function(url, data) {
        $log.debug('apiClient post', url, data);

        var promise = $http({
            method: 'POST',
            url: url,
            data: data
        }).success(function(data, status, headers, config) {
            $log.debug('apiClient post response:', status, data);

        }).error(function(data, status, headers, config) {
            $log.debug('apiClient post error:', status, data);

            if (status == 500) {
                $log.debug('Server Error:', data.errorMessage);
                $log.debug(data.fullException);
                ngToast.create({
                    'content': data.errorMessage,
                    'class': 'danger',
                    'timeout': 20000
                });
            }
        });

        return promise;
    };
});