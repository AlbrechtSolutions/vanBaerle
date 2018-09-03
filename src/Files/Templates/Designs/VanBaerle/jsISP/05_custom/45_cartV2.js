/**
 * Description: Cart V2
 * User: ShareIT
 * Date: 12/Jan/2016
 **/

$(function () {
    if(obj_settings.str_cartQty != "") minicart();
});

function onLoadCartPayment(bol_hasOrderSummary) {
    $("#stepsList li").removeClass("active");
    $("#stepsList li:last").addClass("active");

    if((bol_hasOrderSummary)) $("#cartRightContent").append(localStorage.getItem(str_projectFolder + '_orderSummary'));
}

function onLoadCartPrint() {
    $('#orderDetailContainer .blueBackgroundStripes > span').click(function () {
        window.print();
    });
}

function onLoadCartStep1(obj_params) {
    addressValidator();
    $('.headButton .btn-blue').click(function (e) {
        e.preventDefault();
        $('#customerInfo').submit();
    });
    localStorage.removeItem(str_projectFolder + '_minicart');

    var bol_firstTime = true,
        json_regions = $.parseJSON(obj_params.regions),
        json_addresses = $.parseJSON(obj_params.addresses),
        $obj_deliveryContainer = $("#deliveryFormContainer"),
        obj_helpers = {
            fcn_addValue:function ($obj_el, str_value) {
                $obj_el.val(str_value);
            },
            fcn_ajax:function (str_url, str_parameters, $obj_paste, str_findEl, fcn_action) {
                var $obj_ajax;

                loading(true);
                if ($obj_ajax != null) {
                    $obj_ajax.abort();
                }
                $obj_ajax = $.ajax({
                    type:"POST",
                    url:str_url,
                    data:str_parameters + "&" + obj_globalOptions.obj_urlPaths.str_ajaxRetriever,
                    headers:{
                        "Pragma":"no-cache",
                        "Cache-Control":"no-store, no-cache, must-revalidate, post-check=0, pre-check=0",
                        "Expires":0,
                        "Last-Modified":new Date(0),
                        "If-Modified-Since":new Date(0)
                    },
                    cache:false,
                    dataType:'html',
                    success:function (str_html) {
                        loading(false);

                        $("#updater").val($("#updater").attr("data-next"));
                        if ($obj_paste != null) {
                            $obj_paste.after($(str_html).find(str_findEl));
                            fcn_action();
                        }
                        else if ($(str_html).find(str_findEl).length) {
                            fcn_action($(str_html).find(str_findEl).html(), $(str_html).find(str_findEl).attr("data-errorid"), $(str_html).find(".step2").length);
                        }
                        else {
                            fcn_action();
                        }

                    }
                });
            },
            fcn_change:function ($obj_el, fcn_action) {
                $obj_el.change(function () {
                    fcn_action($(this));
                });
            },
            fcn_loop:function ($obj_elCicle, fcn_action) {
                for (var i = 0; i < $obj_elCicle.length; i++) {
                    fcn_action($obj_elCicle.eq(i), $obj_elCicle.eq(i).attr("name"));
                }
            },
            fcn_click:function ($obj_el, fcn_callBack, bol_preventDefault) {
                $obj_el.unbind("click").click(function (event) {
                    if (bol_preventDefault) event.preventDefault();
                    fcn_callBack($(this));
                });
            }
        },
        fcn_regions = function ($obj_this) {
            var str_regions = "<option class='defaultOption' value=''>" + $("#EcomOrderCustomerRegion option:first").html() + "</option>",
                bol_exist = false,
                bol_deliveryCountry = $obj_this.attr("id") != "EcomOrderDeliveryCountry";

            for (var i = 0; i < json_regions.locations.Countries.length; i++) {
                if (json_regions.locations.Countries[i].country.id == $obj_this.val()) {
                    for (var a = 0; a < json_regions.locations.Countries[i].country.regions.length; a++) {
                        bol_exist = true;
                        str_regions += "<option value='" + json_regions.locations.Countries[i].country.regions[a].code + "'>" + json_regions.locations.Countries[i].country.regions[a].name + "</option>";
                    }
                }
            }
            if (bol_deliveryCountry) {
                $obj_this.closest("div.part").find("#EcomOrderCustomerRegion").html(str_regions);
                obj_helpers.fcn_addValue($("#EcomOrderCustomerRegion"), obj_params.submitedValues.str_customerRegion);

            }
            $obj_this.closest("div.part").find("#EcomOrderDeliveryRegion").html(str_regions);
            if (obj_params.submitedValues.str_deliveryRegion != "") {
                obj_helpers.fcn_addValue($("#EcomOrderDeliveryRegion"), obj_params.submitedValues.str_deliveryRegion);
            }
            else {
                obj_helpers.fcn_addValue($("#EcomOrderDeliveryRegion"), obj_params.submitedValues.str_customerRegion);
            }

            if (bol_exist == false) {
                if (bol_deliveryCountry) {
                    $obj_this.closest("div.part").find("#EcomOrderCustomerRegion").prop("disabled", true);
                }
                $obj_this.closest("div.part").find("#EcomOrderDeliveryRegion").prop("disabled", true);
            }
            else {
                if (bol_deliveryCountry) {
                    $obj_this.closest("div.part").find("#EcomOrderCustomerRegion").prop("disabled", false);
                }
                $obj_this.closest("div.part").find("#EcomOrderDeliveryRegion").prop("disabled", false);
            }


            $('#customerInfo select').selectpicker('refresh');
        },
        fcn_countryChange = function ($obj_this) {
            if ($("#EcomOrderDeliveryCountry").val() == "" || $("input[name=deliveryaddress]:checked").val() != "yes") {
                $("#EcomOrderDeliveryCountry").val($("#EcomOrderCustomerCountry").val());
            }
            if ($obj_this != undefined) {
                fcn_regions($obj_this);
                if ($obj_this.attr("id") == "EcomOrderCustomerCountry" && $("input[name=deliveryaddress]:checked").val() == "no") {
                    fcn_regions($("#EcomOrderDeliveryCountry"));
                }
                fcn_changeTexts($obj_this, "Region");
            }

        },
        fcn_regionChange = function ($obj_this) {
            if ($("#EcomOrderDeliveryCountry").val() == "" || $("input[name=deliveryaddress]:checked").val() != "yes") {
                $("[name=EcomOrderDeliveryCountry]").val($("[name=EcomOrderCustomerCountry]").val());
                $("[name=EcomOrderDeliveryRegion]").val($("[name=EcomOrderCustomerRegion]").val());
            }
        },
        fcn_deliveryAddress = function ($obj_this) {
            var $obj_nameAtt = $('#nameAttention > label'),
                $obj_deliveryName = $('[name=EcomOrderDeliveryName]');
            if ($obj_this.val() == "yes") {
                $obj_deliveryContainer.removeClass("hide");
                obj_helpers.fcn_loop($("#customerInformation input[type=text]"), fcn_applyDeliveryValues);
                obj_helpers.fcn_loop($("#customerInformation input[type=email]"), fcn_applyDeliveryValues);
                obj_helpers.fcn_loop($("#customerInformation select"), fcn_applyDeliveryValues);

                $("#EcomOrderDeliveryCountry").change();
                if ($("[name=EcomOrderDeliveryRegion]").val() == "") $("[name=EcomOrderDeliveryRegion]").val($("[name=EcomOrderCustomerRegion]").val()).change();

                $obj_nameAtt.text($obj_nameAtt.attr("data-textDif"));
                if ($obj_deliveryName.attr('data-value') != "") {
                    $obj_deliveryName.val($obj_deliveryName.attr('data-value'));
                }
                $("#deliveryFormContainer .mandatory_").addClass("mandatory").removeClass("mandatory_");
            }
            else {
                $("#deliveryFormContainer .mandatory").addClass("mandatory_").removeClass("mandatory");
                $("#EcomOrderDeliveryName").val($("#EcomOrderCustomerName").val());
                $obj_deliveryContainer.addClass("hide");
                $obj_nameAtt.text($obj_nameAtt.attr("data-textEqual"));
                if ($obj_deliveryName.attr('data-value') != "") {
                    $obj_deliveryName.val($('[name=EcomOrderCustomerName]').val());
                }
            }
        },
        fcn_applyDeliveryValues = function ($obj_el, str_name) {
            if (str_name != "undefined" && str_name != undefined && str_name != "") {
                if ($("[name=" + str_name.replace("Customer", "Delivery") + "]").val() == "" || $("input[name=deliveryaddress]:checked").val() == "no") {
                    $("[name=" + str_name.replace("Customer", "Delivery") + "]").val($obj_el.val());
                }
            }
        },
        fcn_checkErrors = function (str_latestErrors, str_errorIds, str_existStep2) {
            var $obj_inputs = $("#customerInfo .mandatory input[type=text]").not(".input-block-level"),
                $obj_createPassword = $("#EcomUserCreatePassword"),
                $obj_confirmPassword = $("#EcomUserCreateConfirmPassword");

            if (str_latestErrors != null && str_latestErrors != undefined) {
                obj_params.errors.str_errors = str_latestErrors;
                obj_params.errors.str_errorsID = str_errorIds;
            }
            else if (str_latestErrors == undefined) {
                obj_params.errors.str_errors = "";
            }
            if ($("#EcomOrderDeliveryRegion option").length > 1 && $("#EcomOrderDeliveryRegion").val() == "" && !bol_firstTime) {
                obj_params.errors.str_errors += $("#EcomOrderDeliveryRegion").parent().parent().find("label").html();
                $("#EcomOrderDeliveryRegion, #EcomOrderCustomerRegion").parent().parent().addClass("has-error");
            }

            if (obj_params.errors.str_errorsID != null && obj_params.errors.str_errorsID != "" && obj_params.errors.str_errorsID != undefined) {
                arr_ids = obj_params.errors.str_errorsID.split("--");

                for (var a = 0; a < arr_ids.length; a++) {
                    $("#" + arr_ids[a]).parent().addClass("has-error");
                }
            }
            if (obj_params.errors.str_errors != "") {
                if (obj_params.errors.str_errors.indexOf("newValue") > -1) {
                    $("#addressValidators").remove();
                    $("body").append(obj_params.errors.str_errors);
                    if ($(".addressChange").length > 0) {
                        addressValidator();
                    }
                    else {
                        $("#addressValidators").remove();
                    }
                }
                else {
                    alert(obj_params.errors.str_errors.replace(/\--/g, '<br/>'));
                }
            }
            else if (!bol_firstTime) {
                for (var i = 0; i < $obj_inputs.length; i++) {
                    if ($obj_inputs.eq(i).val() == "") {
                        obj_params.errors.str_errors += $obj_inputs.eq(i).siblings("label").html() + "<br/>";
                    }
                }
                if (obj_params.errors.str_errors == "") {
                    if (generalValidations($("#customerInfo"), true)) {
                        if ($obj_createPassword.length && ($obj_createPassword.val() != $obj_confirmPassword.val())) {
                            $obj_createPassword.closest("fieldset").addClass("has-error");
                            $obj_confirmPassword.closest("fieldset").addClass("has-error");
                        }
                        else {
                            $("#EcomOrderCustomerRegion, #EcomOrderDeliveryRegion").prop("disabled", false);
                            $(".newUsername input[type=text]").val($("#EcomOrderCustomerEmail").val());
                            loading(true);

                            $("#customerInfo").unbind().submit();
                        }
                    }
                }
                else {
                    alert(obj_params.errors.str_errors.replace(/\--/g, '<br/>'));
                }
            }
            bol_firstTime = false;

        },
        fcn_reminderAlertChange = function ($obj_this) {
            if ($obj_this.is(":checked")) {
                obj_helpers.fcn_addValue($obj_this.closest("label").find("input[type=hidden]"), "True");
            }
            else {
                obj_helpers.fcn_addValue($obj_this.closest("label").find("input[type=hidden]"), "False");
            }
        },
        fcn_submitButtons = function ($obj_this) {
            $obj_this.closest("form").trigger("submit");
        },
        fcn_addressChange = function ($obj_this) {
            var $obj_option = $("#EcomOrderCustomerRegion > option:first");
            $("#updateUserAddress").val("False");
            $("#EcomOrderCustomerRegion").val($("#EcomOrderCustomerRegion > option:first").attr("value")).
                prop("disabled", true).
                html("").append($obj_option);
            for (var k in json_addresses[$obj_this.val()]) {
                $("#" + k).val(json_addresses[$obj_this.val()][k]);
                if (k == "EcomOrderCustomerCountry") {
                    $("#EcomOrderCustomerCountry").change();
                }
                if (k == "EcomOrderCustomerRegion" && json_addresses[$obj_this.val()][k] == "") {
                    $("#" + k).val($("#" + k + " option:first").attr("value")).prop("disabled", true);
                }
                else {
                    $("#" + k).prop("disabled", false);
                }
            }
            $('#customerInfo select').selectpicker('refresh');
        },
        fcn_address2Change = function ($obj_this) {
            var $obj_option = $("#EcomOrderDeliveryRegion > option:first");

            $("#EcomOrderDeliveryRegion").val($("#EcomOrderDeliveryRegion > option:first").attr("value")).
                prop("disabled", true).
                html("").append($obj_option);
            
            for (var k in json_addresses[$obj_this.val()]) {
                var key2 = k.replace("Customer", "Delivery");
                
                $("#" + key2).val(json_addresses[$obj_this.val()][k]);
                if (key2 == "EcomOrderDeliveryRegion" && json_addresses[$obj_this.val()][k] == "") {
                    $("#" + key2).val($("#" + key2 + " option:first").attr("value")).prop("disabled", true);
                }
                else {
                    $("#" + key2).prop("disabled", false);
                }
                if (key2 == "EcomOrderDeliveryCountry"){
                    $("#EcomOrderDeliveryCountry").change();        
                }
            }
            $('#customerInfo select').selectpicker('refresh');
        };

    obj_helpers.fcn_change($("#EcomOrderCustomerCountry"), fcn_countryChange);
    obj_helpers.fcn_change($("#EcomOrderDeliveryCountry"), fcn_countryChange);
    obj_helpers.fcn_change($("#EcomOrderCustomerRegion"), fcn_regionChange);
    obj_helpers.fcn_change($("#EcomOrderDeliveryRegion"), fcn_regionChange);
    obj_helpers.fcn_change($("input[name=deliveryaddress]"), fcn_deliveryAddress);
    obj_helpers.fcn_change($("#customerInformation > label > input[type=checkbox]"), fcn_reminderAlertChange);
    obj_helpers.fcn_change($("#selectAddress"), fcn_addressChange);
    obj_helpers.fcn_change($("#selectAddress2"), fcn_address2Change);
    obj_helpers.fcn_click($(".searchButton"), fcn_submitButtons, true);
    obj_helpers.fcn_addValue($("#EcomOrderCustomerCountry"), obj_params.submitedValues.str_customerCountry);

    if (obj_params.submitedValues.str_deliveryCountry != "") {
        obj_helpers.fcn_addValue($("#EcomOrderDeliveryCountry"), obj_params.submitedValues.str_deliveryCountry);
    }
    else {
        obj_helpers.fcn_addValue($("#EcomOrderDeliveryCountry"), $("#EcomOrderCustomerCountry").val());
    }

    fcn_regions($("#EcomOrderCustomerCountry"));
    fcn_regions($("#EcomOrderDeliveryCountry"));
    if ($("#EcomOrderCustomerCountry").val() == "") {
        $("#EcomOrderCustomerCountry").val(obj_user.str_country).change();
    }
    else {
        $("#EcomOrderCustomerCountry").change();
    }
    if ($("#EcomOrderCustomerCountry").val() != "" || $("#EcomOrderDeliveryCountry").val() != "") {
        fcn_countryChange();
    }
    if ($("#EcomOrderCustomerFirstName").val() != "" && $("#EcomOrderCustomerFirstName").val() != undefined && $("#EcomOrderCustomerFirstName").val() != "undefined") {
        $("#EcomOrderCustomerName").val($("#EcomOrderCustomerFirstName").val() + " " + $("#EcomOrderCustomerSurname").val());
    }

    if ($('[name=deliveryaddress]:checked').val() == "no") {
        $("#EcomOrderDeliveryName").val($("#EcomOrderCustomerName").val());
        $("#deliveryFormContainer .mandatory").addClass("mandatory_").removeClass("mandatory");
    }
    else {
        $("#deliveryFormContainer .mandatory_").addClass("mandatory").removeClass("mandatory_");
        $("#EcomOrderDeliveryCountry").change();
        $("[name=EcomOrderDeliveryRegion]").val($("[name=EcomOrderDeliveryRegion]").closest("fieldset").attr("data-region"));
    }
    $("[name=EcomOrderCustomerRegion]").val($("[name=EcomOrderCustomerRegion]").closest("fieldset").attr("data-region"));

    if (obj_params.errors.str_errors != "") {
        alert(obj_params.errors.str_errors.replace(/\--/g, '<br/>'));
    }
    fcn_checkErrors(null, null, null);

    if (obj_params.str_alertErrors != "") {
        alert(obj_params.str_alertErrors);
        window.location.href = window.location.href;
    }

    submitVoucher();

    $("#customerInfo").submit(function () {
        var bol_customerInfo = false,
            obj_ajax = {
                str_findEl:"#error, #addressValidatorsContainer"
            };

        $(".has-error").removeClass("has-error");
        if ($("input[name=deliveryaddress]:checked").val() == "no") {
            obj_helpers.fcn_loop($("#customerInformation input[type=text]"), fcn_applyDeliveryValues);
            obj_helpers.fcn_loop($("#customerInformation input[type=email]"), fcn_applyDeliveryValues);
            obj_helpers.fcn_loop($("#customerInformation select"), fcn_applyDeliveryValues);
            $("[name=EcomOrderDeliveryCountry]").change();
            $("[name=EcomOrderDeliveryRegion]").val($("[name=EcomOrderCustomerRegion]").val());
            $("#deliveryFormContainer .mandatory").addClass("mandatory_").removeClass("mandatory");
        }
        else {
            $("#deliveryFormContainer .mandatory_").addClass("mandatory").removeClass("mandatory_");
        }

        if (generalValidations($(this), true)) {
            $("#updater").val("CartV2.GotoStep0");
            obj_helpers.fcn_ajax($("#customerInfo").attr("action"), $("#customerInfo").serialize() + "&error=true", null, obj_ajax.str_findEl, fcn_checkErrors);
        }
        return bol_customerInfo;
    });

}

