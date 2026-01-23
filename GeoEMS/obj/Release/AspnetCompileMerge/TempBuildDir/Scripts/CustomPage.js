GetGpsDataList();
function GetGpsDataList() {
    let url = "@Url.Action("GetAllGpsData", "Home")";
    $.ajax({
        url: url,
        crossDomain: true,

        success: function (data) {

            var Result = JSON.parse(data)
            data = Result.Data;
            if (data.length == 0) {
                toastr.error('', "Record is empty");
                if ($.fn.DataTable.isDataTable("#tdTableCommunity")) {
                    $('#tdTableCommunity').DataTable().clear().destroy();
                }
            }



            dataFilter = data;
            LiveStockDetailsf = dataFilter;

            if ($.fn.DataTable.isDataTable("#tdTableCommunity")) {
                $('#tdTableCommunity').DataTable().clear().destroy();
            }


            dtablCommunity = $('#tdTableCommunity').DataTable({
                dom: 'Bfrtip',
                // dom: '<"container-fluid"<"row"<"col"B><"col"l><"col"f>>>rtip',
                data: dataFilter,
                pageLength: 5,
                //lengthMenu: [[5, 10, 20, -1], [5, 10, 20, 'Todos']],
                //rowId: 'ImportID',
                scrollX: true,

                "columns": [
                    { "data": "id" },
                    { "data": "jfmc_id" },
                    { "data": "check_dam_id" },
                    { "data": "financial_year" },
                    { "data": "purpose" },
                    { "data": "total_financial_target" },

                    { "data": "comments" }, {
                        "data": function (data, type, row, meta) {
                            if (data.image1_name != null) {
                                data1 = "<button id='img1_" + data.id + "' class='btn btn-green btn-xs btn-rounded btn-border' onclick=DisplayImage(this.id) data-toggle='modal' data-target='#splashImg' data-options='splash-2splash-ef-4'>Image1</button>";
                                return data1;
                            }
                            else {
                                data1 = '';
                                return data1;
                            }
                        }
                    },


                    //{ "data": "image1" },
                    {
                        "data": function (data, type, row, meta) {
                            if (data.image2_name != null && data.image2_name != '') {
                                data1 = "<button id='img2_" + data.id + "' class='btn btn-green btn-xs btn-rounded btn-border' onclick=DisplayImage(this.id) data-toggle='modal' data-target='#splashImg' data-options='splash-2splash-ef-4'>Image2</button>";
                                return data1;
                            }
                            else {
                                data1 = '';
                                return data1;
                            }
                        }
                    },
                    {
                        "data": function (data, type, row, meta) {

                            data1 = "<button id='" + data.id + "' class='btn btn-success btn-xs btn-rounded' onclick=ZoomToMapLocation(this.id) ><span class='glyphicon glyphicon-map-marker'></span></button>";
                            return data1;

                        }
                    }
                ],

                "sScrollX": "100%",
                "bScrollCollapse": false,
                buttons: [
                    {
                        extend: 'print',
                        className: 'btn btn-outline-primary btnoperation',
                        text: '<i class="fa fa-print"  style="color:white"></i>',
                        titleAttr: 'Print'
                    },
                    {
                        extend: 'csv',
                        className: 'btn btn-outline-info btnoperation',
                        text: '<i class="fa fa-table"  style="color:white"></i>',
                        titleAttr: 'Csv'
                    },
                    {
                        extend: 'excel',
                        className: 'btn btn-outline-success btnoperation',
                        text: '<i class="fa fa-file-excel-o"  style="color:white"></i>',
                        titleAttr: 'Excel'
                    },
                    {
                        extend: 'pdf',
                        className: 'btn btn-outline-danger btnoperation',
                        text: '<i class="fa fa-file-pdf-o"  style="color:white"></i>',
                        titleAttr: 'Pdf'
                    }

                ],

            })
        }

    });
}