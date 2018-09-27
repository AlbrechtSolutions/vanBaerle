/**
 * Description: Form Validations
 * User: ShareIT
 * Date: 12/Jan/2016
 **/

var obj_dataManagementForms = {
  str_findText:"",
  arr_formIds:[]
};

function onLoadValidatePageForms($obj_form, bol_dialogOnEmpty) {
  var $obj_selects = $obj_form.find('.Select select'),
      $obj_dates = $obj_form.find('.DateTime select, .Date select'),
      $obj_inputFiles = $obj_form.find('.File input[type="file"]'),
      $obj_Calendar = $('.DateTime, .Date').find('img'),
      num_index;

  for ( num_index = 0 ; num_index < $obj_inputFiles.length ; num_index++ ) {
    if ( !$($obj_inputFiles[num_index]).hasClass("doneAlready") ) {
      $($obj_inputFiles[num_index]).addClass("doneAlready");
      $($obj_inputFiles[num_index]).parent().append("<div class='fakefile'><input class='fakeInput' /><button type='button' class='btn btn-bg'>" + obj_dataManagementForms.str_findText + "</button></div>");
      $($obj_inputFiles[num_index]).change(function () {
        $(this).siblings(".fakefile").children('input').val($(this).val());
      });
      $($obj_inputFiles[num_index]).parent().find(".fakefile").click(function () {
        $(this).parent().find("input[type=file]").click();
      });

    }
  }
  // Select user country
  $obj_form.find("select.country").val(obj_user.str_country).change();


  // If form have country and state

  $(".DMForms .country select").change(function () {
    var str_selectedCode = $(this).val(),
        str_number = $(this).attr("class").split("_")[1],
        $obj_regionSelect = $("select.state");

    $obj_regionSelect.html("");
    $obj_regionSelect.append("<option value='default' disabled='disabled' selected='selected'>"+$("fieldset.state label").html()+"</option>");
    fcn_changeTexts($(this), "State");
    if ( typeof countryRegions[str_selectedCode] != "undefined"
             && typeof countryRegions[str_selectedCode].code != "undefined"
        && countryRegions[str_selectedCode].code.length > 0 ) {
      for ( var i = 0 ; i < countryRegions[str_selectedCode].code.length ; i++ ) {
        $obj_regionSelect.append("<option value='" + countryRegions[str_selectedCode].code[i] + "'>" + countryRegions[str_selectedCode].name[i] + "</option>");
      }
    }
    else {
      $obj_regionSelect.append("<option value='default' disabled='disabled'> ---- </option>");
    }
    $obj_regionSelect.selectpicker("refresh");
  });

  // If this is the Form for Carrer page
  if($("#jobDatePosted").length){
    $("#DatePosted").val($("#jobDatePosted").text());
    $("#JobTitle").val($("#jobTitle").text());
    $("#Location").val($("#jobLocation").text());
  }

  $obj_selects.selectpicker({width:"100%", size:8});
  $obj_dates.selectpicker();

  $obj_Calendar.click(function () {
    var $obj_this = $(this),
        $obj_updatable = $obj_this.parents('fieldset.DateTime, fieldset.Date').children('select'),
        $obj_prevButton = $obj_this.prevUntil('select'),
        $obj_divCalendar = $('body > div.calendar'),
        $obj_headRowChildren,
        $obj_title;

    if ( $obj_divCalendar.length > 1 ) {
      $obj_divCalendar.first().remove();
    }

    $obj_headRowChildren = $obj_divCalendar.find('.headrow').children();
    $obj_divCalendar.find('.headrow .title').remove();
    $obj_title = $obj_divCalendar.find('.title');

    $obj_divCalendar.find('.daynames td.name.wn').remove();
    $obj_title.attr("colspan", "3").insertAfter($($obj_headRowChildren[2]));

    $obj_divCalendar.css({"z-index":"9001", top:$obj_prevButton.offset().top, left:$obj_this.offset().left + 42});

    $obj_divCalendar.click(function () {
      for ( num_index = 0 ; num_index < 3 ; num_index++ ) {
        $($obj_updatable[num_index]).selectpicker("refresh");
      }
    });
  });

  $obj_form.submit(function () {
    return generalValidations($obj_form, bol_dialogOnEmpty);
  });

  $obj_form.find('fieldset').first().focus();
}

function checkIfValidEmailWithObj($obj_element) {
  var bol_return = true;

  var exp_regExp = /^$|^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.(?:[a-zA-Z]{2,5})$/;
  if ( !exp_regExp.test($obj_element.val()) ) {
    bol_return = false;
  }

  return bol_return;
}

function checkIfValidPhoneWithObj($obj_element) {
  var exp_regExp = /^$|^(\(?\+?[0-9]*\)?)?[0-9_\- \(\)]+[0-9\)]$/,
      bol_return = true;

  if ( !exp_regExp.test($obj_element.val()) ) {
    bol_submitForm = false;
    bol_return = false;
  }

  return bol_return;
}