function onLoadCartStep2(str_alertErrors, webSeviceConnectionStatus) {
    var $obj_form = $("#shippingInfo"),
        $obj_datepicker = $("#datepicker"),
        $obj_termsContainer = $("#termsAndConditions"),
        $obj_termsClick = $("#termsAndConditionsClick"),
        $obj_updater = $("#updater"),
        obj_genericFunctions = {
            fcn_click:function ($obj_el, fcn_callBack, bol_preventDefault) {
                $obj_el.unbind("click").click(function (event) {
                    if (bol_preventDefault) event.preventDefault();
                    fcn_callBack($(this));
                });
            }
        },
        $obj_ajax,
        obj_module = {
            fcn_checkMethod:function ($obj_el) {
                var bol_return = true,
                    str_error = "";

                if (!$('input[name=EcomCartPaymethodID]').is(':checked')) {
                    str_error += $(".paymentTitle").html();
                }
                if (!$('input[name=EcomCartShippingmethodID]').is(':checked')) {
                    if (str_error != "") {
                        str_error += "<br/>";
                    }
                    str_error += $(".shippingTitle").html();
                }
                if (str_error != "") {
                    bol_return = false;
                    alert(str_error);
                }
                return bol_return;
            },
            fcn_alertTerms:function () {
                alert($obj_termsContainer.html());
            }
        };

    localStorage.removeItem(str_projectFolder + '_minicart');
    addressValidator();

    $('input[name=EcomCartShippingmethodID], input[name=EcomCartPaymethodID]').change(function () {
        if ($(this).is(':checked')) {
            $obj_updater.attr('name', 'CartV2.GotoStep1');
            loading(true);
            requestAjax({
                str_url:obj_pages.str_cartInformation,
                obj_data:$obj_form.serialize(),
                fnc_callback:function (obj_response) {
                    reloadCartStep(obj_response);
                }
            });
        }
        $obj_updater.attr('name', 'CartV2.GotoStep2');
    });

    $obj_datepicker.datepicker({
        showOn:"button",
        buttonImage:"/Files/Templates/Designs/" + str_projectFolder + "/images/calendar.gif",
        buttonImageOnly:true,
        buttonText:"Select date",
        dateFormat:"mm-dd-yy",
        onClose:function (selectedDate) {
            if (selectedDate != "") {
                selectedDate = selectedDate.replace(/\-/g, "/");
                $("#EcomOrderRecurringEndDate").val(selectedDate);
            }
        }
    });

    $("#expDateIcon").click(function () {
        $(".ui-datepicker-trigger").trigger("click");
    });

    submitVoucher();

    $obj_form.submit(function () {
        var bol_return = obj_module.fcn_checkMethod(),
            $obj_termsAndConditions = $(".termsAndConditions input[type=checkbox]");

        if ($obj_termsAndConditions.length && !$obj_termsAndConditions.is(':checked') && bol_return) {
            alert($obj_termsClick.attr("data-alert"));
            bol_return = false;
        }
        else if (bol_return) {
            bol_return = generalValidations($(this), true);
        }
        localStorage.setItem(str_projectFolder + '_orderSummary', $("#cartRightContent").html());
        
        return bol_return;
    });

    obj_genericFunctions.fcn_click($obj_termsClick, obj_module.fcn_alertTerms, true);

    if (str_alertErrors != "") {
        alert(str_alertErrors.replace(/\--/g, '<br/>'));
    }
    if (document.cookie.indexOf("checkout") != -1) {
        loading(true);
        document.cookie = 'checkout=;expires=Thu, 01 Jan 1970 00:00:01 GMT;';
        window.location = window.location;
    }

    /* todo check this */
    /*if ( webSeviceConnectionStatus == "" ) {
     setTimeout(function(){
     alert('Webserver unavailable');
     }, 1000);
     }*/

}

