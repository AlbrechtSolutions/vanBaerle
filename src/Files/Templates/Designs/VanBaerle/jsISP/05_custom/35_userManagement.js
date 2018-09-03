/**
 * Description: User Management
 * User: ShareIT
 * Date: 12/Jan/2016
 **/

ShareIT.UserManagement = {
  num_toggleForgotPassword:0,
  fcn_login:{
    fcn_init:function ($obj_form) {
      $obj_form.submit(function () {
        return generalValidations($obj_form, true);
      });
    }
  }
};

function onLoadLoginForm(str_form) {
  $('#Username').focus();
  ShareIT.UserManagement.fcn_login.fcn_init($(str_form));
}

function onLoadAddAddresses() {
  var $obj_form = $("#AddressForm"),
      $obj_selects = $("select");

  $obj_form.unbind().submit(function () {
    return generalValidations($obj_form, true);
  });

  $("#centralContent > h2.first").html($(".titleJs").html());
  changeCountry();
  $("#Country").val(obj_user.str_country).change();
  if ( obj_user.str_state != "" ) {
    $("#State").val(obj_user.str_state).change();
  }
  //if ( $obj_selects.length ) $obj_selects.selectpicker({width:"50%", size:8});
  $obj_selects.selectpicker("refresh");
}

function onLoadCreateUser(obj_selected) {
  var $obj_selects = $(".userRelatedForms").find('.Select select');

  if ( $obj_selects.length ) $obj_selects.selectpicker({ width:"100%", size:8 });
  checkUsername();
  changeCountry(obj_selected.str_state);
  if ( obj_selected.str_country != "" ) {
    $("#UserManagement_Form_Country").val(obj_selected.str_country).change();
  }
  else {
    $("#UserManagement_Form_Country").val(obj_user.str_country).change();
  }
  $obj_selects.selectpicker("refresh");

  $("#UserManagement_Form_EmailAllowed").change(function () {
    if ( $(this).val() == "" ) {
      $(this).val("True")
    }
    else {
      $(this).val("")
    }
  });

  if ( obj_selected.errorMessage != "" ) {
    alert(obj_selected.errorMessage);
  }
}

function onLoadListAddresses() {
  $(".viewAddresses .default").click(function (event) {
    event.preventDefault();
    loading(true);
    makeDefaultAddress($(this).attr("data-id"));
  });
  $(".viewAddresses .delete").click(function (event) {
    event.preventDefault();
    loading(true);
    deleteAddress($(this).attr("data-id"));
  });
  $(".viewAddresses .edit").click(function (event) {
    event.preventDefault();
    loading(true);
    updateAddress($(this).attr("data-id"));
  });
}

function onLoadForgotPassword() {
  $('#ExtUserForm').submit(function () {
    return generalValidations($('#ExtUserForm'), true);
  });
}

function onLoadValidateNewsletter() {
  $("#UserManagementEditForm").submit(function () {
    return generalValidations($(this), true);
  });
}

function changeCountry(str_state) {
  var $obj_countrySelect = $('#UserManagement_Form_Country'),
      $obj_regionSelect = $('#UserManagement_Form_State'),
      str_selectedCode = "";

  if ( $("#Country").length ) {
    $obj_countrySelect = $('#Country');
    $obj_regionSelect = $('#State');
  }

  $obj_countrySelect.change(function () {
    str_selectedCode = $(this).val();
    $obj_regionSelect.html("");
    $obj_regionSelect.append("<option value='' selected='selected'>" + $obj_regionSelect.attr('data-text') + "</option>");
    fcn_changeTexts($(this), "State");
    if ( typeof countryRegions[str_selectedCode] != "undefined"
             && typeof countryRegions[str_selectedCode].code != "undefined"
        && countryRegions[str_selectedCode].code.length > 0 ) {
      for ( var i = 0 ; i < countryRegions[str_selectedCode].code.length ; i++ ) {
        $obj_regionSelect.append("<option value='" + countryRegions[str_selectedCode].code[i] + "'>" + countryRegions[str_selectedCode].name[i] + "</option>");
      }
      if ( str_state != null && str_state != "" ) $obj_regionSelect.val(str_state);
    }
    else {
      $obj_regionSelect.append("<option value='default' disabled='disabled'> ---- </option>");
    }
    $obj_regionSelect.selectpicker("refresh");
  });
}

