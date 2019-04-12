/*
 *前台基本操作
 */
var web = {
    //baseUrl: null,
    init: function () {
        //message
        setTimeout(function () {
            web.initMessage();
        }, 1000);

        //bind
        web.bindEvent();
    },
    bindEvent: function () {
        //web.baseUrl = $("#baseUrl").val();

        //captcha
        if ($(".btnnewcode").length > 0) {
            $(".btnnewcode").click(function () {
                $(this).closest("form,#confirm_box").find(".imgverifycode").attr("src", "/Member/CaptchaImage?" + new Date().getTime().toString());
            });
        }

        //save
        if ($("form").length > 0) {
            $(document).on("click", "#SaveBtn", function (e) {
                e.preventDefault();
                web.saveFn($(this));
            });

            $("form").on("keypress", "input", function (e) {
                if (e.which == 13) {
                    if ($(this).hasClass("enterToSave")) {
                        //enter save
                        if ($("#SaveBtn").length > 0) {
                            $("#SaveBtn").click();
                            return false;
                        } else {
                            $(this).closest("form").submit();
                        }
                    } else {
                        e.preventDefault();
                    }
                }
            });
        }

        //get ajax
        if ($(".LinkAjax").length) {
            $(".LinkAjax").click(function (e) {
                e.preventDefault();
                web.linkFn($(this));
            });
        }

        //share      
        if ($(".ShareFacebook").length > 0) {
            $(".ShareFacebook").click(function () {
                web.popupCenter('https://www.facebook.com/share.php?u='.concat(encodeURIComponent(location.href)),
                "", 550, 550);
            })
        }
        if ($(".ShareTwitter").length > 0) {
            $(".ShareTwitter").click(function () {
                web.popupCenter('http://twitter.com/home/?status='.concat(encodeURIComponent(document.title)).concat(' ').concat(encodeURIComponent(location.href)),
                "", 550, 550);
            })
        }
        if ($(".ShareLine").length > 0) {
            $(".ShareLine").click(function () {
                web.popupCenter('https://social-plugins.line.me/lineit/share?url='.concat(encodeURIComponent(location.href)),
               "", 550, 550);
            })
        }
        if ($(".SharePinterest").length > 0) {
            $(".SharePinterest").click(function () {
                web.popupCenter('https://pinterest.com/pin/create/button/?description='.concat(encodeURIComponent(document.title)).concat('&url=').concat(encodeURIComponent(location.href)),
               "", 550, 550);
            })
        }
    },
    saveFn: function ($element, customEvent) {
        var $form = $element.closest("form");

        //fix valid
        $form.removeData("validator").removeData("unobtrusiveValidation");//remove the form validation
        $.validator.unobtrusive.parse($form);//add the form validation

        if ($form.valid() && web.fileUpload.checkSize($form)) {

            //rename input
            web.autoRow.renameInput();
            var formData = new FormData($form[0]);

            $.ajax({
                type: "POST",
                url: $form.attr("action"),
                data: formData,
                catche: false,
                dataType: "json",
                processData: false,
                contentType: false,
                beforeSend: function () {
                    //web.loadingMask.start();                    
                    $element.addClass("disabled");
                    $element.prop('disabled', true);
                    //all btn  
                    $(".btn:not(.disabled)").addClass("tempDisabled");
                    $(".btn:not(.disabled)").addClass("disabled");
                    $(".btn:not(.disabled)").prop('disabled', true);
                },
                complete: function () {
                    //web.loadingMask.stop();
                    $element.removeClass("disabled");
                    $element.prop('disabled', false);
                    $(".tempDisabled").removeClass("disabled");
                    $(".tempDisabled").prop('disabled', false);
                    $(".tempDisabled").removeClass("tempDisabled");
                },
                success: function (result) {
                    setTimeout(function () {
                        //confirm
                        if (result.IsSuccess) {  // && $form.find("#successMessage").val() == "Confirm"                    
                            var fn = customEvent ? customEvent : web.goToUrl;
                            web.showConfirmOnlyOk($form.find("#successMessage").data("title"), result.Message, result.IsSuccess, fn);

                        } else {
                            web.showConfirmOnlyOk($form.find("#successMessage").data("title"), result.Message, result.IsSuccess);
                        }
                    }, 500);

                },
                error: function (xhr, ajaxOptions, thrownError) {
                    if (xhr.responseText == "LogOutError") {
                        web.showToast("您已登出系統", 'error');

                    } else if (xhr.responseText == "PostInvalid") {
                        web.showToast("發生錯誤: 請勿多次送出, 或同時開啟多視窗", 'error');

                    } else {
                        console.log('ERRORS: ' + xhr.responseText);
                        web.showToast("發生錯誤", 'error');
                    }
                }
            });
        } else {
            //web.showToast('欄位錯誤', 'error');
        }
    },
    linkFn: function ($element) {
        $.ajax({
            type: "POST",
            url: $element.data("url"),
            catche: false,
            dataType: "json",
            beforeSend: function () {
                //web.loadingMask.start();                    
                $element.addClass("disabled");
                $element.prop('disabled', true);
            },
            complete: function () {
                //web.loadingMask.stop();
                $element.removeClass("disabled");
                $element.prop('disabled', false);
            },
            success: function (result) {

                //confirm           
                var goToUrl = $element.data("gotourl");
                if (goToUrl) {
                    web.showConfirmOnlyOk($element.data("title"), result.Message, result.IsSuccess, web.goToUrl, goToUrl);
                }
                else {
                    web.showConfirmOnlyOk($element.data("title"), result.Message, result.IsSuccess);
                }

            },
            error: function (xhr, ajaxOptions, thrownError) {
                if (xhr.responseText == "LogOutError") {
                    web.showToast("您已登出系統", 'error');
                                   
                } else {
                    console.log('ERRORS: ' + xhr.responseText);
                    web.showToast("發生錯誤", 'error');
                }
            }
        });
    },
    goToUrl: function (toUrl) {
        var url = $("#returnUrl").val();
        var block = $("#Block").val();

        if (toUrl)
            url = toUrl;

        if (url) {
            if (block) {
                url = url + "#" + block;
            }
            window.location = url;
            if (block) {
                location.reload();
            }
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
    autoRow: {
        init: function () {
            //delete row
            $(document).on("click", ".rowDeleteBtn", function (e) {
                web.showConfirm("", "確定要刪除嗎?", web.autoRow.removeRow, $(this).closest("tr"));
            });
            //clean hidden
            $(document).on("click", ".deleteHidden", function (e) {
                $("[name=" + $(this).data("target") + "]").val('');
                $(this).hide();
            });
        },
        renameInput: function () {
            $(".tableAutoRow").each(function () {
                $(this).find("tbody tr").each(function (i) {
                    $(this).find("input,select").each(function () {
                        if ($(this).attr("rename") != undefined) {
                            $(this).attr("name", $(this).attr("rename").replace("[x]", "[" + i + "]"));
                        }
                    });
                });
            });
        },
        removeRow: function ($item) {
            $item.remove();
        }
    },
    fileUpload: {
        maxsize: 104857600, //Bytes=102400KB in web.config
        checkSize: function ($form) {
            var totalsize = 0;
            $form.find('input:file').each(function () {
                //console.log(totalsize)
                if ($(this).val().length > 0) {
                    totalsize = totalsize + $(this)[0].files[0].size;
                }
            });
            if (totalsize > web.fileUpload.maxsize) {
                web.showToast("檔案上傳超出限制!", 'error');
                return false;
            }
            return true;
        }
    },
    dynamicSelect: {
        //data: null,
        init: function ($block, className, data) {

            //each select
            $block.find(className).each(function () {
                var i = parseInt($(this).data("level"));
                web.dynamicSelect.setOption($block, className, i, data);
            });

            //on change, set next
            $block.on("change", className, function (e) {
                var i = parseInt($(this).data("level"));
                web.dynamicSelect.setOption($block, className, i + 1, data);
            });

        },
        setOption: function ($block, className, i, data) {

            //var data = web.dynamicSelect.data;
            if (i > 1) {
                var parentValue = $block.find(className + "[data-level='" + (i - 1) + "']").val();

                //[ie not support]
                //if (parentValue == undefined || data.find(x => x.Value == parentValue) == undefined) {                   
                //    data = undefined;
                //} else {
                //    data = data.find(x => x.Value == parentValue).SubSelect;
                //}

                if (parentValue != undefined) {

                    var result = $.grep(data, function (item) {
                        return item.Value == parentValue;
                    });
                    if (result.length == 1) {
                        data = result[0].SubSelect;
                    } else {
                        data = undefined;
                    }
                }

            }

            if (data != undefined) {
                var $select = $block.find(className + "[data-level='" + i + "']");
                var value = $select.data("value");
                $select.empty();

                $.each(data, function (i, item) {
                    var selectedStr = (value != undefined && value.indexOf(item.Value) >= 0) ? " selected" : "";
                    $select.append("<option value='" + item.Value + "'" + selectedStr + ">" + item.Text + "</option>");
                });

                if ($select.hasClass("selectpicker")) {
                    $select.selectpicker("refresh");
                }
            }
        }
    },
    popupCenter: function (url, title, w, h) {
        var dualScreenLeft = window.screenLeft != undefined ? window.screenLeft : window.screenX;
        var dualScreenTop = window.screenTop != undefined ? window.screenTop : window.screenY;

        var width = window.innerWidth ? window.innerWidth : document.documentElement.clientWidth ? document.documentElement.clientWidth : screen.width;
        var height = window.innerHeight ? window.innerHeight : document.documentElement.clientHeight ? document.documentElement.clientHeight : screen.height;

        var left = ((width / 2) - (w / 2)) + dualScreenLeft;
        var top = ((height / 2) - (h / 2)) + dualScreenTop;
        var newWindow = window.open(url, title, 'scrollbars=yes, width=' + w + ', height=' + h + ', top=' + top + ', left=' + left);

        // Puts focus on the newWindow
        if (window.focus) {
            newWindow.focus();
        }
    },
    initMessage: function () {
        //show alert
        var message = $("#AlertMessage").val();
        var type = $("#AlertType").val(); // Enum:AlertType

        if (message != '' && type != '') {
            //web.showToast(message, type);
            web.showConfirmOnlyOk("", message, type == 'success')
            $("#AlertMessage").val('');
            $("#AlertType").val();;;
        }
    },
    showToast: function (message, type) {
        alert(message);
    },

    ConfirmCaptcha: {
        showFn: function () {
            $("#confirm_box .btnnewcode").click();//set img           
            $("#confirm_box input").val('');
            $("#confirm_box .confirm_captcha").removeClass("hidden");
        },
        hideFn: function () {
            $("#confirm_box .confirm_captcha").addClass("hidden");
        }
    },
    showConfirm: function (subject, message, okEvent, id) {
        $('#confirm_cancel').show();
        $("#confirm_success").hide();
        $("#confirm_fail").hide();
        $("#confirm_box .modal-title").show();

        $("#confirm_box .modal-title").text(subject);
        $("#confirm_box .confirm_message").html(message);
        $("#confirm_box").modal('show')

        $('#confirm_cancel').unbind();
        $("#confirm_cancel").click(function () {
            $("#confirm_box").modal('hide');
        });

        $('#confirm_ok').unbind();
        $("#confirm_ok").click(function () {
            //required captcha
            if (!$("#confirm_box .confirm_captcha").hasClass("hidden") && $("#confirm_box input").val() == '') {
                alert("請輸入驗證碼");
            }
            else {
                $("#confirm_box").modal('hide');
                if (okEvent) {
                    okEvent(id);
                }
            }
        });

        $('#confirm_box').on('hidden.bs.modal', function () {
            web.ConfirmCaptcha.hideFn();
        });
    },
    showConfirmOnlyOk: function (subject, message, isSuccess, okEvent, id) {
        $('#confirm_cancel').hide();
        $("#confirm_success").hide();
        $("#confirm_fail").hide();
        $("#confirm_box .modal-title").show();

        if (isSuccess) {
            $("#confirm_success").show();
        } else {
            $("#confirm_fail").show();
        }

        if (subject == '')
            $("#confirm_box .modal-title").hide();

        $("#confirm_box .modal-title").text(subject);
        $("#confirm_box .confirm_message").html(message);
        $("#confirm_box").modal('show')

        $('#confirm_cancel').unbind();
        $('#confirm_ok').unbind();
        $("#confirm_ok").click(function () {
            $("#confirm_box").modal('hide');
        });

        //okEvent when modal hide
        $('#confirm_box').on('hidden.bs.modal', function () {
            $('.modal-backdrop:not(:first)').remove();
            web.ConfirmCaptcha.hideFn();
            if (okEvent) {
                okEvent(id);
            }
        });
    },
    getUrlParameter: function (sParam) {
        var sPageURL = window.location.search.substring(1),
            sURLVariables = sPageURL.split('&'),
            sParameterName,
            i;

        for (i = 0; i < sURLVariables.length; i++) {
            sParameterName = sURLVariables[i].split('=');

            if (sParameterName[0] === sParam) {
                return sParameterName[1] === undefined ? true : decodeURIComponent(sParameterName[1]);
            }
        }
    },
}