function onLoadCartStep3(str_title, str_print) {
    var fcn_shareYourOrder = function ($obj_el) {
        $obj_el.click(function (event) {
            var $obj_this = $(this),
                str_type = $obj_this.attr("id") == "shareToTwitter" ? "data-twitter" : "data-facebook";

            event.preventDefault();

            $obj_el.removeClass("active");
            $(this).addClass("active");
            $("#shareContainer").remove();
            $obj_this.parent().append("<div id='shareContainer' class='col-xs-12'><h2>" + $obj_this.closest(".blockContent").find("h2").html() + "</h2><a href='javascript:void(0)'><i class='fa fa-times'></i></a></div>");
            $("#shareContainer h2").after($(".cartProductsList").clone());
            $obj_this.parent().find(".cartProductsList > li a").attr("target", "_blank");
            $obj_this.parent().find(".cartProductsList > li *").removeClass();
            $obj_this.parent().find(".cartProductsList > li img").addClass("img-responsive");
            $obj_this.parent().find(".cartProductsList > li").addClass("col-xs-12 col-sm-6 col-md-4").removeClass("noPadding");

            for (var i = 0; i < $obj_this.parent().find(".cartProductsList > li").length; i++) {
                $obj_this.parent().find(".cartProductsList > li").eq(i).find(" > div").not(":first").remove();
                $obj_this.parent().find(".cartProductsList > li").eq(i).find("a").attr("href", $obj_this.parent().find(".cartProductsList > li").eq(i).attr(str_type));
            }
            $obj_this.parent().find(".cartProductsList > li p").remove();

            $("#shareContainer > a").click(function (event) {
                event.preventDefault();

                $obj_el.removeClass("active");
                $(this).parent().remove();
            });

        });
    },
        fcn_print = function () {
            if (str_print == "print") {
                // elements to remove on print
                //$("#footer, .continueButton, #createCartUser, #topMenuContainer, #menuMainContainer").remove();
                window.print();
            }
        };

    localStorage.removeItem(str_projectFolder + '_minicart');
    localStorage.removeItem(str_projectFolder + '_orderSummary');

    if ($("body.print").length) {
        window.print();
    }
    // Create user form
    $("form[name=UserManagementEditForm]").submit(function () {
        if (generalValidations($(this), true)) {
            var $obj_this = $(this);

            loading(true);
            requestAjax({
                str_url:$obj_this.attr("action"),
                str_type:"post",
                obj_data:$obj_this.serialize(),
                fnc_callback:function (obj_response) {
                    if ($(obj_response).find("#formValidations").length > 0) {
                        $("#createCartUser > div").html("<h2><i class='fa fa-close'></i>" + $(obj_response).find("#formValidations").html() + "</h2>");
                    }
                    else {
                        $("#createCartUser > div").html("<h2><i class='fa fa-check'></i>" + $("[data-success]").attr("data-success") + "</h2>");
                    }
                    loading(false);
                }
            });
        }

        return false;
    });
    // Change title
    if (str_title != null) $("#contentWrapper .h1:first").html(str_title);

    fcn_shareYourOrder($("#shareToTwitter, #shareToFacebook"));
    fcn_print();

}

