/*
 *後台基本操作
 */
var site = {
    masterView: '#masterView',    //主畫面 (list/edit可切換)
    editView: '#editView',        //編輯畫面 (list/edit可切換)
    mainTable: '#mainTable',    //列表datatable
    treeView: '#treeView',
    treeSort: '#treeSort',
    editTable: '#editTable',
    baseUrl: null,              //controller url

    init: function () {
        $(".nav .submenu li.active").closest("ul").closest("li")
            .addClass("active").addClass("open");
    },
    bindEvent: function () {
        //console.log("--bindEvent--");

        site.baseUrl = $("#baseUrl").val();
        //-----master-----
        if ($(site.masterView).length) {
            //change page        
            $(site.masterView).on("click", ".pagination a", function () {
                var page = $(this).data("page");
                site.getPageList(page);
            });

            //change page size       
            $(site.masterView).on("change", "#PageSize", function () {
                var page = $(this).data("page");
                site.getPageList();
            });

            //go to page
            $(site.masterView).on("keypress", "#page-text", function () {           
                if (event.keyCode == 13) {
                    if ($(this).val() != "") {
                        var page = $(this).val();
                        site.getPageList(page);
                    }
                }                 
            });
            
            //delete & confirm
            $(site.masterView).on("click", ".DeleteBtn", function (e) {
                e.preventDefault();
                site.showConfirm("確認刪除?", site.doingFn, $(this).closest('tr').data("id"), "Delete");
            });

            //go to view
            $(site.masterView).on("click", ".ViewBtn", function (e) {
                e.preventDefault();
                var action = "View/" + $(this).closest('tr').data("id");
                site.showEdit(action);
            });
            $(site.masterView).on("click", ".LogBtn", function (e) {
                e.preventDefault();
                var action = "Log/" + $(this).closest('tr').data("id");
                site.showEdit(action);
            });

            //go to edit
            $(site.masterView).on("click", ".EditBtn,.languageBtn", function (e) {
                var lang = $(this).val();
                var addurl = (lang) ? ("langType=" + lang) : "";
                var action = "Update/" + $(this).closest('tr').data("id");
                site.showEdit(action, addurl);
            });

            //go to create
            $(site.masterView).on("click", "#CreateBtn", function (e) {
                site.showEdit("Create");
            });

            //go to sort
            $(site.masterView).on("click", "#SortBtn", function (e) {
                site.showEdit("Sort");
            });
                        
            //init datetime picker
            site.datePlugin($(site.masterView));

            //---search---
            $(site.masterView + " .toolbar :not(.input-daterange)").find("input[type='text']").keypress(function (event) {
                if (event.keyCode == 13)
                    site.getPageList();
            });

            $(site.masterView + " .toolbar .input-daterange input[type='text']").change(function (event) {
                site.getPageList();
            });
           
            $(site.masterView + " .toolbar").find("input[type='checkbox']").click(function () {
                site.getPageList();
            });

            $(site.masterView + " .toolbar").find("select").change(function () {
                site.getPageList();
            });

        }

        //-----edit-----
        if ($(site.editView).length) {
            //go to index
            $(site.editView).on("click", "#CancelBtn", function (e) {
                site.showMaster();
            });
            $(site.editView).on("click", ".CancelBtn", function (e) {
                site.showMaster();
            });

            //save
            $(site.editView).on("click", "#SaveBtn", function (e) {
                site.saveFn($(this));
            });

            //preview
            $(site.editView).on("click", "#PreviewBtn", function (e) {
                site.previewFn($(this));
            });

            //revert combine
            $(site.editView).on("click", ".RevertBtn", function (e) {
                e.preventDefault();
                site.showConfirm("確認要復原?", site.revertFn, $(this).closest('tr').data("id"), $(this).closest('tr').data("backtoid"));
            });
            //check all
            site.checkAll($(site.editView));

            //reset template 
            $(site.editView).on("click", "#resetTemplate", function (e) {
                e.preventDefault();
                $(".template-block").html($(this).siblings(".template-default").html());
            });

        }

        //-----tree-----
        if ($(site.treeView).length) {
            //display edit
            $(site.treeView).on("click", ".timeline-label:not(.no-edit), .languageBtn", function (e) {
                var $node = $(this).closest("li");
                var id = $node.data("id");
                var lang = $(this).val();

                if (!$node.hasClass("EditNode")) {//editing previent
                    //close last one, 同時只開啟一個edit
                    if ($(site.treeView).find(".EditNode").length) {
                        site.showTreeView($(site.treeView).find(".EditNode> .timeline-label"), $(site.treeView).find("li.EditNode").data("id"), false);
                    }

                    site.showTreeEdit($node.find(".timeline-label:first"), id, lang);
                }
            });

            //go to view
            $(site.treeView).on("click", ".CancelTree", function (e) {
                e.stopPropagation()
                site.showTreeView($(this).closest(".timeline-label"), $(this).closest("li").data("id"), false);
            });

            //save
            $(site.treeView).on("click", "#SaveBtn", function (e) {
                e.stopPropagation()
                site.saveFn($(this));
            });

            //delete & confirm
            $(site.treeView).on("click", ".DeleteTree", function (e) {
                e.stopPropagation()
                e.preventDefault();
                site.showConfirm("確認刪除?", site.doingFn, $(this).closest("li").data("id"), "Delete");
            });

            //load init tree
            if ($(site.treeView).hasClass("loadData")) {
                site.getTree($("#treeTypeBtns button.active").val());
            }

            //change type
            $("#treeTypeBtns button").click(function () {
                site.getTree($(this).val());
                $(this).siblings("button").removeClass("btn-primary").addClass("btn-default");
                $(this).removeClass("btn-default").addClass("btn-primary")
            })

        }

    },
    showMaster: function (id) {
        //console.log("--showMaster--");

        if ($(site.mainTable).length) {
            site.getPageList(null, id);
        }
        if ($(site.treeView).length) {
            site.getTree($("#treeTypeBtns button.active").val());
        }

        $(site.masterView).show();
        $(site.editView).hide();
        $(site.editView).empty();
    },
    showEdit: function (action, addurl) {
        //console.log("--showEdit--" + id);

        var url = site.makeUrl(action, addurl);

        $.ajax({
            type: "GET",
            url: url,
            dataType: "html",
            beforeSend: function () {
                site.loadingMask.start();
            },
            complete: function () {
                site.loadingMask.stop();
            },
            success: function (result) {
                if (result == "LogOutError") {
                    site.showToast("請重新登入", 'error');
                    return;
                }

                $(site.editView).empty();
                $(site.editView).html(result);
                $(site.editView).show();
                $(site.masterView).hide();
                site.markRequired();

                //init edit table            
                if ($(site.editTable).length) {
                    site.editData.init();
                }

                //init datetime picker
                site.datePlugin($(site.editView));

                //init tree  
                if ($(site.editView + " " + site.treeView).length) {
                    site.tree.init();
                }
                if ($(site.editView + " " + site.treeSort).length) {
                    site.tree.sortInit();
                }

                //init contain check
                site.containCheck.sum($(site.editView));

                //init select
                $(site.editView).find(".selectpicker").selectpicker("refresh");
                site.customSelect.init($(site.editView));

                //fix radio-remove all checked
                //$(site.editView).find('input[type=radio]').change(function () {                    
                //    $("[name='" + $(this).attr("name") + "']").not(this).removeAttr('checked');
                //});

                //init ckeditor
                site.ckeditor.init();

                //init template
                site.template.edit($(site.editView));

                //init file
                site.fileUpload.fileList = [];
                $(site.editView).find(".fileuploader").each(function () {
                    site.fileUpload.bindEvent($(this));
                });

                //init popover
                $(site.editView).find('[data-toggle=popover]').popover();

                //init sort               
                $(".sort-list, .sort-tree").sortable({
                    placeholder: "highlight",
                    start: function (event, ui) {
                        ui.item.toggleClass("highlight");
                    },
                    stop: function (event, ui) {
                        ui.item.toggleClass("highlight");
                    }
                });

                //init tag---
                if ($(site.editView).find(".tagsinput").length > 0) {
                    var tagUrl = site.makeUrl("GetAllTags", "type=Keywords");
                    var tagnames = new Bloodhound({
                        datumTokenizer: Bloodhound.tokenizers.obj.whitespace('name'),
                        queryTokenizer: Bloodhound.tokenizers.whitespace,
                        prefetch: {
                            url: tagUrl,
                            filter: function (list) {
                                return $.map(list, function (tagname) {
                                    return { name: tagname };
                                });
                            },
                            cache: false
                        }
                    });
                    //console.log(tagnames)
                    tagnames.initialize();

                    $(site.editView).find(".tagsinput").tagsinput({
                        allowDuplicates: false,
                        typeaheadjs: {
                            name: 'tagnames',
                            displayKey: 'name',
                            valueKey: 'name',
                            source: tagnames.ttAdapter()
                        }
                    })
                }

            },
            error: function (error) {
                console.log('ERRORS: ' + error);
                site.showToast('系統錯誤', 'error');
            }
        });
    },

    showTreeEdit: function ($block, id, lang) {
        //console.log("--showTreeEdit--" + id);
        var action = id ? "/Update/" + id : "/Create";
        var addurl = "";
        if (lang)
            addurl = "?langType=" + lang;

        var structureid = $block.closest("li").data("structureid");
        if (structureid != undefined)
            addurl = "?structureID=" + structureid;

        //內容複製到暫存區
        $block.closest("li").find(".hidden:first").html($block.html());

        $.ajax({
            type: "GET",
            url: site.baseUrl + action + addurl,
            dataType: "html",
            beforeSend: function () {
                site.loadingMask.start();
            },
            complete: function () {
                site.loadingMask.stop();
            },
            success: function (result) {
                if (result == "LogOutError") {
                    site.showToast("請重新登入", 'error');
                    return;
                }

                $block.html(result);
                $block.hide();

                if (!id)//create
                {
                    //set value
                    var parentID = $block.closest("ul.tree-branch-children").closest("li").data("id");
                    if (parentID != undefined) {
                        $block.find("#ParentID").val(parentID);
                    }

                    var sort = $block.closest("ul").children("li.ViewNode:last()").data("sort");
                    var value = sort == undefined ? 0 : parseInt(sort) + 1
                    $block.find("#Sort").val(value);

                    $block.find("#Type").val($("#treeTypeBtns button.active").val());
                }

                //init select 
                $block.find(".selectpicker").selectpicker("refresh");
                $block.find('.ffl-wrapper').floatingFormLabels();

                //init input Required
                site.markRequired();

                //init contain check
                site.containCheck.sum($block);

                //change css                
                $block.closest("li")
                    .removeClass("ViewNode").removeClass("CreateNode")
                    .addClass("EditNode");

                $block.show();

            },
            error: function (error) {
                console.log('ERRORS: ' + error);
                site.showToast('系統錯誤', 'error');
            }
        });
    },
    showTreeView: function ($block, id, isRefresh) {

        if (!isRefresh) {//from cancel
            //從暫存區還原     
            $block.html($block.closest("li").find(".hidden:first").html());

            //change css
            $block.closest("li").removeClass("EditNode");
            if (!id) {
                $block.closest("li").addClass("CreateNode");
            } else {
                $block.closest("li").addClass("ViewNode")
            }

        } else { //from edit complete
            $.ajax({
                type: "GET",
                url: site.baseUrl + "/GetNode/" + (id ? id : ""),
                dataType: "html",
                beforeSend: function () {
                    site.loadingMask.start();
                },
                complete: function () {
                    site.loadingMask.stop();
                },
                success: function (result) {
                    if (result == "LogOutError") {
                        site.showToast("請重新登入", 'error');
                        return;
                    }

                    $block.html(result);

                    //change css
                    $block.closest("li").removeClass("EditNode");
                    if (!id) {
                        $block.closest("li").addClass("CreateNode");
                    } else {
                        $block.closest("li").addClass("ViewNode")
                    }

                },
                error: function (error) {
                    console.log('ERRORS: ' + error);
                    site.showToast('系統錯誤', 'error');
                }
            });
        }
    },
    getTree: function (type) {
        //console.log("--getTree--" + type);

        $.ajax({
            url: site.baseUrl + "/_Tree",
            dataType: "html",
            data: { type: type },
            beforeSend: function () {
                site.loadingMask.start();
            },
            complete: function () {
                site.loadingMask.stop();
            },
            success: function (data) {
                if (data == "LogOutError") {
                    site.showToast("請重新登入", 'error');
                    return;
                }

                //編輯後重整，恢復位置
                //var $editNode = $(site.treeView).find(".EditNode").offset();
                //var pos = $editNode == undefined ? 0 : $editNode.top - 20;

                //clear all
                //$(site.treeView).jstree('destroy');
                $(site.treeView).empty();

                //set data
                $(site.treeView).html(data);
                $("#treeTypeBtns button.active").removeClass("active");
                $("#treeTypeBtns button[value='" + type + "']").addClass("active");

                //init tree              
                site.tree.init();
                //$(window).scrollTop(pos);
            },
            error: function (err) {
                console.log('ERRORS: ' + err);
            }
        });
    },

    createSearchUrl: function(){
        //組合查詢查詢字串
        var addurl = "";
        $(site.masterView + " .toolbar").find("input[type='text'],select").each(function () {
            var value = $(this).val();
            var name = $(this).attr("name");
          
            if (jQuery.type(value) == "array") {

                $.each(value, function (i, v) {
                    addurl += name + '=' + v + '&';
                });

            } else {
                addurl += name + '=' + value + '&';
            }
        });

        $(site.masterView + " .toolbar").find("input[type='checkbox']:checked").each(function () {
            addurl += $(this).attr("name") + '=true&';
        });

        return addurl;
    },
    getPageList: function (page, id) {
        //console.log("--getPageList--, page=" + page + ", id=" + id);

        var addurl = site.createSearchUrl();       
        var url = site.makeUrl("GetPageList", addurl);
        //console.log(url);
        var pagesize = $("#PageSize").val();

        if (!page)
            page = $("#CurrentPage").val();//reload

        $.ajax({
            url: url,
            dataType: "html",
            data: { CurrentPage: page, PageSize: pagesize },
            beforeSend: function () {
                site.loadingMask.start();
            },
            complete: function () {
                site.loadingMask.stop();
            },
            success: function (data) {
                if (data == "LogOutError") {
                    site.showToast("請重新登入", 'error');
                    return;
                }

                $(site.mainTable).html(data);
                if (id != undefined)
                    site.markNowRow(id);

                //init popover
                $(site.masterView).find('[data-toggle=popover]').popover();

                //init select
                $("#PageSize").selectpicker("refresh");                
            },
            error: function (err) {
                console.log('ERRORS: ' + err);
            }
        });

    },
    doingFn: function (id, action) {
        //console.log("--doingFn--, id=" + id);

        $.ajax({
            type: "POST",
            url: site.makeUrl(action),
            data: { id: id },
            dataType: "json",
            beforeSend: function () {
                site.loadingMask.start();
            },
            complete: function () {
                site.loadingMask.stop();
            },
            success: function (result) {
                if (result == "LogOutError") {
                    site.showToast("請重新登入", 'error');
                    return;
                }
                site.showToast(result.Message, result.IsSuccess ? 'success' : 'error');
                if (result.IsSuccess) {
                    //back to list
                    if ($(site.mainTable).length) {
                        site.showMaster();
                    }

                    //back to tree view                    
                    if ($(site.treeView).length) {
                        // site.getTree($("#treeTypeBtns button.active").val());

                        //remove tree node
                        $(site.treeView).find("li[data-id='" + id + "']").remove();
                    }

                }
            },
            error: function (xhr, ajaxOptions, thrownError) {
                console.log('ERRORS: ' + xhr.responseText);
                var errormsg = xhr.responseText == "LogOutError" ? "請重新登入" : "系統錯誤";
                site.showToast(errormsg, 'error');
            }
        });
    },
    saveFn: function ($element) {
        var $form = $element.closest("form");

        //custion tool
        site.customSelect.save($form);
        site.stringReplace.save($form);

        //ckeditor
        site.ckeditor.save();

        //fix valid
        $form.removeData("validator").removeData("unobtrusiveValidation");//remove the form validation
        $.validator.unobtrusive.parse($form);//add the form validation

        //fileupload        
        $form.find(".fileuploader").each(function () {
            site.fileUpload.renameInput($(this));
        });

        var formData = new FormData($form[0]);
        $form.find(".fileuploader").each(function () {
            formData = site.fileUpload.appendFile($(this), formData);
        });
        if (formData == null) {
            return;//file size over
        }
        //not work for fileupload, serialize=key&value
        //var formData = $form.serialize() + '&' + $.param(obj);   


        //html encode      
        $form.find(".htmlencode").each(function () {
            site.template.save($form);
            var str = $(this).prop("tagName") == "TEXTAREA" ? $(this).val() : $(this).html();
            //console.log($(this).data("name"));
            //console.log(str);
            formData.append($(this).data("name"), site.htmlEncode(str));
        });


        if ($form.valid()) {
            $.ajax({
                type: "POST",
                url: $form.attr("action"),
                data: formData,
                catche: false,
                dataType: "json",
                processData: false,
                contentType: false,
                beforeSend: function () {
                    site.loadingMask.start();
                },
                complete: function () {
                    site.loadingMask.stop();
                },
                success: function (result) {
                    if (result == "LogOutError") {
                        site.showToast("請重新登入", 'error');
                        return;
                    }

                    site.showToast(result.Message, result.IsSuccess ? 'success' : 'error');

                    if (result.IsSuccess) {
                        //remove all fileupload
                        $form.find(".fileuploader").each(function () {
                            site.fileUpload.fileListRemoveAll($(this));
                        });

                        //0.refesh edit table                       
                        if ($(site.editTable).length) {
                            var $row = $element.closest("tr");
                            var url = $(site.editTable).data('view') + result.Data;
                            site.editData.showView($row, url);
                            $row.attr("data-addurl", result.Data);
                            $row.attr("data-id", result.ID);
                        }

                            //1.back to list & mark row
                        else if ($(site.mainTable).length) {
                            var id = result.Data == undefined ? $form.find("#ID").val() : result.Data; //edit: get Hidden id, create: return id
                            site.showMaster(id);
                        }

                        //2.back to tree view
                        var $block = $element.closest(".timeline-label");
                        if ($block.length) {
                            if ($form.find("#isNew").val()) {
                                site.getTree($("#treeTypeBtns button.active").val());
                            } else {
                                site.showTreeView($block, id, true);
                            }
                        }
                        //3.go to list url
                        var url = $("#returnUrl").attr("href");
                        if (url) {
                            window.location = url;
                        }
                    }
                },
                error: function (xhr, ajaxOptions, thrownError) {
                    console.log('ERRORS: ' + xhr.responseText);
                    var errormsg = xhr.responseText == "LogOutError" ? "請重新登入" : "系統錯誤";
                    site.showToast(errormsg, 'error');
                }
            });
        } else {
            site.showToast('欄位錯誤', 'error');
        }
    },
    previewFn: function ($element) {
        var $form = $element.closest("form");

        //ckeditor
        site.ckeditor.save();

        //fix valid
        $form.removeData("validator").removeData("unobtrusiveValidation");//remove the form validation
        $.validator.unobtrusive.parse($form);//add the form validation

        var formData = new FormData($form[0]);
        $form.find(".fileuploader").each(function () {
            formData = site.fileUpload.appendFile($(this), formData);
        });
        if (formData == null) {
            return;//file size over
        }

        //fileupload        
        var count = 0;
        $form.find(".fileuploader").each(function () {
            formData = site.fileUpload.renameInputPreview($(this), formData, count);
            count++;
        });

        //html encode      
        $form.find(".htmlencode").each(function () {
            site.template.save($form);
            var str = $(this).prop("tagName") == "TEXTAREA" ? $(this).val() : $(this).html();
            formData.append($(this).data("name"), site.htmlEncode(str));
        });

        if ($form.valid()) {
            var w = window.open('about:blank', '_blank');

            $.ajax({
                type: "POST",
                url: "/Item/Preview",
                data: formData,
                catche: false,
                dataType: "html",
                processData: false,
                contentType: false,
                beforeSend: function () {
                    site.loadingMask.start();
                },
                complete: function () {
                    site.loadingMask.stop();
                },
                success: function (result) {
                    // $(w.document.html).html(result);
                    w.document.write(result);
                },
                error: function (xhr, ajaxOptions, thrownError) {
                    console.log('ERRORS: ' + xhr.responseText);
                    var errormsg = xhr.responseText == "LogOutError" ? "請重新登入" : "系統錯誤";
                    site.showToast(errormsg, 'error');
                }
            });
        } else {
            site.showToast('欄位錯誤', 'error');
        }
    },
    revertFn: function (id, backtoID) { //--------------todo
    
        $.ajax({
            type: "POST",
            url: site.makeUrl("Revert"),
            data: { id: id },
            dataType: "json",
            beforeSend: function () {
                site.loadingMask.start();
            },
            complete: function () {
                site.loadingMask.stop();
            },
            success: function (result) {
                if (result == "LogOutError") {
                    site.showToast("請重新登入", 'error');
                    return;
                }
                site.showToast(result.Message, result.IsSuccess ? 'success' : 'error');
                if (result.IsSuccess) {
                    //back to Edit
                    var action = "Update/" + backtoID;
                    site.showEdit(action);
                }
            },
            error: function (xhr, ajaxOptions, thrownError) {
                console.log('ERRORS: ' + xhr.responseText);
                var errormsg = xhr.responseText == "LogOutError" ? "請重新登入" : "系統錯誤";
                site.showToast(errormsg, 'error');
            }
        });
    },

    editData: {
        init: function () {

            //row add
            $(site.editTable).on("click", "#edittable-action-create", function (e) {
                var $row = $(this).closest('tr');
                var url = $(site.editTable).data('create') + $row.data('addurl');

                site.editData.addRow(url);
            });

            //row edit
            $(site.editTable).on("click", ".edittable-action-edit", function (e) {
                var $row = $(this).closest('tr');
                var url = $(site.editTable).data('update') + $row.data('addurl');

                site.editData.showEdit($row, url);
            });

            //row delete
            $(site.editTable).on("click", ".edittable-action-remove", function (e) {
                var $row = $(this).closest('tr');
                var url = $(site.editTable).data('remove') + $row.data('addurl');

                site.showConfirm("確認刪除?", site.editData.deleteRow, $row, url);
            });

            //row cancel
            $(site.editTable).on("click", ".edittable-action-cancel", function (e) {
                var $row = $(this).closest('tr');
                var action = $(site.editTable).data('view');
                var addurl = $row.data('addurl');
                if (addurl == undefined) {
                    $row.remove();
                    site.editData.showButton();
                }
                else {
                    site.editData.showView($row, action + addurl);
                }
            });

            //toggle
            $(site.editTable).on("change", "[name='AllSwitch']", function () {
                if ($(this).is(":checked")) {
                    $(site.editTable).find("[name='InnerSwitch']").prop("checked", true);
                    $(site.editTable).find(".switch-panel").collapse("show");
                } else {
                    $(site.editTable).find("[name='InnerSwitch']").prop("checked", false);
                    $(site.editTable).find(".switch-panel").collapse("hide");
                }
            });

            //check all
            site.checkAll($(site.editTable));

            //sort
            if ($(site.editTable).hasClass("sort-table")) {
                $(site.editTable + " tbody").sortable({
                    handle: '.edittable-action-sort',
                    start: function (event, ui) {
                        ui.item.toggleClass("highlight");
                    },
                    stop: function (event, ui) {
                        ui.item.toggleClass("highlight");

                        site.editData.saveSort();
                    },
                    helper: function (e, ui) {//fix td width
                        ui.children().each(function () {
                            $(this).width($(this).width());
                        });
                        return ui;
                    }
                });
            }
        },
        hideButton: function () {
            $(".edittable-action-sort").addClass("not-active");
            $(".edittable-action-edit").addClass("not-active");
            $(".edittable-action-remove").addClass("not-active");
            $("#edittable-action-create").addClass("not-active");
        },
        showButton: function () {
            $(site.editTable + " .not-active").removeClass("not-active");
        },
        showEdit: function ($row, url) {
            $.ajax({
                type: "GET",
                url: url,
                dataType: "html",
                beforeSend: function () {
                    site.loadingMask.start();
                },
                complete: function () {
                    site.loadingMask.stop();
                },
                success: function (result) {
                    if (result == "LogOutError") {
                        site.showToast("請重新登入", 'error');
                        return;
                    }

                    $row.html(result);
                    $row.addClass("edit-row");
                    site.editData.hideButton();

                    site.markRequired();

                    //init datetime picker
                    site.datePlugin($(site.editView));

                    //init select
                    $(site.editView).find(".selectpicker").selectpicker("refresh");

                    //init string
                    site.stringReplace.init($(site.editView));
                },
                error: function (error) {
                    console.log('ERRORS: ' + error);
                    site.showToast('系統錯誤', 'error');
                }
            });
        },
        showView: function ($row, url) {
            $.ajax({
                type: "GET",
                url: url,
                dataType: "html",
                beforeSend: function () {
                    site.loadingMask.start();
                },
                complete: function () {
                    site.loadingMask.stop();
                },
                success: function (result) {
                    if (result == "LogOutError") {
                        site.showToast("請重新登入", 'error');
                        return;
                    }

                    $row.html(result);
                    $row.removeClass("edit-row");
                    site.editData.showButton();
                },
                error: function (error) {
                    console.log('ERRORS: ' + error);
                    site.showToast('系統錯誤', 'error');
                }
            });
        },
        deleteRow: function ($row, url) {
            $.ajax({
                type: "GET",
                url: url,
                dataType: "json",
                beforeSend: function () {
                    site.loadingMask.start();
                },
                complete: function () {
                    site.loadingMask.stop();
                },
                success: function (result) {
                    if (result == "LogOutError") {
                        site.showToast("請重新登入", 'error');
                        return;
                    }
                    site.showToast(result.Message, result.IsSuccess ? 'success' : 'error');

                    if (result.IsSuccess) {
                        $row.remove();
                    }
                },
                error: function (error) {
                    console.log('ERRORS: ' + error);
                    site.showToast('系統錯誤', 'error');
                }
            });
        },
        addRow: function (url) {
            $.ajax({
                type: "GET",
                url: url,
                dataType: "html",
                beforeSend: function () {
                    site.loadingMask.start();
                },
                complete: function () {
                    site.loadingMask.stop();
                },
                success: function (result) {
                    if (result == "LogOutError") {
                        site.showToast("請重新登入", 'error');
                        return;
                    }

                    $(site.editTable + " tbody").append("<tr class='edit-row'>" + result + "</tr>");
                    //$("<tr class='edit-row'>" + result + "</tr>").insertBefore($row);                                      
                    site.editData.hideButton();

                    site.markRequired();

                    //init datetime picker
                    site.datePlugin($(site.editView));

                    //init select
                    $(site.editView).find(".selectpicker").selectpicker("refresh");
                },
                error: function (error) {
                    console.log('ERRORS: ' + error);
                    site.showToast('系統錯誤', 'error');
                }
            });
        },
        saveSort: function () {
            var data = [];

            $(site.editTable + " tbody tr").each(function () {
                data.push($(this).data("id"));
            });

            $.ajax({
                type: "POST",
                url: site.makeUrl("Sort"),
                data: { IDs: data },
                dataType: "json",
                beforeSend: function () {
                    site.loadingMask.start();
                },
                complete: function () {
                    site.loadingMask.stop();
                },
                success: function (result) {
                    if (result == "LogOutError") {
                        site.showToast("請重新登入", 'error');
                        return;
                    }
                    site.showToast(result.Message, result.IsSuccess ? 'success' : 'error');
                },
                error: function (xhr, ajaxOptions, thrownError) {
                    console.log('ERRORS: ' + xhr.responseText);
                    var errormsg = xhr.responseText == "LogOutError" ? "請重新登入" : "系統錯誤";
                    site.showToast(errormsg, 'error');
                }
            });
        }
    },

    containCheck: {
        sum: function ($block) {
            var _check = ".containChk";
            var _sum = ".containSum";
            var _group = ".containGroup";

            $block.off('change', _check)
            $block.on('change', _check, function (event) {

                var sum = [];

                $(this).closest(_group).find(_check).each(function () {
                    if ($(this).is(":checked")) {
                        sum.push($(this).val());
                    }
                });

                $(this).closest(_group).find(_sum).val(sum.join(","));
            });
        }
    },
    tree: {
        init: function () {
            var close_icon = "fa-caret-right"; //ace-icon tree-plus
            var open_icon = "fa-caret-down";//ace-icon tree-minus

            //add element
            $(site.treeView + " ul").closest("li").prepend("<i class='ace-icon fa'></i>");//collapse icon
            $(site.treeView + " li").prepend("<div class='hidden'></div>");//hide area for recover

            //add class
            $(site.treeView).addClass("tree");
            $(site.treeView + " ul").addClass("tree-branch-children");
            $(site.treeView + " li").addClass("tree-item");//no child    
            $(site.treeView + " ul").closest("li").removeClass("tree-item").addClass("tree-branch tree-open");

            //open all 
            $(site.treeView + " li.tree-branch").addClass("tree-open");
            $(site.treeView + " li.tree-branch > i").addClass(open_icon);

            $(site.treeView + " li.tree-branch > i").click(function () {
                var $node = $(this).closest("li.tree-branch")
                if ($node.hasClass("tree-open")) {
                    //close
                    $node.removeClass("tree-open");
                    $node.find("ul:first()").addClass("hidden")
                    $(this).addClass(close_icon).removeClass(open_icon);

                } else {
                    //open
                    $node.addClass("tree-open");
                    $node.find("ul:first()").removeClass("hidden")
                    $(this).addClass(open_icon).removeClass(close_icon);
                }
            });
        },
        sortInit: function () {
            //add class
            $(site.treeSort).addClass("tree");
            $(site.treeSort + " ul").addClass("tree-branch-children");
            $(site.treeSort + " li").addClass("tree-item");//no child    
            $(site.treeSort + " ul").closest("li").removeClass("tree-item").addClass("tree-branch tree-open");
        }
    },
    datePlugin: function ($block) {
        //date   
        $block.find('.date-picker').datepicker({
            autoclose: true,
            todayHighlight: true,
            format: "yyyy/mm/dd"
        })
        //show datepicker when clicking on the icon
        .next().on(ace.click_event, function () {
            $(this).prev().focus();
        });
        //date range       
        $block.find('.input-daterange').datepicker({
            autoclose: true,
            todayHighlight: true,
            format: "yyyy/mm/dd"
        });

        //datetime    
        $block.find('.datetimepicker').datetimepicker({
            useCurrent: false,
            format: "YYYY/MM/DD HH:mm",
        });
    },
    customSelect: {
        //[自訂工具] check選項轉成select, 存進hidden
        //select-mapping: 區域
        //select-mapping-hidden: 存值
        //select-mapping-source: check來源區
        //select-mapping-target: 產生select
        init: function ($block) {
            //check
            $block.find('.select-mapping-source').unbind();
            $block.find('.select-mapping-source [type=checkbox],.select-mapping-source[type=radio]').unbind();

            $block.find(".select-mapping").each(function () {
                var $me = $(this);
                //init
                site.customSelect.checkToSelect($me);
                //onchange
                $me.on('change', ".select-mapping-source [type=checkbox],.select-mapping-source[type=radio]", function (event) {
                    site.customSelect.checkToSelect($me);
                });            
            });
        },
        checkToSelect: function ($this) {
            $source = $this.find(".select-mapping-source");
            $target = $this.find("select.select-mapping-target");
            $hidden =  $this.find(".select-mapping-hidden");          

            $target.empty();
            $source.find("[type=checkbox]:checked,[type=radio]:checked").each(function () {
                var value = $(this).closest("li").find("input[type='hidden']").val();
                var name = $(this).siblings("span").text();
                var selected = value == $hidden.val() ? "selected" : "";
                $target.append("<option value='" + value + "' " + selected + ">" + name + "</option>");
            });

            $target.selectpicker("refresh");
            $hidden.val($target.val());
            $target.change(function () {
                $hidden.val($target.val());
            });
        },
        save: function ($block) {

            //select to Hidden
            $block.find(".select-mapping-block").each(function () {
                var ss = $(this).find("select").val();
                $(this).find("input[type='hidden']").val(ss);
            });
        },
    },
    stringReplace: {
        //[自訂工具] 符號取代
        //string-replace: 區域
        //string-replace-hidden: 存值
        //string-replace-target: 顯示textarea
        init: function ($block) {

            $block.find(".string-replace").each(function () {
                var $me = $(this);
                site.stringReplace.fn($me);
            });
        },
        fn: function ($this) {

            var type = $this.data("type");
            var str = $this.find(".string-replace-hidden").val();

            //replace all
            //type 1: {,} to {new line}
            str = str.replace(/\,/g, "\n");

            $target = $this.find(".string-replace-target");
            $target.val(str);
        },
        save: function ($block) {

            //input to Hidden
            $block.find(".string-replace").each(function () {
                var str = $(this).find(".string-replace-target").val()

                //type 1
                str = str.replace(/\n/g, ",");
                str = str.replace(/\s+/g, "");

                $(this).find(".string-replace-hidden").val(str);
            });
        },
    },
    checkAll: function ($block) {

        $block.on('change', '.checkAll', function (event) {
            var target = $(this).data("target");
            var checked = $(this).is(":checked");
            console.log(target)
            $("[name='" + target + "']").prop('checked', checked);
        });
    },

    ckeditor: {
        init: function () {
            if ($("#ContentText").length) {
                CKEDITOR.replace('ContentText', {
                    filebrowserImageUploadUrl: $("#ImageUploadUrl").val()
                });
            }
            site.ckeditor.imagesJson = [];
        },
        save: function () {
            for (instance in CKEDITOR.instances) {
                CKEDITOR.instances[instance].updateElement();
            }
        },
        imagesJson: [],
        setImage: function (model) {
            site.ckeditor.imagesJson.push(model)
            $("#ContentImagesJson").val(JSON.stringify(site.ckeditor.imagesJson));
        }
    },
    fileUpload: {
        //{$fileuploader_id, filename, file} save file for upload
        //所有 $fileuploader共用
        fileList: [],
        maxsize: 104857600, //Bytes=102400KB in web.config
        fileListRemoveAll: function ($fileuploader) {
            //回傳符合條件的陣列
            site.fileUpload.fileList =
                $.grep(site.fileUpload.fileList, function (item) {
                    return (item.fileuploader != $fileuploader.attr("id"))
                });

            if (site.fileUpload.fileList == undefined)
                site.fileUpload.fileList = [];

        },
        removeItem: function ($item) {
            var $FileStatus = $item.find(".FileStatus");//New, Delete, Normal
            var filename = $item.find("input.Name").val();
            var fileuploader_id = $item.closest(".fileuploader").attr("id");

            if ($FileStatus.val() == "New") {
                //New:直接刪除
                site.fileUpload.fileList =
                    $.grep(site.fileUpload.fileList, function (item) {
                        return (!(item.fileuploader == fileuploader_id && item.filename == filename));
                    });

                $item.remove();
            } else {
                //Normal:狀態改為Delete
                $FileStatus.val("Delete");
                $item.addClass("hidden");
            }
        },

        bindEvent: function ($fileuploader) {
            //file size limit
            $fileuploader.find(".maxsize").html(site.fileUpload.bytesToSize(site.fileUpload.maxsize));

            // youtube btn
            $fileuploader.find(".youtubeBtn").click(function () {
                site.fileUpload.displayYouTube($fileuploader);
            });

            // choose file
            $fileuploader.find('.fileuploader-input').unbind();
            $fileuploader.find(".fileuploader-input").click(function () {
                $fileuploader.find(".mainFile").click();
            });

            //drag and drop file
            $fileuploader.find(".fileuploader-input")
                .bind('dragenter', function (e) {
                    e.preventDefault();
                    $(this).addClass('fileuploader-dragging');

                }).bind('dragover', function (e) {
                    e.preventDefault();
                    $(this).addClass('fileuploader-dragging');

                }).bind('dragleave', function (e) {
                    e.preventDefault();
                    $(this).removeClass('fileuploader-dragging');

                }).bind('drop', function (e) {
                    e.preventDefault();
                    $(this).removeClass('fileuploader-dragging');

                    var files = e.originalEvent.dataTransfer.files;

                    if (files.length) {
                        site.fileUpload.displayFiles(files, $fileuploader);
                    }
                });

            // file change
            $fileuploader.find('.mainFile').unbind();
            $fileuploader.find(".mainFile").change(function (e) {
                //console.log('file_change');
                if (!e.target.files || !window.FileReader) return;

                site.fileUpload.displayFiles(e.target.files, $fileuploader);
            });

            //delete & confirm
            $fileuploader.find('.fileuploader-action-remove').unbind();
            $fileuploader.on("click", ".fileuploader-action-remove", function (e) {
                e.preventDefault();
                site.showConfirm("確認刪除?", site.fileUpload.removeItem, $(this).closest('.fileuploader-item'));
            });

            //sort
            $fileuploader.find(".fileuploader-items-list").sortable({
                handle: '.fileuploader-action-sort'
            });
        },
        displayFiles: function (files, $fileuploader) {
            var $itemlist = $fileuploader.find('.fileuploader-items-list');
            var isMulitiple = $fileuploader.data("multiple");
            var checkext = $fileuploader.data("checkext").split(',')
                .map(function (item, index) {
                    return item.replace(".", "");
                });

            $(files).each(function (i) {
                var file = files[i];
                var filename = file.name;
                var filesize = site.fileUpload.bytesToSize(file.size);
                var ext = filename.split('.').pop();
                ext = ext.toLowerCase();
                var filetype = site.fileUpload.getFileType(ext);

                //check ext
                if (site.fileUpload.checkExt(ext, checkext)) {

                    //not mutiple file
                    if (!isMulitiple) {
                        site.fileUpload.removeItem($fileuploader.find(".fileuploader-items-list .fileuploader-item"));
                    }

                    //tempfile for save
                    var item = {
                        fileuploader: $fileuploader.attr("id"),
                        filename: filename,
                        file: file,
                    }
                    site.fileUpload.fileList.push(item);

                    //append to list                  
                    if (filetype == "Images") {
                        //image
                        var reader = new FileReader();
                        reader.readAsDataURL(file);
                        reader.onload = function (e) {
                            var filedata = e.target.result;
                            site.fileUpload.setHtml($itemlist, file, filedata, filetype, filename, filesize);
                        }

                    } else {
                        //document
                        site.fileUpload.setHtml($itemlist, file, null, filetype, filename, filesize);
                    }

                } else {
                    //wrong ext
                    site.showToast('副檔名錯誤 ' + ext, 'error');
                }

            });
        },
        displayYouTube: function ($fileuploader) {
            var $itemlist = $fileuploader.find('.fileuploader-items-list');
            var isMulitiple = $fileuploader.data("multiple");

            var $input = $fileuploader.find(".youtubeText")
            var filetype = "YouTube";
            var youtubeid = site.fileUpload.getYouTubeID($input.val());

            //check url
            if (youtubeid != "") {

                //not mutiple file
                if (!isMulitiple) {
                    site.fileUpload.removeItem($fileuploader.find(".fileuploader-items-list .fileuploader-item"));
                }

                //append to list
                site.fileUpload.setHtml($itemlist, null, youtubeid, "YouTube", "YouTube", "");

                //clear
                $input.val('')

            } else {
                //wrong ext
                site.showToast('YouTube網址錯誤', 'error');
            }

        },
        setHtml: function ($itemlist, file, filedata, filetype, filename, filesize) {
            var count = $itemlist.find("li").length;
            var template = $itemlist.siblings(".template").html();
            $itemlist.append(template);
            var $itemHtml = $itemlist.find("li:last()");

            //file
            if (file != null) {
                $itemHtml.find(".fileupload").on('blur', function (e) {
                    $itemHtml.find(".fileupload").val(file);
                });
            }

            //thumbnail 
            if (filedata != null) {
                var imgurl = filetype != "YouTube" ? filedata :
                    "http://i.ytimg.com/vi/" + filedata + "/hqdefault.jpg";
                var linkurl = "https://www.youtube.com/watch?v=" + filedata;

                if (filetype != "YouTube") {
                    $itemHtml.find(".fileuploader-item-image img").unwrap();
                } else {
                    $itemHtml.find(".fileuploader-item-image img").parent().attr("href", linkurl);
                    $itemHtml.find("input.FilePath").val(filedata);
                }

                $itemHtml.find(".fileuploader-item-image img").attr("src", imgurl);
                $itemHtml.find(".fileuploader-item-image img").removeClass("hidden");
                $itemHtml.find(".fileuploader-item-icon").addClass("hidden");
            }

            $itemHtml.find("span.FileType").text(filetype);
            $itemHtml.find("input.FileType").val(filetype);

            //title
            $itemHtml.find("span.Name").text(filename);
            $itemHtml.find("input.Name").val(filename);
            $itemHtml.find(".Size").text(filesize);

            if (!$itemHtml.closest(".fileuploader").data("multiple")) {
                $itemHtml.find(".fileuploader-action-sort").addClass("hidden");
            }
        },
        /*setHtml: function (filedata, ext, filename, filesize, $items) {
            var count = $items.find("li").length;

            var itemHtml =
            '<li class="fileuploader-item">'
            + '<div class="columns">'
            //thumbnail
            + '<div class="column-thumbnail">'
            + ' <div class="fileuploader-item-image">'
            + (filedata != null
            //---image
                ? '   <img src="' + filedata + '" />'
            //---document
                : '   <div class="fileuploader-item-icon"><span>' + ext + '</span></div>'
            )
            + ' </div>'
            + '</div>'
            //title
            + '<div class="column-title">'
            + ' <div class="column-title-filename">' + filename + '</div>'
            + ' <div class="column-title-filesize">' + filesize + '</div>'
            + '</div>'
            //action
            + '<div class="column-actions">'
            + (count > 0 ? ' <a class="fileuploader-action-sort" title="Sort"><i class="fa fa-arrows-alt"></i></a>' : "")
            + ' <a class="fileuploader-action-remove" title="Remove"><i class="fa fa-times"></i></a>'
            + ' </div>'
            //
            + '</div>'
            + '</li>';

            $items.append(itemHtml);

        },*/
        types: [
                { name: "Images", extensions: ['jpg', 'jpeg', 'gif', 'png', 'bmp'] },
                { name: "PDF", extensions: ['pdf'] },
                { name: "Word", extensions: ['doc', 'docx'] },
                { name: "Excel", extensions: ['xls', 'xlsx', 'csv'] },
        ],
        getFileType: function (ext) {
            var itemType = site.fileUpload.types.find(x => site.fileUpload.checkExt(ext, x.extensions));
            return itemType == undefined ? "" : itemType.name;
        },
        checkExt: function (ext, arr) {
            if ($.inArray(ext, arr) == -1) {
                return false;
            }
            return true;
        },
        bytesToSize: function (bytes) {
            var sizes = ['Bytes', 'KB', 'MB', 'GB', 'TB'];
            if (bytes == 0) return '0 Byte';
            var i = parseInt(Math.floor(Math.log(bytes) / Math.log(1024)));
            return Math.round(bytes / Math.pow(1024, i), 2) + ' ' + sizes[i];
        },
        renameInput: function ($fileuploader) {
            $fileuploader.find(".fileuploader-items-list li").each(function (i) {
                $(this).find("input").each(function () {
                    $(this).attr("name", $(this).attr("name").replace("[x]", "[" + i + "]"));
                });
            });
        },
        appendFile: function ($fileuploader, formData) {
            var totalsize = 0;
            var idName = $fileuploader.attr("id");

            $fileuploader.find(".fileuploader-items-list li").each(function (i) {

                //find object in list
                var filename = $(this).find("input.Name").val();
                var x = $.map(site.fileUpload.fileList, function (item, index) {
                    return item.filename
                }).indexOf(filename);

                if (x != -1) {
                    //append file         
                    formData.append(idName + "[" + i + "].FileUpload", site.fileUpload.fileList[x].file);
                    totalsize += site.fileUpload.fileList[x].file.size;
                }
            });

            if (totalsize > site.fileUpload.maxsize) {
                site.showToast('檔案上傳超出限制! ' + site.fileUpload.bytesToSize(site.fileUpload.maxsize), 'error');
                return null;
            } else {
                //console.log(totalsize)
                return formData;
            }
        },
        renameInputPreview: function ($fileuploader, formData, count) {
            var idName = $fileuploader.attr("id");

            $fileuploader.find(".fileuploader-items-list li").each(function (i) {
                var fileType = $(this).find(".FileType").val();
                //enum to int
                $(this).find(".FileType").val(fileType == "Images" ? 1 : fileType == "YouTube" ? 30 : 0);
                //set sourceType(封面 or 內頁)
                $(this).find(".SourceType").val(idName == "CoverModel" ? 0 : 4);

                $(this).find("input").each(function () {
                    var name = $(this).attr("name").replace("[x]", "[" + i + "]")//todo (i + count)
                                                   .replace(idName, "ItemFiles");
                    var value = $(this).val();
                    if ($(this).hasClass("FilePath") && fileType == "Images") {//youtube:val or image:src
                        value = $(this).closest(".fileuploader-item").find("img").attr("src");
                    }

                    formData.append(name, value);
                    //console.log(name + " " + value)
                });
            });

            return formData;
        },
        getYouTubeID: function (urlstr) {
            var youtubeid = "";

            if (!site.isNullOrEmpty(urlstr)) {
                var symbols = new Array("?v=", "&v=", "/v/", "/embed/", "//youtu.be/");
                var strAt = -1;
                var symLen = 0;

                symbols.forEach(function (sym) {
                    if (strAt == -1) {
                        strAt = urlstr.indexOf(sym, 0);
                        symLen = sym.length;
                    }
                });

                if (strAt > 0) {
                    var endAt = urlstr.indexOf("&", strAt + 1);
                    endAt = endAt > 0 ? endAt : urlstr.length;
                    youtubeid = urlstr.substring(strAt + symLen, endAt);
                }
            }

            return youtubeid;
        },
    },
    dynamicSelect: {
        //data: null,
        init: function ($block, $parent, $child) {

            $child.find("option").hide();

            $block.on("change", $parent, function (e) {
                var value = $parent.val();
                $child.find("option:not([data-parentid=''])").hide(); //hide all except default

                if (value != '') {                
                    $child.find("option[data-parentid='" + value + "']").show();
                }
                $child.selectpicker("refresh");
            });
        },
      
    },
    template: {
        edit: function ($block) {
            $block.find(".template-block .template,"
                      + ".template-default .template").attr("contenteditable", "true");

            //prevent insert html when press enter
            $block.find("[contenteditable='true']").keydown(function (e) {
                if (e.keyCode === 13) {
                    document.execCommand('insertHTML', false, '<br><br>');
                    return false;
                }
            });

            //remove all html tag (when keyup)
            //$block.find("[contenteditable='true']").keyup(function (e) {
            //    $(this).html($(this).text());
            //});

        },
        save: function ($block) {
            //remove all html tag 
            $block.find("[contenteditable='true']").each(function (e) {
                //keep br                   
                $(this).html($(this).html().replace(/<br>/g, "@@br@@"));

                $(this).html($(this).text().replace(/@@br@@/g, "<br>"));
            });

            $block.find(".template-block .template").removeAttr("contenteditable");
        },
    },
    //jstree: {
    //    init: function () {
    //        $(site.treeView).jstree({
    //            "types": {
    //                "default": {
    //                    "icon": " "//fa fa-folder
    //                },
    //            },
    //            "plugins": ["types"],
    //            "core": {
    //                "check_callback": true //for create_node work
    //            }

    //        }).on("open_node.jstree", function (e, data) {
    //            //--remove selected: for input focus not work--
    //            $(site.treeView + " .jstree-anchor").removeClass("jstree-anchor");
    //        });
    //        $(site.treeView + " .jstree-anchor").removeClass("jstree-anchor");
    //        //open all
    //        $(site.treeView).jstree("open_all");
    //        //remove a-link #
    //        //$(site.treeView + " .jstree-node > a").attr("href", "javascript:");
    //        $(site.treeView + " .jstree-node > a").wrap('<div class="node-div"></div>').contents().unwrap();

    //    },      
    //},
    //--------------------------------
    initMessage: function () {
        //show alert
        var message = $("#AlertMessage").val();
        var type = $("#AlertType").val(); // Enum:AlertType

        if (message != '' && type != '') {
            site.showToast(message, type);
            $("#AlertMessage").val('');
            $("#AlertType").val();;;
        }
    },
    showToast: function (message, type) {
        toastr.options = {
            "closeButton": true,
            "debug": false,
            "progressBar": true,
            "preventDuplicates": false,
            "positionClass": "toast-top-center",
            "onclick": null,
            "showDuration": "400",
            "hideDuration": "1000",
            "timeOut": "3000",
            "extendedTimeOut": "1000",
            "showEasing": "swing",
            "hideEasing": "linear",
            "showMethod": "fadeIn",
            "hideMethod": "fadeOut"
        }
        toastr[type](message);
    },
    showConfirm: function (message, okEvent, x, y) { //x,y自訂參數
        $("#confirm_box .confirm_message").text(message);
        $("#confirm_box").modal('show')

        $('#confirm_cancel').unbind();
        $("#confirm_cancel").click(function () {
            $("#confirm_box").modal('hide');
        });

        $('#confirm_ok').unbind();
        $("#confirm_ok").click(function () {
            $("#confirm_box").modal('hide');
            okEvent(x, y);
        });
    },
    loadingMask: {
        start: function () {
            $("#LoadingMask").modal({ backdrop: 'static' });
        },
        stop: function () {
            $("#LoadingMask").modal('hide');
        },
    },
    markNowRow: function (id) {
        $(site.mainTable).find("tr").removeClass("warning");
        $(site.mainTable).find("tr[data-id='" + id + "']").addClass("warning").removeClass("pending");
    },
    markRequired: function () {
        $("input[data-val-required]:not([type='radio']):not([type='hidden']), textarea[data-val-required]").each(function () {

            if ($(this).attr("data-val-required") == "") {//remove empty
                $(this).removeAttr("data-val-required");
                return;
            }

            var $label = $(this).closest(".form-group").find(".control-label");
            if ($label.length) {
                //normal
                $label.find("span").remove();
                var str = $label.text();
                $label.html(str + '<span class="mark-required">＊</span>');

            } else {
                //ffl-label
                $label = $(this).siblings(".ffl-label")
                $label.find("span").remove();
                var str = $label.text();
                $label.html(str + '<span>＊</span>');
            }
        });
    },
    makeUrl: function (action, addurl) {
        //加入action在接?參數
        var urlArr = site.baseUrl.split("?");
        var url = (urlArr.length == 2)
                ? urlArr[0] + "/" + action + "?" + urlArr[1]
                : site.baseUrl + "/" + action;

        //其他參數
        if (addurl != undefined && addurl != "") {
            url = (urlArr.length == 2)
                ? url + "&" + addurl
                : url + "?" + addurl;
        }

        return url
    },
    isNullOrEmpty: function (s) {
        return (s == "" || s == null || s == undefined);
    },
    htmlEncode: function (s) {
        var div = document.createElement('div');
        div.appendChild(document.createTextNode(s));
        return div.innerHTML;
    },
    test: function () {

    }
}
