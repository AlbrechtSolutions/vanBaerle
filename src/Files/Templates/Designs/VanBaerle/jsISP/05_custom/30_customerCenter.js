/**
 * Description: Customer Center
 * User: ShareIT
 * Date: 12/Jan/2016
 **/

function onLoadOrderList(){
  $(".orderList a").click(function(event){
    event.preventDefault();

    if($(this).attr("href").indexOf("Asc") > -1){
      $(this).attr("href", $(this).attr("href").replace("Asc", "Desc"));
    }
    else {
      $(this).attr("href", $(this).attr("href").replace("Desc", "Asc"));
    }
    window.location = $(this).attr("href");
  });
}

function onLoadRMADetails(obj_settings) {
    var $obj_form = $('#' + obj_settings.str_formID),
        $obj_checkboxes = $('#myOrdersTable input[type=checkbox]').not("#all"),
        $obj_selects = $('.Select select'),
        $obj_fakeSelect = $("select[name=fakeSelect]"),
        fcn_fileInput = function ($obj_inputFiles) {
            for (num_index = 0; num_index < $obj_inputFiles.length; num_index++) {
                if (!$($obj_inputFiles[num_index]).hasClass("doneAlready")) {
                    $($obj_inputFiles[num_index]).addClass("doneAlready");
                    $($obj_inputFiles[num_index]).parent().append("<div class='fakefile'><input class='fakeInput' /><button type='button' class='btn btn-bg'>Browse</button></div>");
                    $($obj_inputFiles[num_index]).change(function () {
                        $(this).siblings(".fakefile").children('input').val($(this).val());
                    });
                    $($obj_inputFiles[num_index]).parent().find(".fakefile").click(function () {
                        $(this).parent().find("input[type=file]").click();
                    });

                }
            }
        },
        fcn_showComments = function (obj_this, bol_show) {
            var qty = obj_this.closest("tr").find(".filter-option").length ? parseInt(obj_this.closest("tr").find(".filter-option").text()) : 1 ;

            if ($(".prodComment").length) {
                if (bol_show) {
                    $("tr[prodid=" + obj_this.attr("data-prodid") + "]").removeClass("hide");
                    $("tr[prodid=" + obj_this.attr("data-prodid") + "]").find("div").addClass("hidden");
                    for (var i = 1; i <= qty; i++) {
                        $("tr[prodid=" + obj_this.attr("data-prodid") + "]").find("div.item_" + i).removeClass("hidden");
                    }
                }
                else {
                    $("tr[prodid=" + obj_this.attr("data-prodid") + "]").addClass("hide");
                    $("tr[prodid=" + obj_this.attr("data-prodid") + "]").find("div").addClass("hidden");
                }
            }
        };
    fcn_fileInput($("input[type=file]"));
    $obj_checkboxes.click(function () {
        setRmaItem(this.checked, $(this).attr("data-serial"), $(this).attr("data-productComment"))
    });
    $("#all").change(function () {
        if ($(this).is(":checked")) {
            $obj_checkboxes.prop("checked", true);
            for(var s=0; s < $obj_fakeSelect.length; s++){
                $obj_fakeSelect.eq(s).val($obj_fakeSelect.eq(s).find("option:last").attr("value"));
            }
            $obj_fakeSelect.selectpicker('refresh');
            for(var i=0; i < $("#myOrdersTable tbody input[type=checkbox]").length ; i++){
                fcn_showComments($("#myOrdersTable tbody input[type=checkbox]").eq(i), true);
            }
        }
        else {
            $obj_checkboxes.prop("checked", false);
            for(var s=0; s < $obj_fakeSelect.length; s++){
                $obj_fakeSelect.eq(s).val($obj_fakeSelect.eq(s).find("option:first").attr("value"));
            }
            $obj_fakeSelect.selectpicker('refresh');
            for(var i=0; i < $("#myOrdersTable tbody input[type=checkbox]").length ; i++){
                fcn_showComments($("#myOrdersTable tbody input[type=checkbox]").eq(i), false);
            }
        }
    });

    $obj_selects.change(function () {
        loading(true);
        $obj_form.unbind().submit();
    });

    $("#myOrdersTable [name=fakeSelect]").change(function () {
        var arr_values = $(this).val().split(","),
            $obj_this = $(this);

        $obj_this.closest("tr").find("[type=checkbox]").prop("checked", false);
        for (var i = 0; i < arr_values.length; i++) {
            $("[data-id=" + arr_values[i] + "]").prop("checked", true);
        }
        setTimeout(function () {
            fcn_showComments($obj_this, true);
        }, 200);
    });

    $("input.primary").change(function () {
        if (!$(this).is(':checked')) {
            $(this).closest("label").find("[type=checkbox]").prop("checked", false);
            $(this).closest("tr").find("select").val("1");
            $('select').selectpicker("refresh");
            fcn_showComments($(this), false);
        }
        else {
            fcn_showComments($(this), true);
        }
    });

    $obj_form.unbind().submit(function () {
        var bol_isCreate = Boolean($obj_form.attr("data-isCreate") === "1"),
            bol_return = true;

        if (!generalValidations($('#' + obj_settings.str_formID), true)) {
            bol_return = false;
        }
        else if (!onValidateForm($obj_form.attr("data-PID"), 'myOrdersTable', bol_isCreate, obj_settings)) {
            bol_return = false;
        }

        return bol_return;

    });

    $('select').selectpicker();
}

