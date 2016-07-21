/// <reference path="angular.min.js" />

var myApp = angular
        .module("app", [])
        .controller("SGProb2Ctrl", function ($scope, $http, $log) {
            var successCallback = function (response) { // 'then' func called when GET succeeds
                $scope.responseString = angular.toJson(response.data);
            };

            var errorCallback = function (reason) { // function called if error happened
                $scope.responseString = reason.data;
                $log(reason.data);
            };

            $scope.processSubmit = function (playerId, coinsBet, coinsWon, hashValue)
            {
                $http({
                    method: 'GET',
                    url: "SGProb2WebSvc.asmx/GetSlotMachineSpinResult",
                        params: { playerId: playerId, coinsBet: coinsBet, coinsWon: coinsWon, hashValue: hashValue }
                     })
                     .then(successCallback, errorCallback);
            }
        });