function onLoadCartLogin() {
    localStorage.removeItem(str_projectFolder + '_minicart');
    $("div.h1").remove();
    $("#centralContent").addClass("noMarginTop");
    $("#returningLogin > div:first form")
        .addClass("col-sm-6")
        .after($("#ExtUserForm #dontHaveAnAccount"));

    $("form[data-checkoutpage] input[name=redirect]").val($("form[data-checkoutpage]").attr("data-checkoutpage"));
    $("form[data-checkoutpage]").attr("action", $("form[data-checkoutpage]").attr("data-mycart"));
    // change forgot password href.. to the login page
    $("#forgotPassword").attr("href", $("li.login > a").attr("href") + "?LoginAction=Recovery");

}

function onLoadPaymentEbiz() {
    var $obj_form = $("#ebizForm"),
        $obj_select = $obj_form.find("select"),
        $obj_stepsLi = $("#stepsList").find('li');

    $("#cartRightContent").append(localStorage.getItem(str_projectFolder + '_orderSummary'));
    $obj_select.selectpicker({size:8});

    $obj_stepsLi.removeClass("active");
    $obj_stepsLi.eq(2).addClass("active");
    
    $("#CardTokenName").blur(function () {
        var $cartToken = $("#chkSaveCardToken");
        if ($(this).val() !== "") {
            $cartToken.prop("checked", true);
        }
        else {
            $cartToken.prop("checked", false);
        }
    });

    $obj_select.change(function () {
        if ($(this).val() !== "") {
            $obj_form.find(".newCardContent input").prop("disabled", true);
            $obj_form.find(".newCardContent").addClass("hidden");
            $obj_form.find(".newCardContent .mandatory").addClass("_mandatory").removeClass("mandatory");
        }
        else {
            $obj_form.find(".newCardContent input").prop("disabled", false);
            $obj_form.find(".newCardContent").removeClass("hidden");
            $obj_form.find(".newCardContent ._mandatory").addClass("mandatory").removeClass("_mandatory");
        }
    });

    $obj_form.submit(function () {
        return generalValidations($obj_form, true);
    });
}