function onLoadWishList(str_type) {
  var fcn_removeFromList = function ($obj_this) {
    var obj_ajax,
        $obj_list = $("#productsList"); // UL - products list ID

    loading(true);
    if ( typeof obj_ajax != "undefined" ) {
      obj_ajax.abort();
    }

    obj_ajax = $.ajax({
      url:$obj_this.attr("href"),
      success:function (obj_data) {},
      complete:function () {
        $obj_this.closest("li").hide();
        loading(false);
      }
    });
  },
      fcn_addToCart = function ($obj_this) {
        var obj_ajax;

        loading(true);
        if ( typeof obj_ajax != "undefined" ) {
          obj_ajax.abort();
        }

        obj_ajax = $.ajax({
          url:$obj_this.attr("action"),
          data:$obj_this.serialize(),
          method: "POST",
          success:function (data) {
            obj_data = data;
          },
          complete:function () {
            minicart(obj_data);
            $obj_this.find(".btn").addClass("highlighted");

            $obj_this.find(".btn span").html($obj_this.find(".btn span").attr("data-added"));
            setTimeout(function() {
                $obj_this.find(".btn span").html($obj_this.find(".btn span").attr("data-add"));
                $obj_this.find(".btn").removeClass("highlighted");
              },
              3000);
            loading(false);
          }
        });
      },
      fcn_createWishlist = function () {
        $("#AddListForm").submit(function () {
          return generalValidations($(this), true);
        });
        if ( str_type != "" || str_type != "showLists" ) $("div.h2").html($("#createTitle").html());
      },
      fcn_changeList = function () {
        var $obj_form = $('#changeList'),
            $obj_select = $obj_form.find("select"),
            obj_ajax,
            str_data;

        $obj_select.selectpicker({size:8});

        $obj_select.change(function () {
          loading(true);
          if ( typeof obj_ajax != "undefined" ) {
            obj_ajax.abort();
          }

          obj_ajax = $.ajax({
            url:$obj_form.attr("action"),
            data:$obj_form.serialize() + "&" + obj_globalOptions.obj_urlPaths.str_ajaxRetriever,
            success:function (obj_data) {
              str_data = $(obj_data);
            },
            complete:function () {
              var $obj_prodList = $("#productsList");

              if ( str_data.find("#productsList").length ) {
                $obj_prodList.html(str_data.find("#productsList").html());
                fcn_events();
              }
              else {
                $("#productsList").html("<li><h3>" + $('[data-noproducts]').attr('data-noproducts') + "</h3></li>");
              }
              if (str_data.find("#favoriteAllList").length){
                $("#favoriteAllList").html(str_data.find("#favoriteAllList").html());
                fcn_events();
              }
              loading(false);
            }
          });
        });

      },
      fcn_move = function () {
        var $obj_form = $('form[name=change]'),
            $obj_select = $obj_form.find("select"),
            $obj_selectChange = $("form[name=change] select"),
            str_moveOption = $obj_select.find("option:first").html(),
            obj_ajax,
            str_data;


        $obj_selectChange.html($("#changeList select").html());
        $obj_selectChange.find("option:first").html(str_moveOption);

        $obj_select.selectpicker({size:8});

        $obj_select.change(function () {
          var $obj_this = $(this),
              str_formAction = $obj_this.closest("form").attr("action"),
              str_formSerialize = $obj_this.closest("form").serialize();

          $obj_this.closest("li").find(".removeFromList").trigger("click");

          loading(true);
          if ( typeof obj_ajax != "undefined" ) {
            obj_ajax.abort();
          }

          obj_ajax = $.ajax({
            url:str_formAction,
            data:str_formSerialize,
            success:function (obj_data) {
              str_data = $(obj_data);
            },
            complete:function () {
              if ( $("#productsList > li:visible").length > 0 ) {
                loading(false);
              }
              else {
                loading(true);
                window.location = window.location;
              }
            }
          });
        });
      },
      fcn_events = function () {
        // add to cart
        $(".addToCart").unbind("click").click(function (event) {
          event.preventDefault();
          if (parseInt($(this).attr("data-stock")) > 0){
            fcn_addToCart($(this).closest("form"));
          }
          else {
              alert($(this).attr("data-outofstock"));
          }
        });
        //Add all to the cart
        $("#favoriteAllList").unbind().submit(function() {
          fcn_addToCart($("#favoriteAllList"));
          return false;
        });
        // remove from wishlist
        $(".removeFromList").unbind("click").click(function (event) {
          event.preventDefault();
          fcn_removeFromList($(this));
        });
        fcn_move();
      };

  // All events
  fcn_events();
  // Create new list
  fcn_createWishlist();
  // change list
  fcn_changeList();
  // Move product to list
  fcn_move();

}

