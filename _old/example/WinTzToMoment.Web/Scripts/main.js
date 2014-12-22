(function() {

    // Add Windos time zones to the moment.js
    _.each(window.windowsTimezones, function(tz) {
        var packed = moment.tz.pack(tz);
        moment.tz.add(packed);
    });

    angular.module('app', []).controller('AppController', ['$scope', '$q', '$http', function($scope, $q, $http) {

        $scope.save = function() {
            if (localStorage) {
                localStorage.setItem("selectedTz", $scope.selectedTz.name);
                localStorage.setItem("dateTime", $scope.dateTime);
            }
        };
        
        $scope.restore = function () {
            if (localStorage) {
                var val = localStorage.getItem("selectedTz");
                if (val) {
                    $scope.selectedTz = _.find($scope.windowsTimezones, function (x) {
                        return x.name === val;
                    });
                }
                
                val = localStorage.getItem("dateTime");
                if (val) {
                    $scope.dateTime = val;
                }
            }
        };
        
        $scope.windowsTimezones = window.windowsTimezones;

        $scope.selectTimeZone = function(tz) {
            $scope.selectedTz = tz;
        };
        $scope.selectTimeZone($scope.windowsTimezones[0]);
        
        $scope.dateTime = moment().utc().format();

        $scope.restore();
        
        function convert(id, dt) {
            return $q(function(resolve) {
                $http.get("/default/convert", {
                    cache: false,
                    params: {
                        id: id,
                        dt: dt
                    }
                }).success(function(data) {
                    resolve(data);
                });
            });
        }


        $scope.convertSelected = function() {
            $scope.isSuccess = false;
            $scope.isFailure = false;
            var tUtc = moment.utc($scope.dateTime);
            $scope.clientDateTime = moment.tz(tUtc, $scope.selectedTz.name).format();
            $scope.serverDateTime = "loading...";
            convert($scope.selectedTz.name, $scope.dateTime).then(function(data) {
                $scope.serverDateTime = data.value;
                $scope.serverOffset = data.offset;
                $scope.isSuccess = $scope.clientDateTime === $scope.serverDateTime;
                $scope.isFailure = !$scope.isSuccess;
            });
        };

        $scope.$watch("selectedTz", $scope.convertSelected);

        $scope.convertAll = function() {
            var tUtc = moment.utc($scope.dateTime);
            $scope.$all = null;
            var n = 1;
            var total = $scope.windowsTimezones.length;

            var queue = _.map($scope.windowsTimezones, function(tz) {
                return $q(function(resolve) {
                    var s1 = moment.tz(tUtc, tz.name).format();
                    convert(tz.name, $scope.dateTime).then(function(data) {
                        n++;
                        var s2 = data.value;
                        tz.$c = {
                            isSuccess: s1 === s2,
                            serverDateTime: s2
                        };
                        $scope.$all = n + " / " + total;
                        resolve();
                    });
                });
            });


            $q.all(queue).then(function() {
                var s = _.filter($scope.windowsTimezones, function(x) {
                    return x.$c.isSuccess;
                });

                $scope.$all = Math.round((s.length * 100.0 / $scope.windowsTimezones.length)) + " % SUCCESS"

            });
        };

    }]);


})()