function onLoadPaymentStripe(stripeKey) {
    var $paymentForm = $('#payment-form'),
        $purchaseForm = $("#purchaseForm"),
        $savedCardName = $("#savedCardName"),
        $cardName = $purchaseForm.find("input[name='CardTokenName']"),
        $stripeToken = $purchaseForm.find("input[name='stripeToken']"),
        $stripeTokenType = $purchaseForm.find("input[name='stripeTokenType']");

    $("#stepsList > li").removeClass("active");
    $("#stepsList > li:last").addClass("active");

    Stripe.setPublishableKey(stripeKey);

    var stripeResponseHandler = function (status, response) {
        if (response.error) {
            loading(false);
            alert(response.error.message);
        }
        else {
            $stripeToken.val(response.id);
            $stripeTokenType.val(response.type);
            $cardName.val($savedCardName.val());
            $purchaseForm.submit();
        }
    };

    $paymentForm.submit(function (e) {
        loading(true);
        Stripe.card.createToken($paymentForm, stripeResponseHandler);
        return false;
    });
}

function onLoadShoppingCart(str_alert, str_confirm) {
    localStorage.removeItem(str_projectFolder + '_minicart');
    $(".productCount").remove();
    minicart();
    var obj_elements = {
        $obj_form:$("#productContent .quantity form"),
        $obj_remove:$(".removeFromCart"),
        $obj_checkout:$(".gotoCheckout"),
        $obj_update:$(".cartPromo"),
        $obj_select:$("select"),
        $obj_quantityInput:$(".quantityInput")
    },
        obj_genericFunctions = {
            fcn_click:function ($obj_el, fcn_callBack, bol_preventDefault) {
                $obj_el.unbind("click").click(function (event) {
                    if (bol_preventDefault) event.preventDefault();
                    fcn_callBack($(this));
                });
            },
            fcn_change:function ($obj_el, fcn_callBack) {
                $obj_el.change(function () {
                    fcn_callBack($(this));
                });
            }
        },
        obj_module = {
            fcn_remove:function ($obj_this) {
                var str_txt = confirm(str_confirm);

                loading(true);
                if (str_txt == true) {
                    window.location = $obj_this.attr("href");
                }
                else {
                    loading(false);
                }

            },
            fcn_removeVoucher:function () {
                $(".removeVoucher").click(function () {
                    loading(true);
                    $("input[name=EcomOrderVoucherCode]").val("");
                    $(".cartPromo").submit();
                });
            },
            fcn_changeQuantity:function ($el) {
                if ($el.val() == "10+") {
                    $el.closest("form").find(".hidden").removeClass("hidden");
                    $el.closest("form").find(".bootstrap-select").addClass("hidden");
                    $el.closest("form").find("input.quantityInput").val("10");
                }
                else {
                    $el.closest("form").find("input.quantityInput").val($el.val());
                    $el.closest("form").submit();
                }
            },
            fcn_checkout:function ($el) {
                $("#showCart").submit();
            }
        };

    obj_genericFunctions.fcn_change($("select[name=quantitySelect]"), obj_module.fcn_changeQuantity);
    obj_genericFunctions.fcn_click(obj_elements.$obj_remove, obj_module.fcn_remove, true);
    obj_genericFunctions.fcn_click(obj_elements.$obj_checkout, obj_module.fcn_checkout, true);
    obj_elements.$obj_form.submit(function () {
        loading(true);
    });
    obj_elements.$obj_update.submit(function () {
        loading(true);
    });
    obj_elements.$obj_quantityInput.blur(function () {
        $(this).closest("form").submit();
    });
    obj_elements.$obj_select.selectpicker({size:8});
    obj_module.fcn_removeVoucher();

    if (document.referrer != document.URL && document.referrer != "") {
        $('.goBackShopping').attr("href", document.referrer);
    }

    if (str_alert != "") {
        alert(str_alert);
        window.location.href = window.location.href;
    }
}