function checkIfFutureWithObj($obj_element) {
  var bol_return = false,
      dat_limit = new Date(),
      dat_user;

  if ( isNotEmptyWithObj($obj_element) ) {
    if ( $obj_element.val().length <= 15 ) {
      $obj_element.val($obj_element.val().substring(0, 10));
    }

    dat_user = Date.parse($obj_element.val()).clearTime();

    dat_limit.setDate(dat_limit.getDate() - 1);

    if ( dat_limit.compareTo(dat_user) == -1 ) {
      $obj_element.parents("fieldset.DateTime, fieldset.Date").removeClass("invalidDate");
      bol_return = true;
    }
    else {
      $obj_element.parents("fieldset.DateTime, fieldset.Date").addClass("invalidDate");
      bol_submitForm = false;
    }
  }
  return(bol_return);
}

function isNotEmptyWithObj($obj_element, bol_required) {

  if ( $obj_element.val() == '' || $obj_element.val() == $obj_element.attr("placeholder") ) {
    if ( bol_required ) bol_submitForm = false;
    return false;
  }
  else {
    return true;
  }
}

function isNumberKey(evt) {
  var charCode = (evt.which) ? evt.which : event.keyCode
  if ( charCode > 31 && charCode != 43 && (charCode < 48 || charCode > 57) )
    return false;
  return true;
}