function fcn_changeTexts($obj_this, str_regionOrState) {
  var str_labels = $obj_this.attr("name").substring(0, $obj_this.attr("name").indexOf("Country")),
      str_thisVal = $obj_this.val() == "US" || $obj_this.val() == "CA" ? $obj_this.val() : "",
      $obj_zipLabel = $("label[for='" + str_labels + "Zip']"),
      $obj_regionLabel = $("label[for='" + str_labels + str_regionOrState + "']"),
      $obj_regionSelect = $("Select#" + str_labels + str_regionOrState);

  fcn_changeRegions($obj_this, str_regionOrState);
  $obj_zipLabel.text($obj_zipLabel.attr("data-text" + str_thisVal));
  $obj_regionLabel.text($obj_regionLabel.attr("data-text" + str_thisVal));
  $obj_regionSelect.find('option.defaultOption').text($obj_regionLabel.attr("data-text" + str_thisVal));
  $obj_zipLabel.parent().find("input[type=text]").attr("placeholder", $obj_zipLabel.attr("data-text" + str_thisVal));
  $obj_regionLabel.parent().find("input[type=text]").attr("placeholder", $obj_regionLabel.attr("data-text" + str_thisVal));

  if ( !$obj_regionSelect.prop("disabled") ) {
    $obj_regionSelect.closest("fieldset").addClass("mandatory");
  }
  else {
    $obj_regionSelect.closest("fieldset").removeClass("mandatory");
  }
  $obj_regionSelect.selectpicker("refresh");
}

function fcn_changeRegions($obj_this, str_regionOrState) {
  var str_name = $obj_this.attr("name").replace("Country", str_regionOrState),
      $obj_region = $("#" + str_name),
      str_val = typeof $obj_region.attr("data-value") != "undefined" ? $obj_region.attr("data-value") : "";

  if ( $($obj_region[0]).next().is('div.bootstrap-select') && !$obj_region.is($("#" + str_name + " + div.bootstrap-select")) ) {
    $obj_region = $obj_region.add("#" + str_name + " + div.bootstrap-select");
  }

  if ( $obj_this.val() == "CA" || $obj_this.val() == "US" ) {
    $obj_region.removeAttr("disabled").attr("name", str_name).removeClass("hidden");
    $("#" + str_name + "_2").remove();
  }
  else {
    $obj_region.attr("disabled", "disabled").attr("name", "").addClass("hidden");
    if ( !$("#" + str_name + "_2").length ) {
      $($obj_region).parents("fieldset.Select").append("<input type='text' id='" + str_name + "_2' name='" + str_name + "' value='" + str_val + "' />");
    }
  }
  if ( $obj_this.val() == "" ) {
    $("#" + str_name + "_2").attr("readonly", "readonly");
    $obj_region.attr("readonly", "readonly");
  }
  else {
    $("#" + str_name + "_2").removeAttr("readonly");
    $obj_region.removeAttr("readonly");
  }
}

function checkUsername() {
  var $obj_form = $('form.userRelatedForms:not(#masterExtUserForm)'),
      $obj_usernameTextBox = $obj_form.find('#UserManagement_Form_UserName'),
      bol_submit = false;

  $obj_form.submit(function () {
    if ( $obj_form.find("#UserManagement_Form_Email").length ) $obj_form.find("#UserManagement_Form_UserName").val($obj_form.find("#UserManagement_Form_Email").val());
    $obj_form.find("#UserManagement_Form_Name").val($obj_form.find("#UserManagement_Form_FirstName").val() + " " + $obj_form.find("#UserManagement_Form_LastName").val());

    if ( !$('div.bootbox.modal').length ) {
      if ( generalValidations($obj_form, true) ) {
          bol_submit = true;
        loading(true);
          /*
        $.ajax({
          type:"POST",
          url:$obj_form.attr("action"),
          data:$obj_form.serialize(),
          dataType:"html",
          success:function (obj_data) {
            loading(false);
            if ( $(obj_data).find('#userNameTaken').length ) {
              alert($(obj_data).find('#userNameTaken').text(), obj_formErrorMessages.str_errorMessagesTitle);
              $obj_usernameTextBox.parents('fieldset').addClass("fieldError2");
            }
            else {
              if(!$(obj_data).find("#ExtUserForm").length) {
                $("#contentWrapper").html($(obj_data).find("#contentWrapper").html());
              }
              parseScript(obj_data);
            }

          }
        });
        */
      }
    }
    return bol_submit;
  });
}

/* DW Functions */

function makeDefaultAddress(addressId) {
  document.getElementById('SelectedAddressId').value = addressId;
  document.getElementById('ManageAddressesFormAction').value = 'MakeDefault';
  document.getElementById('UserManagementManageAddressesForm').submit();
}

function deleteAddress(addressId) {
  loading(false);
  if ( document.getElementById('UserId').value == addressId ) {
    alert($(".viewAddresses").attr("data-cannotdelete"));
    return;
  } else if ( document.getElementById('DefaultAddressId').value == addressId ) {
    alert($(".viewAddresses").attr("data-cantdelete"));
    return;
  } else if ( confirm($(".viewAddresses").attr("data-deleteAddress")) ) {
    document.getElementById('SelectedAddressId').value = addressId;
    document.getElementById('ManageAddressesFormAction').value = 'Delete';
    loading(true);
    document.getElementById('UserManagementManageAddressesForm').submit();
  }
}

function addAddress() {
  document.getElementById('ManageAddressesFormAction').value = 'Add';
  document.getElementById('UserManagementManageAddressesForm').submit();
}

function updateAddress(addressId) {
  document.getElementById('SelectedAddressId').value = addressId;
  document.getElementById('ManageAddressesFormAction').value = 'Update';
  document.getElementById('UserManagementManageAddressesForm').submit();
}