function addressValidator() {
    var $obj_container = $("#addressValidators"),
        fcn_change = function () {
            var $obj_label = $("#addressValidators label");

            for (var i = 0; i < $obj_label.length; i++) {
                if ($obj_label.eq(i).find("input:checked").length > 0) {
                    var type = $obj_label.eq(i).attr("data-type") == "Billing" ? "EcomOrderCustomer" : "EcomOrderDelivery";

                    switch ($obj_label.eq(i).attr("data-field")) {
                        case "AddressLine1":
                            $("[name=" + type + "Address]").val($obj_label.eq(i).find(".newValue").html());
                            break;
                        case "ZipCode":
                            $("[name=" + type + "Zip]").val($obj_label.eq(i).find(".newValue").html());
                            break;
                        case "Region":
                            $("[name=" + type + "Region]").val($obj_label.eq(i).find(".newValue").html());
                            break;
                        case "City":
                            $("[name=" + type + "City]").val($obj_label.eq(i).find(".newValue").html());
                            break;
                    }
                }
            }
            $('#formContainer select').selectpicker('refresh');
            $("#updateUserAddress").val($("#updateExistingAddress:checked").length > 0 ? "True" : "False");
        },
        $obj_selectedAddress = $("#selectAddress option:selected");

    if ($obj_selectedAddress.length > 0)
    {
        if ($obj_selectedAddress.data("isdefaultaddress").toLowerCase() == "false")
        {
            $("#updateExistingAddressContainer").remove();
        } 
    }    
    
    $("#changeAll").change(function () {
        if ($(this).prop("checked")) {
            $(this).closest(".modal-content").find("input").not("#changeAll, #updateExistingAddress").prop("checked", true);
        }
        else {
            $(this).closest(".modal-content").find("input").not("#changeAll, #updateExistingAddress").prop("checked", false);
        }
    });
    if ($(".addressChange").length > 0) {
        if ($obj_container.length > 0) customAlert($obj_container, "", "change", "Change", true, fcn_change);
    }

}