function generalValidations($obj_form, bol_dialogOnEmpty) {
  var bol_validForm = true,
      str_fieldErrorClass = "fieldError",
      $obj_allFieldErrors = $obj_form.find('.' + str_fieldErrorClass),
      $obj_formFieldsets = $obj_form.find('fieldset'),
      $obj_fieldsetConfirmation = $obj_form.find('fieldset.confirmation'),
      $obj_thisFieldset,
      str_firstErrorID = "",
      str_formErrorMessages = "",
      str_heapBoxText = "";

  $obj_allFieldErrors.removeClass(str_fieldErrorClass);

  for ( var i = 0 ; i < $obj_formFieldsets.length ; i++ ) {
    $obj_thisFieldset = $($obj_formFieldsets[i]);

    if ( $obj_thisFieldset.hasClass("mandatory") ) {
      if ( ($obj_thisFieldset.find('.bootstrap-select').length && ($obj_thisFieldset.find('select').val() == "" || $obj_thisFieldset.find('select').val() == "default2"))
               || ($obj_thisFieldset.hasClass("File") && $obj_thisFieldset.find('.fakefile > input').val() == "")
               || ($obj_thisFieldset.find('input[type=text],input[type=email], input[type=password], textarea').not(".form-control").length && !isNotEmptyWithObj($obj_thisFieldset.find('input[type=text], input[type=email], input[type=password], textarea').not(".form-control")))
               || ($obj_thisFieldset.find("input[type=checkbox]").length && !$obj_thisFieldset.find("input[type=checkbox]").is(":checked"))
               || ($obj_thisFieldset.find("input[type=radio]").length && !$obj_thisFieldset.find("input[type=radio]").is(":checked"))
               || ($obj_thisFieldset.find("select").length && $obj_thisFieldset.find("select").val() == "") ) {
          
        $obj_thisFieldset.addClass(str_fieldErrorClass);
 
        if ( bol_dialogOnEmpty ) {
          str_firstErrorID += $obj_thisFieldset.attr("name") + ",";

          if ( $obj_thisFieldset.find('label:first').length && !$obj_thisFieldset.find('.bootstrap-select').length ) {
            str_formErrorMessages += "<p>" + $obj_thisFieldset.find('label:first').text() + " - <span>" + obj_formErrorMessages.str_emptyField + "</span></p>";
          } else if ( $obj_thisFieldset.find('input').attr("placeholder") ) {
            str_formErrorMessages += "<p>" + $obj_thisFieldset.find('input').attr("placeholder") + " - <span>" + obj_formErrorMessages.str_emptyField + "</span></p>";
          } else if ( $obj_thisFieldset.find('.bootstrap-select') ) {
            str_heapBoxText = $obj_thisFieldset.find('label:first').length ? $obj_thisFieldset.find('label').text() : $obj_thisFieldset.find('.bootstrap-select > .btn').text();
            str_formErrorMessages += "<p>" + str_heapBoxText + " - <span>" + obj_formErrorMessages.str_selectDefault + "</span></p>";
          }
        }
        bol_validForm = false;
      }
    }
    if ( $obj_thisFieldset.hasClass("checkEmail") && !checkIfValidEmailWithObj($obj_thisFieldset.find('input')) ) {
      $obj_thisFieldset.addClass(str_fieldErrorClass);
      str_firstErrorID += $obj_thisFieldset.attr("name") + ",";

      if ( $obj_thisFieldset.find('label:first').length ) {
        str_formErrorMessages += "<p>" + $obj_thisFieldset.find('label:first').text() + " - <span>" + obj_formErrorMessages.str_invalidEmail + "</span></p>";
      } else if ( $obj_thisFieldset.find('input').attr("placeholder") ) {
        str_formErrorMessages += "<p>" + $obj_thisFieldset.find('input').attr("placeholder") + " - <span>" + obj_formErrorMessages.str_invalidEmail + "</span></p>";
      } else if ( $obj_thisFieldset.find('.bootstrap-select') ) {
        str_formErrorMessages += "<p>" + $obj_thisFieldset.find('.bootstrap-select > .btn').text() + " - <span>" + obj_formErrorMessages.str_invalidEmail + "</span></p>";
      }

      bol_validForm = false;
    }
    if ( $obj_thisFieldset.hasClass("checkPhone") && !checkIfValidPhoneWithObj($obj_thisFieldset.find('input')) ) {
      $obj_thisFieldset.addClass(str_fieldErrorClass);
      str_firstErrorID += $obj_thisFieldset.attr("name") + ",";

      if ( $obj_thisFieldset.find('label:first').length ) {
        str_formErrorMessages += "<p>" + $obj_thisFieldset.find('label:first').text() + " - <span>" + obj_formErrorMessages.str_invalidPhoneNumber + "</span></p>";
      } else if ( $obj_thisFieldset.find('input').attr("placeholder") ) {
        str_formErrorMessages += "<p>" + $obj_thisFieldset.find('input').attr("placeholder") + " - <span>" + obj_formErrorMessages.str_invalidPhoneNumber + "</span></p>";
      } else if ( $obj_thisFieldset.find('.bootstrap-select') ) {
        str_formErrorMessages += "<p>" + $obj_thisFieldset.find('.bootstrap-select > .btn').text() + " - <span>" + obj_formErrorMessages.str_invalidPhoneNumber + "</span></p>";
      }

      bol_validForm = false;
    }
    if ( $obj_thisFieldset.hasClass("checkDate") && !checkIfFutureWithObj($obj_thisFieldset.find("input[name*='DatePicker']")) ) {
      $obj_thisFieldset.addClass(str_fieldErrorClass);
      str_firstErrorID += $obj_thisFieldset.attr("name") + ",";

      if ( $obj_thisFieldset.find('label:first').length ) {
        str_formErrorMessages += "<p>" + $obj_thisFieldset.find('label:first').text() + " - <span>" + obj_formErrorMessages.str_invalidDate + "</span></p>";
      } else if ( $obj_thisFieldset.find('input').attr("placeholder") ) {
        str_formErrorMessages += "<p>" + $obj_thisFieldset.find('input').attr("placeholder") + " - <span>" + obj_formErrorMessages.str_invalidDate + "</span></p>";
      } else if ( $obj_thisFieldset.find('.bootstrap-select') ) {
        str_formErrorMessages += "<p>" + $obj_thisFieldset.find('.bootstrap-select > .btn').text() + " - <span>" + obj_formErrorMessages.str_invalidDate + "</span></p>";
      }

      bol_validForm = false;
    }
  }
  if ( $obj_fieldsetConfirmation.length > 1 ) {
    if ( $($obj_fieldsetConfirmation[0]).find('input').val() != $($obj_fieldsetConfirmation[1]).find('input').val() ) {

      $obj_fieldsetConfirmation.addClass(str_fieldErrorClass);

      if ( bol_dialogOnEmpty ) {
        str_firstErrorID += $($obj_fieldsetConfirmation[0]).attr("name") + ",";
        str_firstErrorID += $($obj_fieldsetConfirmation[1]).attr("name") + ",";

        if ( $obj_fieldsetConfirmation.find('label:first').length ) {
          str_formErrorMessages += "<p>" + $($obj_fieldsetConfirmation[0]).find('label:first').text() + " - <span>" + obj_formErrorMessages.str_mustBeEqual + "</span></p>";
          str_formErrorMessages += "<p>" + $($obj_fieldsetConfirmation[1]).find('label:first').text() + " - <span>" + obj_formErrorMessages.str_mustBeEqual + "</span></p>";
        } else if ( $obj_thisFieldset.find('input').attr("placeholder") ) {
          str_formErrorMessages += "<p>" + $($obj_fieldsetConfirmation[0]).find('input').attr("placeholder") + " - <span>" + obj_formErrorMessages.mustBeEqual + "</span></p>";
          str_formErrorMessages += "<p>" + $($obj_fieldsetConfirmation[1]).find('input').attr("placeholder") + " - <span>" + obj_formErrorMessages.mustBeEqual + "</span></p>";
        }
      }
      bol_validForm = false;
    }
  }
  str_firstErrorID = str_firstErrorID.substring(0, str_firstErrorID.indexOf(','));

  if ( $('[name="' + str_firstErrorID + '"]').length ) {
    $('body').animate({scrollTop:($('body').scrollTop() + $('[name="' + str_firstErrorID + '"]').offset().top)}, 'slow');
  }

  if ( str_formErrorMessages != "" ) {
    alert(str_formErrorMessages, obj_formErrorMessages.str_errorMessagesTitle);
  }

  return bol_validForm;
}
