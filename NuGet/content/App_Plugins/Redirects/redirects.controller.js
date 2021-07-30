angular.module("umbraco").controller("RedirectsController", function ($scope, $http, notificationsService, listViewHelper, overlayService, Upload) {
    var vm = this;
    vm.loading = true;
    vm.page = {
        name: "Manage Redirects"
    };
    vm.pagination = {
        pageNumber: 1,
        totalPages: 1
    };
    vm.options = {
        includeProperties: [
            { alias: "name", header: "Old URL", allowSorting: false },
            { alias: "newUrl", header: "New URL", allowSorting: false }
        ]
    };
    vm.items = [];
    vm.selection = [];
    vm.deleteState = "";
    vm.addState = "";
    vm.allowSelectAll = true;

    vm.selectItem = selectItem;
    vm.selectAll = selectAll;
    vm.isSelectedAll = isSelectedAll;
    vm.changePage = changePage;
    vm.deleteRedirects = deleteRedirects;
    vm.addRedirect = addRedirect;
    vm.filterModel = "";

    $scope.handleFiles = handleFiles;
    $scope.uploadState = 'busy';
    $scope.upload = upload;

    $scope.showImportingMessage = false;

    function handleFiles(files, event) {
        if (files && files.length > 0) {
            $scope.uploadState = 'init';
            $scope.file = files[0];
        } else {
            $scope.file = null;
        }
    }

    function upload(file) {
        $scope.uploadState = 'busy';
        $scope.showImportingMessage = true;
        Upload.upload({
            url: '/Umbraco/backoffice/api/RedirectApi/ImportRedirects',
            fields: {
                'someId': 1234
            },
            file: file
        }).success(function (data, status, headers, config) {
            $scope.uploadState = 'success';
            $scope.showImportingMessage = false;

            document.getElementById('uplredirects').value = "";
            notificationsService.success("Success", "Redirects imported successfully");
            changePage(1);
            $scope.file = null;

        }).error(function (data, status, headers, config) {
            $scope.buttonState = 'error';
            notificationsService.error('Upload Failed', data);
            $scope.showImportingMessage = false;
            notificationsService.error("Error", "Error importing redirects");
        });
    }

    angular.element(function () {
        init();
    });

    var init = function () {
        $http({
            method: 'POST',
            url: '/Umbraco/backoffice/Api/RedirectApi/ListRedirects',
            cache: false
        }).then(function (data) {
            for (var i = 0; i < data.data.length; i++) {
                var d = data.data[i];
                vm.items.push({
                    "id": d.Id,
                    "name": d.OldUrl,
                    "newUrl": d.NewUrl,
                })
            }
            vm.loading = false;
        });
        $http({
            method: 'GET',
            url: '/Umbraco/backoffice/Api/RedirectApi/GetRedirectPageCount',
            cache: false
        }).then(function (data) {
            var pages = data.data;
            if (pages == 0)
                pages = 1;
            vm.pagination.totalPages = pages;
        });
    };

    vm.filter = function () {
        vm.items = [];
        if (vm.filterModel == null || vm.filterModel == "") {
            $http({
                method: 'POST',
                url: '/Umbraco/backoffice/Api/RedirectApi/ListRedirects',
                cache: false
            }).then(function (data) {
                for (var i = 0; i < data.data.length; i++) {
                    var d = data.data[i];
                    vm.items.push({
                        "id": d.Id,
                        "name": d.OldUrl,
                        "newUrl": d.NewUrl,
                    })
                }
                vm.loading = false;
            });
            $http({
                method: 'GET',
                url: '/Umbraco/backoffice/Api/RedirectApi/GetRedirectPageCount',
                cache: false
            }).then(function (data) {
                var pages = data.data;
                if (pages == 0)
                    pages = 1;
                vm.pagination.totalPages = pages;
            });
        } else {
            $http({
                method: 'POST',
                url: '/Umbraco/backoffice/Api/RedirectApi/FilterRedirects?searchTerm=' + vm.filterModel,
                cache: false
            }).then(function (data) {
                for (var i = 0; i < data.data.length; i++) {
                    var d = data.data[i];
                    vm.items.push({
                        "id": d.Id,
                        "name": d.OldUrl,
                        "newUrl": d.NewUrl,
                    })
                }
                vm.loading = false;
            });
            $http({
                method: 'GET',
                url: '/Umbraco/backoffice/Api/RedirectApi/GetFilterRedirectPageCount?searchTerm=' + vm.filterModel,
                cache: false
            }).then(function (data) {
                var pages = data.data;
                if (pages == 0)
                    pages = 1;
                vm.pagination.totalPages = pages;
            });
        }
    };

    function changePage(pageNumber) {
        if (pageNumber != undefined) {
            vm.items = [];
            vm.loading = true;
            if (vm.filterModel != "") {
                $http({
                    method: 'POST',
                    url: '/Umbraco/backoffice/Api/RedirectApi/FilterRedirects?searchTerm=' + vm.filterModel + '&page=' + pageNumber,
                    cache: false
                }).then(function (data) {
                    for (var i = 0; i < data.data.length; i++) {
                        var d = data.data[i];
                        vm.items.push({
                            "id": d.Id,
                            "name": d.OldUrl,
                            "newUrl": d.NewUrl,
                        })
                    }
                    vm.loading = false;
                });
            } else {
                $http({
                    method: 'POST',
                    url: '/Umbraco/backoffice/Api/RedirectApi/ListRedirects?page=' + pageNumber,
                    cache: false
                }).then(function (data) {
                    for (var i = 0; i < data.data.length; i++) {
                        var d = data.data[i];
                        vm.items.push({
                            "id": d.Id,
                            "name": d.OldUrl,
                            "newUrl": d.NewUrl,
                        })
                    }
                    vm.loading = false;
                });
            }
        }
    }

    function selectAll($event) {
        listViewHelper.selectAllItemsToggle(vm.items, vm.selection);
        toggleBulkActions();
    }

    function isSelectedAll() {
        return listViewHelper.isSelectedAll(vm.items, vm.selection);
    }

    function selectItem(selectedItem, $index, $event) {
        listViewHelper.selectHandler(selectedItem, $index, vm.items, vm.selection, $event);
        toggleBulkActions();
    }

    function toggleBulkActions() {
        document.getElementById('bulkActions').style.display = vm.selection.length > 0 ? null : "none";
    }

    function deleteRedirects() {
        var confirm = {
            title: "Delete Redirects?",
            view: "default",
            content: "Are you sure you wish to delete the selected redirects? This action cannot be reversed.",
            submitButtonLabel: "Delete",
            closeButtonLabel: "Cancel",
            submit: function submit() {
                vm.deleteState = "busy";
                overlayService.close();
                var ids = [];
                for (var i = 0; i < vm.selection.length; i++) {
                    ids.push(vm.selection[i].id);
                }
                $http({
                    method: 'DELETE',
                    url: '/Umbraco/backoffice/Api/RedirectApi/DeleteRedirect?id=' + ids.join(),
                    cache: false
                }).then(function (data) {
                    vm.deleteState = "success";
                    changePage(1);
                });
            },
            close: function close() {
                overlayService.close();
            }
        };
        overlayService.open(confirm);
    }

    function addRedirect() {
        var confirm = {
            title: "Add Redirect",
            view: "/App_Plugins/Redirects/Views/Overlays/AddRedirect.html",
            content: "",
            submitButtonLabel: "Add",
            closeButtonLabel: "Cancel",
            submit: function submit() {
                vm.addState = "busy";
                overlayService.close();
                $http({
                    method: 'POST',
                    url: '/Umbraco/backoffice/Api/RedirectApi/AddRedirect',
                    data: { oldUrl: document.getElementById('txtOldUrl').value, newUrl: document.getElementById('txtNewUrl').value },
                    cache: false
                }).then(function (data) {
                    vm.addState = "success";
                    changePage(1);
                });
            },
            close: function close() {
                overlayService.close();
            }
        };
        overlayService.open(confirm);
    }
});