function minicart(obj_data) {
    var obj_myCartLi = $("#serviceMenu > li.myCart"),
        fcn_getMiniCart = function(obj_response){
            localStorage.setItem(str_projectFolder + '_minicart', obj_response);

            var obj_minicart = $(obj_response).find("#miniCart"),
                num_quantity = obj_minicart.length ? obj_minicart.attr("data-quantity") : 0;


            if (num_quantity > 0) {
                $("#miniCart").remove();
                if (obj_myCartLi.find("span.productCount").length) {
                    obj_myCartLi.find("span.productCount").html(" (" + num_quantity + ")");
                }
                else {
                    obj_myCartLi.find("a").append("<span class='productCount'> (" + num_quantity + ") </span>");
                }
                obj_myCartLi.append(obj_minicart);
            }
            $(".miniCartBtns  > a:first").attr("href", $("li.myCart > a").attr("href"));
        };

    if (obj_myCartLi.length > 0 && obj_data == null) {

        if ( performance.navigation.type == 2 || localStorage.getItem(str_projectFolder + '_minicart') == null) {
            requestAjax({
                str_url:obj_pages.str_miniCart,
                str_type:"get",
                obj_data:"html",
                fnc_callback:function (obj_response) {
                    fcn_getMiniCart(obj_response);
                }
            });
        }
        else {
            fcn_getMiniCart(localStorage.getItem(str_projectFolder + '_minicart'));
        }

    }
    else {
        fcn_getMiniCart(obj_data);
    }
}