function onValidateForm(paragraphID, orderLinesContainerId, isCreateRMA, obj_settings) {
  var isFormValid = true,
      $obj_comment = $('#' + paragraphID + "RMAComment");

  if ( isCreateRMA ) {
    if ( document.getElementById(paragraphID + "RMATypeID") != null ) {
      var typeEl = document.getElementById(paragraphID + "RMATypeID");

      if ( typeEl.selectedIndex == -1 || (typeEl.selectedIndex >= 0 && typeEl.options[typeEl.selectedIndex].value == "") ) {
        alert(obj_settings.str_requestType);
        isFormValid = false;
        typeEl.focus();
      }
    }

    if ( isFormValid && orderLinesContainerId != null && orderLinesContainerId != "" && $('#myOrdersTable input[type="checkbox"]').length && !validateSerialNumbers(paragraphID, orderLinesContainerId) ) {
      alert(obj_settings.str_select);
      isFormValid = false;
    }
  }

  if ( !isCreateRMA && isFormValid && document.getElementById(paragraphID + "RMAComment") != null && document.getElementById(paragraphID + "RMAComment").value.length < 1 ) {
    alert(obj_settings.str_comment);
    document.getElementById(paragraphID + "RMAComment").focus();
    isFormValid = false;
  }

  if ( isCreateRMA && isFormValid && ($obj_comment != null && $obj_comment.val() == "") ) {
    alert(obj_settings.str_additionalInfo);
    document.getElementById(paragraphID + "RMAComment").focus();
    isFormValid = false;
  }

  if ( isFormValid && document.getElementById(paragraphID + "HasSubmit") != null ) {
    document.getElementById(paragraphID + "HasSubmit").value = "1";
  }

  return isFormValid;
}

function setRmaItem(active, pnCtrlId, commentCtrlId) {
  if ( pnCtrlId && pnCtrlId != "" && commentCtrlId && commentCtrlId != "" ) {
    var ctrl = document.getElementById(pnCtrlId);

    if ( ctrl && ctrl != null ) {
      ctrl.value = "";
      ctrl.disabled = !active;
    }

    var ctrl = document.getElementById(commentCtrlId);

    if ( ctrl && ctrl != null ) {
      ctrl.value = "";
      ctrl.style.display = !active ? "none" : "";
      //$(ctrl).toggleClass("activeTextarea");
    }
  }
}

function validateSerialNumbers(paragraphID, orderLinesContainerId) {
  var result = true;
  var container = document.getElementById(orderLinesContainerId);

  if ( container ) {
    result = false;

    var checkBoxes = new Array();

    for ( var i = 0 ; i < container.childNodes.length ; i++ ) {
      var childArrays = getCheckBoxes(container.childNodes[i]);

      if ( childArrays && childArrays.length > 0 ) {
        for ( var j = 0 ; j < childArrays.length ; j++ ) {
          checkBoxes.push(childArrays[j]);
        }
      }
    }

    if ( checkBoxes && checkBoxes.length > 0 ) {
      var preffixId = paragraphID + "OrderLineID";
      for ( var i = 0 ; i < checkBoxes.length ; i++ ) {
        if ( checkBoxes[i].id.indexOf(preffixId) >= 0 && checkBoxes[i].checked ) {
          result = true;
          break;
        }
      }
    }
  }

  return result;
}

function getCheckBoxes(el) {
  var result = new Array();

  if ( el ) {
    var child = null, serNumberEl = null;

    for ( var i = 0 ; i < el.childNodes.length ; i++ ) {
      child = el.childNodes[i];
      if ( child && typeof child.type != "undefined" && child.type == "checkbox" )
        result.push(child);

      var childArrays = getCheckBoxes(child);

      if ( childArrays && childArrays.length > 0 ) {
        for ( var j = 0 ; j < childArrays.length ; j++ ) {
          result.push(childArrays[j]);
        }
      }
    }
  }

  return result;
}

function rmaTableAdjust() {
  var $obj_form = $('.rmaDetailsForm');
  $('#myOrdersTable .checkBoxContainer input').click(function () {
    setRmaItem(this.checked, $(this).attr("data-serial"), $(this).attr("data-productComment"));
  });
  checkAndRadio();

  $obj_form.unbind("submit").submit(function () {
    var bol_isCreate = Boolean($('.rmaDetailsForm').attr("data-isCreate") === "1");

    return onValidateForm($obj_form.attr("data-PID"), 'myOrdersTable', bol_isCreate);
  });
}