function reloadCartStep(obj_response) {
    var cart = $.parseJSON(obj_response),
        $obj_shippinhMethodName = $("#shippingMethodName"),
        $obj_shippingMethodPrice = $("#shippingMethodPrice");

    $("#orderListTotal").html(cart.totalPrice);
    if (cart.shippingMethod != "") {
        $obj_shippingMethodPrice.html(cart.shippingFee);
        $obj_shippingMethodPrice.removeClass("hidden");
        $obj_shippinhMethodName.html(cart.shippingMethod);
        $obj_shippinhMethodName.removeClass("hidden");
    }
    if (cart.paymentMethod != "") {
        $("#paymentMethodName").html(cart.paymentMethod);
        $("#paymentMethodLiContainer").removeClass("hidden");
    }
    loading(false);
}

function submitVoucher() {
    var $obj_form = $("#customerInfo").length ? $("#customerInfo") : $("#shippingInfo");
    $("a.submitVoucher").unbind().click(function () {
        loading(true);
        requestAjax({
            str_url:$obj_form.attr('action') + "&" + obj_globalOptions.obj_urlPaths.str_ajaxRetriever,
            obj_data:"EcomOrderVoucherCode=" + $("[name=EcomOrderVoucherCode]").val(),
            fnc_callback:function (obj_response) {
                $("#promoCodeContainer").html($(obj_response).find("#promoCodeContainer").html());
                $("#cartRightContent").html($(obj_response).find("#cartRightContent").html());
                loading(false);
                if($("#promoCodeContainer").attr("data-error") != ""){
                    alert($("#promoCodeContainer").attr("data-error"));
                }
                submitVoucher();
            }
        });
    });
    $(".removeVoucher").unbind().click(function () {
        loading(true);
        requestAjax({
            str_url:$obj_form.attr('action') + "&" + obj_globalOptions.obj_urlPaths.str_ajaxRetriever,
            obj_data:"EcomOrderVoucherCode=",
            fnc_callback:function (obj_response) {
                $("#promoCodeContainer").html($(obj_response).find("#promoCodeContainer").html());
                $("#cartRightContent").html($(obj_response).find("#cartRightContent").html());
                loading(false);
                submitVoucher();
            }
        });
    });
}
