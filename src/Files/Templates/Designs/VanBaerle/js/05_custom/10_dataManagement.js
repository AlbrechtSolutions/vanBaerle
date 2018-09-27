/**
 * Description: Forms
 * User: ShareIT
 * Date: 12/Jan/2016
**/


var obj_errMsgTypes = {
      str_compulsoryFieldEmpty:"str_compulsoryFieldEmpty",
      str_notValidEmail:"str_notValidEmail",
      str_unknownErrors:"str_unknownErrors"
    },
    obj_formData = [],
    str_formId;

function onLoadForm(obj_formOptions) {
  styleForm(obj_formOptions.str_formId, obj_formOptions.str_findText);

  switch (obj_formOptions.str_formId) {
    case 'none':{} break;
    default:{
      loadForms(obj_formOptions);
    }
  }
}

function onLoadFormSuccess(){
  setTimeout(function(){
    var $obj_formSuccess = $('.paragraphModule').children('div.formSuccess');

    for (var i = 0; i < $obj_formSuccess.length; i++)
    {
      if($obj_formSuccess.parents('article.paragraph').siblings('div.googleMapsContainer').length >= 1){
        $obj_formSuccess.parents('article.paragraph').siblings('div.googleMapsContainer').remove();
      }
      $($obj_formSuccess[i]).parents('div.paragraphText').hide();
    }
  },200);
}

function calendarLang(str_lang) {
  if ( str_lang == 'pt-PT' ) {
    // full day names
    Calendar._DN = new Array("Domingo", "Segunda", "Terça", "Quarta", "Quinta", "Sexta", "Sábado", "Domingo");
    // short day names
    Calendar._SDN = new Array("Dom", "Seg", "Ter", "Qua", "Qui", "Sex", "Sáb", "Dom");
    // First day of the week. "0" means display Sunday first, "1" means display Monday first, etc.
    Calendar._FD = 1;
    // full month names
    Calendar._MN = new Array("Janeiro", "Fevereiro", "Março", "Abril", "Maio", "Junho", "Julho", "Agosto", "Setembro", "Outubro", "Novembro", "Dezembro");
    //short month names
    Calendar._SMN = new Array("Jan", "Fev", "Mar ", "Abr", "Mai", "Jun", "Jul", "Ago", "Set", "Out", "Nov", "Dez");
    // tooltips
    Calendar._TT = {};
    Calendar._TT["INFO"] = "Sobre o calendário";
    Calendar._TT["ABOUT"] = "DHTML Date/Time Selector\n" + "(coffee) dynarch.com 2002-2005 / Author: Mihai Bazon\n" + "For latest version visit: http://www.dynarch.com/projects/calendar/\n" + "Distributed under GNU LGPL. See http://gnu.org/licenses/lgpl.html for details." + "\n\n" + "Date selection:\n" + "- Use the \xab, \xbb buttons to select year\n" + "- Use the " + String.fromCharCode(0x2039) + ", " + String.fromCharCode(0x203a) + " buttons to select month\n" + "- Hold mouse button on any of the above buttons for faster selection.";
    Calendar._TT["ABOUT_TIME"] = "\n\n" + "Time selection:\n" + "- Click on any of the time parts to increase it\n" + "- or Shift-click to decrease it\n" + "- or click and drag for faster selection.";
    Calendar._TT["PREV_YEAR"] = "Ano anterior";
    Calendar._TT["PREV_MONTH"] = "Mês anterior";
    Calendar._TT["GO_TODAY"] = "Seleccionar hoje";
    Calendar._TT["NEXT_MONTH"] = "Próximo mês";
    Calendar._TT["NEXT_YEAR"] = "Próximo ano";
    Calendar._TT["SEL_DATE"] = "Seleccione a data";
    Calendar._TT["DRAG_TO_MOVE"] = "Arraste para mover";
    Calendar._TT["PART_TODAY"] = " (hoje)";
    // the following is to inform that "%s" is to be the first day of week
    // %s will be replaced with the day name.
    Calendar._TT["DAY_FIRST"] = "Ordenar por %s";
    // This may be locale-dependent. It specifies the week-end days, as an array
    // of comma-separated numbers. The numbers are from 0 to 6: 0 means Sunday, 1
    // means Monday, etc.
    Calendar._TT["WEEKEND"] = "0,6";
    Calendar._TT["CLOSE"] = "Fechar";
    Calendar._TT["TODAY"] = "Hoje";
    Calendar._TT["TIME_PART"] = "(Shift-) Clique ou arraste para alterar o valor";
    // date formats
    Calendar._TT["DEF_DATE_FORMAT"] = "%Y-%m-%d";
    Calendar._TT["TT_DATE_FORMAT"] = "%a, %b %e";
    Calendar._TT["WK"] = "Sm";
    Calendar._TT["TIME"] = "Tempo:";
  }
  else if ( str_lang == 'it-IT' ) {
    // full day names
    Calendar._DN = new Array("Domenica", "Lunedi", "Martedì", "Mercoledì", "Giovedi", "Venerdì", "Sabato", "Domenica");
    // short day names
    Calendar._SDN = new Array("Dom", "Lun", "Mar", "Mer", "Gio", "Ven", "Sab", "Dom");
    // First day of the week. "0" means display Sunday first, "1" means display Monday first, etc.
    Calendar._FD = 1;
    // full month names
    Calendar._MN = new Array("Gennaio", "Febbraio", "Marzo", "Aprile", "Maggio", "Giugno", "Luglio", "Agosto", "Settembre", "Ottobre", "Novembre", "Dicembre");
    //short month names
    Calendar._SMN = new Array("Gen", "Feb", "Mar ", "Abr", "Mag", "Giu", "Lug", "Ago", "Set", "Ott", "Nov", "Dic");
    // tooltips
    Calendar._TT = {};
    Calendar._TT["INFO"] = "Informazioni sul calendario";
    Calendar._TT["ABOUT"] = "DHTML Date/Time Selector\n" + "(coffee) dynarch.com 2002-2005 / Author: Mihai Bazon\n" + "For latest version visit: http://www.dynarch.com/projects/calendar/\n" + "Distributed under GNU LGPL. See http://gnu.org/licenses/lgpl.html for details." + "\n\n" + "Date selection:\n" + "- Use the \xab, \xbb buttons to select year\n" + "- Use the " + String.fromCharCode(0x2039) + ", " + String.fromCharCode(0x203a) + " buttons to select month\n" + "- Hold mouse button on any of the above buttons for faster selection.";
    Calendar._TT["ABOUT_TIME"] = "\n\n" + "Time selection:\n" + "- Click on any of the time parts to increase it\n" + "- or Shift-click to decrease it\n" + "- or click and drag for faster selection.";
    Calendar._TT["PREV_YEAR"] = "Anno precedente";
    Calendar._TT["PREV_MONTH"] = "Mese precedente";
    Calendar._TT["GO_TODAY"] = "Selezionare oggi";
    Calendar._TT["NEXT_MONTH"] = "Il mese prossimo";
    Calendar._TT["NEXT_YEAR"] = "Il prossimo anno";
    Calendar._TT["SEL_DATE"] = "Selezionare data";
    Calendar._TT["DRAG_TO_MOVE"] = "Trascinare per spostare";
    Calendar._TT["PART_TODAY"] = " (oggi)";
    // the following is to inform that "%s" is to be the first day of week
    // %s will be replaced with the day name.
    Calendar._TT["DAY_FIRST"] = "visualizzare %s prima";
    // This may be locale-dependent. It specifies the week-end days, as an array
    // of comma-separated numbers. The numbers are from 0 to 6: 0 means Sunday, 1
    // means Monday, etc.
    Calendar._TT["WEEKEND"] = "0,6";
    Calendar._TT["CLOSE"] = "Chiudere";
    Calendar._TT["TODAY"] = "Oggi";
    Calendar._TT["TIME_PART"] = "(Shift-) Fare clic o trascinare per modificare il valore";
    // date formats
    Calendar._TT["DEF_DATE_FORMAT"] = "%Y-%m-%d";
    Calendar._TT["TT_DATE_FORMAT"] = "%a, %b %e";
    Calendar._TT["WK"] = "Se";
    Calendar._TT["TIME"] = "volta:";
  }
  else if ( str_lang == 'en-GB' ) {
    // full day names
    Calendar._DN = new Array("Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday");
    // short day names
    Calendar._SDN = new Array("Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun");
    // First day of the week. "0" means display Sunday first, "1" means display
    // Monday first, etc.
    Calendar._FD = 1;
    // full month names
    Calendar._MN = new Array("January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December");
    // short month names
    Calendar._SMN = new Array("Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Set", "Oct", "Nov", "Dec");
    // tooltips
    Calendar._TT = {};
    Calendar._TT["INFO"] = "About the calendar";
    Calendar._TT["ABOUT"] = "DHTML Date/Time Selector\n" + "(coffee) dynarch.com 2002-2005 / Author: Mihai Bazon\n" + "For latest version visit: http://www.dynarch.com/projects/calendar/\n" + "Distributed under GNU LGPL. See http://gnu.org/licenses/lgpl.html for details." + "\n\n" + "Date selection:\n" + "- Use the \xab, \xbb buttons to select year\n" + "- Use the " + String.fromCharCode(0x2039) + ", " + String.fromCharCode(0x203a) + " buttons to select month\n" + "- Hold mouse button on any of the above buttons for faster selection.";
    Calendar._TT["ABOUT_TIME"] = "\n\n" + "Time selection:\n" + "- Click on any of the time parts to increase it\n" + "- or Shift-click to decrease it\n" + "- or click and drag for faster selection.";
    Calendar._TT["PREV_YEAR"] = "Prev. year (hold for menu)";
    Calendar._TT["PREV_MONTH"] = "Prev. month (hold for menu)";
    Calendar._TT["GO_TODAY"] = "Go Today";
    Calendar._TT["NEXT_MONTH"] = "Next month (hold for menu)";
    Calendar._TT["NEXT_YEAR"] = "Next year (hold for menu)";
    Calendar._TT["SEL_DATE"] = "Select date";
    Calendar._TT["DRAG_TO_MOVE"] = "Drag to move";
    Calendar._TT["PART_TODAY"] = " (today)";
    // the following is to inform that "%s" is to be the first day of week
    // %s will be replaced with the day name.
    Calendar._TT["DAY_FIRST"] = "Display %s first";
    // This may be locale-dependent.  It specifies the week-end days, as an array
    // of comma-separated numbers.  The numbers are from 0 to 6: 0 means Sunday, 1
    // means Monday, etc.
    Calendar._TT["WEEKEND"] = "0,6";
    Calendar._TT["CLOSE"] = "Close";
    Calendar._TT["TODAY"] = "Today";
    Calendar._TT["TIME_PART"] = "(Shift-)Click or drag to change value";
    // date formats
    Calendar._TT["DEF_DATE_FORMAT"] = "%Y-%m-%d";
    Calendar._TT["TT_DATE_FORMAT"] = "%a, %b %e";
    Calendar._TT["WK"] = "wk";
    Calendar._TT["TIME"] = "Time:";
  }
  else if ( str_lang == 'fr-FR' ) {
    // full day names
    Calendar._DN = new Array("Dimanche", "Lundi", "Mardi", "Mercredi", "Jeudi", "Vendredi", "Samedi", "Dimanche");
    // short day names
    Calendar._SDN = new Array("Dim", "Lun", "Mar", "Mer", "Jeu", "Ven", "Sam", "Dim");
    // First day of the week. "0" means display Sunday first, "1" means display
    // Monday first, etc.
    Calendar._FD = 1;
    // full month names
    Calendar._MN = new Array("Janvier", "Février", "Mars", "Avril", "Mai", "Juin", "Juillet", "Août", "Septembre", "Octobre", "Novembre", "Décembre");
    // short month names
    Calendar._SMN = new Array("Jan", "Fév", "Mar", "Avr", "Mai", "Jui", "Juil", "Aoû", "Sep", "Oct", "Nov", "Dec");
    // tooltips
    Calendar._TT = {};
    Calendar._TT["INFO"] = "Sur le calendrier";
    Calendar._TT["ABOUT"] = "DHTML Sélecteur de date / heure\n" + "(coffee) dynarch.com 2002-2005 / Auteur: Mihai Bazon\n" + "Pour la version la plus récente visite: http://www.dynarch.com/projects/calendar/\n" + "Distribué sous licence GNU LGPL. Voir http://gnu.org/licenses/lgpl.html for details." + "\n\n" + "Date de sélection:\n" + "- Utilisez les \xab, \xbb boutons pour sélectionner l'année\n" + "- Utilisez les " + String.fromCharCode(0x2039) + ", " + String.fromCharCode(0x203a) + " boutons pour sélectionner le mois\n" + "- Maintenez le bouton de la souris sur l'un des boutons ci-dessus pour une sélection plus rapide.";
    Calendar._TT["ABOUT_TIME"] = "\n\n" + "Sélection de l'heure:\n" + "- Cliquez sur l'une des parties de temps à augmenter\n" + "- ou Shift-click pour le réduire\n" + "- ou cliquez et faites glisser pour une sélection plus rapide.";
    Calendar._TT["PREV_YEAR"] = "Précédent année (en attente pour le menu)";
    Calendar._TT["PREV_MONTH"] = "Précédent. mois (en attente pour le menu)";
    Calendar._TT["GO_TODAY"] = "Allez Aujourd'hui";
    Calendar._TT["NEXT_MONTH"] = "Le mois prochain (en attente pour le menu)";
    Calendar._TT["NEXT_YEAR"] = "L'année prochaine (en attente pour le menu)";
    Calendar._TT["SEL_DATE"] = "Sélectionnez la date";
    Calendar._TT["DRAG_TO_MOVE"] = "Faites glisser pour déplacer";
    Calendar._TT["PART_TODAY"] = " (aujourd'hui)";
    // the following is to inform that "%s" is to be the first day of week
    // %s will be replaced with the day name.
    Calendar._TT["DAY_FIRST"] = "Affichage %s premier";
    // This may be locale-dependent.  It specifies the week-end days, as an array
    // of comma-separated numbers.  The numbers are from 0 to 6: 0 means Sunday, 1
    // means Monday, etc.
    Calendar._TT["WEEKEND"] = "0,6";
    Calendar._TT["CLOSE"] = "Fermer";
    Calendar._TT["TODAY"] = "Aujourd'hui";
    Calendar._TT["TIME_PART"] = "(Shift-)Click ou faites glisser pour changer la valeur";
    // date formats
    Calendar._TT["DEF_DATE_FORMAT"] = "%Y-%m-%d";
    Calendar._TT["TT_DATE_FORMAT"] = "%a, %b %e";
    Calendar._TT["WK"] = "sm";
    Calendar._TT["TIME"] = "Heure:";
  }
}

function loadForms(obj_formOptions) {
  var $obj_form = $('#' + obj_formOptions.str_formId);

  if ( $('.Date, .DateTime').length >= 1 ) {
    calendarLang($("html").attr("lang"));
  }

  /*if($('.calendarHolder').length >= 1)
  {
    calendarLang($("html").attr("lang"));
  }*/

  $obj_form.find('select').heapbox({
    'effect':{
      'type':'slide',
      'speed':'fast'
    }
  });
}

function loadCheckRadioBoxes(){

  var str_inputType;

  $('.checkBoxContainer, .radioBoxContainer').each(function(i){

    if($(this).hasClass('checkBoxContainer')){
      str_inputType = 'checkBox';
    }else{
      str_inputType = 'radioBox';
    }

    if($(this).find('input').is(':checked')){
     $(this).find('label').removeClass('unChecked');
     $(this).find('input').attr('checked',true);
    }else{
     $(this).find('label').addClass('unChecked');
     $(this).find('input').attr('checked',false);
    }
    $(this).find('label').click(function(){
      if($(this).parents('div').hasClass('checkBoxContainer')){
        str_inputType = 'checkBox';
      }else{
        str_inputType = 'radioBox';
      }

      if(str_inputType == 'radioBox'){
       $('.radioBoxContainer').each(function(i){
         $(this).find('label').addClass('unChecked');
         $(this).find('input').attr('checked',false);
       });
       $(this).removeClass('unChecked');
       $(this).find('input').attr('checked',true);
      }else{
        if($(this).find('input').is(':checked')){
         $(this).removeClass('unChecked');
         $(this).find('input').attr('checked',true);
        }else{
         $(this).addClass('unChecked');
         $(this).find('input').attr('checked',false);
        }
      }
    });
    if(str_inputType == 'checkBox'){
     //$(this).find('input').attr('checked', true);
    }
  });
}

function processErrorMessages(str_formId) {
  var str_errorMessage = "",
      str_fieldLabel = "",
      str_errorFound;

  switch ( str_formId ) {
    case 'none':{}break;
    default:{
      $("#" + str_formId).find('.formErrors').empty().remove();
      $('#' + str_formId).find('input, textarea, fieldset').removeClass('fieldError');
      for ( var num_i = 0 ; num_i < obj_formData[str_formId].arr_fields.length ; num_i++ ) {
        str_errorFound = obj_formData[str_formId].arr_fields[num_i].str_errorFound;
        if ( str_errorFound != '' ) {
          str_fieldLabel = validateFieldLabel(str_formId, num_i);
          switch ( str_errorFound ) {
            case obj_errMsgTypes.str_notValidEmail:
              obj_formData[str_formId].arr_fields[num_i].str_error = " ("+obj_formData[str_formId].obj_errMsg[str_errorFound]+")";
              //$("#" + obj_formData[str_formId].arr_fields[num_i].str_name).addClass('fieldError').val($("#" + obj_formData[str_formId].arr_fields[num_i].str_name).val()+" ("+obj_formData[str_formId].obj_errMsg[str_errorFound]+")");
              $("#" + obj_formData[str_formId].arr_fields[num_i].str_name).parents('fieldset').addClass('fieldError');
              str_errorMessage += obj_formData[str_formId].arr_fields[num_i].str_label + " - '" + $("#" + obj_formData[str_formId].arr_fields[num_i].str_name).val() + "' " + obj_formData[str_formId].obj_errMsg[str_errorFound] + "</br>";
              break;
            case obj_errMsgTypes.str_compulsoryFieldEmpty:
              obj_formData[str_formId].arr_fields[num_i].str_error = " ("+obj_formData[str_formId].obj_errMsg[str_errorFound]+")";
              //$("#" + obj_formData[str_formId].arr_fields[num_i].str_name).addClass('fieldError').val($("#" + obj_formData[str_formId].arr_fields[num_i].str_name).val()+" ("+obj_formData[str_formId].obj_errMsg[str_errorFound]+")");
              $("#" + obj_formData[str_formId].arr_fields[num_i].str_name).parents('fieldset').addClass('fieldError');
              str_errorMessage = obj_formData[str_formId].obj_errMsg[obj_errMsgTypes.str_mandatoryFieldFull];
            break;
            case obj_errMsgTypes.str_unknownErrors:
              obj_formData[str_formId].arr_fields[num_i].str_error = " ("+obj_formData[str_formId].obj_errMsg[str_errorFound]+")";
              //$("#" + obj_formData[str_formId].arr_fields[num_i].str_name).addClass('fieldError').val($("#" + obj_formData[str_formId].arr_fields[num_i].str_name).val()+" ("+obj_formData[str_formId].obj_errMsg[str_errorFound]+")");
              $("#" + obj_formData[str_formId].arr_fields[num_i].str_name).parents('fieldset').addClass('fieldError');
              str_errorMessage += obj_formData[str_formId].arr_fields[num_i].str_label + " - " +processErrorString(str_fieldLabel, obj_formData[str_formId].obj_errMsg[str_errorFound]) + "</br>";
              break;
          }
        }
      }
      if ( str_errorMessage != "" ){
        if($("#" + str_formId).find('.formErrors').length){
          $("html, body").animate({ scrollTop:$("#" + str_formId + ' .formErrors').offset().top - 140}, obj_globalNums.num_animationSpeed);
          //$("#" + str_formId).find('.formErrors').html(str_errorMessage).hide();
        }else{
          $("#" + str_formId).prepend('<div class="formErrors">'+str_errorMessage+'</div>');
          $("html, body").animate({ scrollTop:$("#" + str_formId + ' .formErrors').offset().top - 140}, obj_globalNums.num_animationSpeed);
          //$("#" + str_formId).find('.formErrors').hide();
        }
      }
    }
  }
}

function processValidations(str_formId, num_i) {
  try {
    if ( !eval(obj_formData[str_formId].arr_fields[num_i].str_validation + '("' + obj_formData[str_formId].arr_fields[num_i].str_name + '");') ) {
      switch ( obj_formData[str_formId].arr_fields[num_i].str_validation ) {
        case "checkIfValidEmail":
          obj_formData[str_formId].arr_fields[num_i].str_errorFound = obj_errMsgTypes.str_notValidEmail;
          break;
        case "checkIfFuture":
          obj_formData[str_formId].arr_fields[num_i].str_errorFound = obj_errMsgTypes.str_unknownErrors;
          break;
        case "checkIfValidPhone":
          obj_formData[str_formId].arr_fields[num_i].str_errorFound = obj_errMsgTypes.str_unknownErrors;
          break;
        default:
          obj_formData[str_formId].arr_fields[num_i].str_errorFound = obj_errMsgTypes.str_unknownErrors;
          break;
      }
    }
  }
  catch( obj_error ) {
    alert("validation function '" + obj_formData[str_formId].arr_fields[num_i].str_validation + "' missing!\n '" + obj_formData[str_formId].arr_fields[num_i].str_name + "' WILL NOT be checked\n" + obj_error);
  }
}

function validateFieldLabel(str_formId, num_i) {
  var $obj_field = $("#" + obj_formData[str_formId].arr_fields[num_i].str_name),
      $obj_fieldLabel = $("#" + str_formId + " label[for=" + obj_formData[str_formId].arr_fields[num_i].str_name + "]:eq(0)"),
      str_fieldTitle = $obj_field.attr("title"),
      str_fieldPlaceholder = $obj_field.attr("placeholder");

  if ( $obj_fieldLabel.length ) str_fieldLabel = $obj_fieldLabel.text();
  else if ( str_fieldTitle != "" && typeof str_fieldTitle != 'undefined' ) str_fieldLabel = str_fieldTitle;
  else if ( str_fieldPlaceholder != "" && typeof str_fieldPlaceholder != 'undefined' ) str_fieldLabel = str_fieldPlaceholder;
  else str_fieldLabel = obj_formData[str_formId].arr_fields[num_i].str_name;

  str_fieldLabel = "'" + str_fieldLabel + "'";
  return str_fieldLabel;
}

function validateForm(str_formId) {
  $('.errorAlert').hide();

  var bol_fieldStatus = "";

  bol_submitForm = true;

  if ( typeof obj_formData[str_formId].arr_fields != "undefined" ) {
    for ( var num_i = 0 ; num_i < obj_formData[str_formId].arr_fields.length ; num_i++ ) {
      obj_formData[str_formId].arr_fields[num_i].str_errorFound = '';
      bol_fieldStatus = (obj_formData[str_formId].arr_fields[num_i].bol_required == "True") ? true : false;

      if ( bol_fieldStatus &&
           validateIfEmpty(str_formId, num_i, bol_fieldStatus) &&
           obj_formData[str_formId].arr_fields[num_i].str_validation != '' ) {
        processValidations(str_formId, num_i);
      }
      /* If validation exists and value not empty, validate*/
      else if ( !bol_fieldStatus && validateIfEmpty(str_formId, num_i, bol_fieldStatus) &&
                obj_formData[str_formId].arr_fields[num_i].str_validation != '' ) {
        processValidations(str_formId, num_i);
      }
    }
  }

  if ( !bol_submitForm ) processErrorMessages(str_formId);

  $("form#" + str_formId).find('fieldset.fieldError').children('.errorAlert').show();

  return bol_submitForm;

  //$("#" + str_formId).submit();
}

function validateIfEmpty(str_formId, num_i, bol_generateErrorMessage) {
  if ( (obj_formData[str_formId].arr_fields[num_i].str_type == 'TextInput' || obj_formData[str_formId].arr_fields[num_i].str_type == 'Textarea' || obj_formData[str_formId].arr_fields[num_i].str_type == 'Select' || obj_formData[str_formId].arr_fields[num_i].str_type == 'File' ) &&
       !isNotEmpty(obj_formData[str_formId].arr_fields[num_i].str_name, bol_generateErrorMessage) ) {
    if ( bol_generateErrorMessage ) obj_formData[str_formId].arr_fields[num_i].str_errorFound = obj_errMsgTypes.str_compulsoryFieldEmpty;
    return false;
  }
  else if ( (obj_formData[str_formId].arr_fields[num_i].str_type == 'Radio' || obj_formData[str_formId].arr_fields[num_i].str_type == 'CheckBox') &&
            !validateCheckboxOrRadio(obj_formData[str_formId].arr_fields[num_i].str_name) ) {
    if ( bol_generateErrorMessage ) obj_formData[str_formId].arr_fields[num_i].str_errorFound = obj_errMsgTypes.str_compulsoryFieldEmpty;
    return false;
  }
  else {
    return true;
  }
}

function validateDefaultValue($obj_input) {
  if ( $obj_input.value == $obj_input.defaultValue ) {
    $obj_input.value = '';
  } else if ( $obj_input.value == '' ) {
    $obj_input.value = $obj_input.defaultValue;
  }
}

function styleForm(str_formID, str_btnText) {
  var W3CDOM = (document.createElement && document.getElementsByTagName),
      $obj_DateTimeYear,
      $obj_DateYear,
      $obj_Calendar,
      $obj_dateTime,
      $obj_date,
      $obj_fieldset = $('.paragraphModule fieldset'),
      num_YearWidth = 100;

  setTimeout(function(){
    $obj_DateTimeYear = $($('.DateTime > .heapBox')[2]);
    $obj_DateYear = $($('.Date > .heapBox')[2]);
    $obj_Calendar = $('.DateTime, .Date').find('img');

    $obj_DateTimeYear.children('.holder').width(num_YearWidth);
    $obj_DateTimeYear.children('div.heap').width(num_YearWidth + 21);
    $obj_DateYear.children('.holder').width(num_YearWidth);
    $obj_DateYear.children('div.heap').width(num_YearWidth + 21);

    $obj_Calendar.click(function () {
      var $obj_updatable = $(this).parents('fieldset.DateTime, fieldset.Date').children('select'),
        $obj_this = $(this),
        $obj_divCalendar = $('body > div.calendar');

      $obj_divCalendar.css({"z-index": "9001", top: $obj_this.offset().top, left: $obj_this.offset().left+42})

      $obj_divCalendar.click(function () {
        for ( var i = 0 ; i < 3 ; i++ ) {
          $($obj_updatable[i]).heapbox("update");
        }
      });

      if ( $obj_divCalendar.length > 1 ) {
          $obj_divCalendar.first().remove();
        }
    });

    $obj_dateTime = $('.DateTime');
    $obj_date = $('.Date');

    $('.DateTime, .Date').find('img').attr('src', '/Files/Templates/Designs/VanBaerle/images/calendar.png');

    for(var i = 0; i < $obj_fieldset.length; i++){
      if(typeof $($obj_fieldset[i]).attr('data-width') != "undefined" && $($obj_fieldset[i]).attr('data-width') != 0){
         $($obj_fieldset[i]).addClass("sizedField").find(".heapBox .holder, .heapBox .heap, input").width($($obj_fieldset[i]).attr('data-width'));
         $($obj_fieldset[i]).find(".heapBox .heap").width($($obj_fieldset[i]).find(".heapBox .heap").width()+20);
      }
    }

    var $obj_sizedField = $('.sizedField');

    for(var i = 0; i < $obj_sizedField.length; i++){
      if(!$($obj_sizedField[i]).next().hasClass("sizedField")){
         $($obj_sizedField[i]).addClass("lastSized");
      }
    }

    var str_textInputs = $('.formFields .TextInput > input[type=text]')

    for(var i = 0; i < str_textInputs.length; i++){
      if($(str_textInputs[i]).attr('id').indexOf("phone") > 1){$(str_textInputs[i]).keypress(function(evt){return isNumberKey(evt);})};
    }

    $('form#'+str_formId).find('input[name*="ChosenTrip"]').hide();

  }, 200);

  if ( !W3CDOM ) return;

  var fakeFileUpload = document.createElement('div');

  fakeFileUpload.className = 'fakefile';

  fakeFileUpload.appendChild(document.createElement('input'));

  var button = document.createElement('button');

  button.className = 'grayButton';
  button.innerHTML = str_btnText;

  fakeFileUpload.appendChild(button);

  var x = document.getElementsByTagName('input');

  for ( var i = 0 ; i < x.length ; i++ ) {
    if ( x[i].type != 'file' ) continue;
    if ( x[i].parentNode.className != 'fileinputs' ) continue;
    if(!$(x[i]).hasClass("doneAlready")){
      x[i].className = 'file';
      x[i].className = x[i].className + " doneAlready";
      var clone = fakeFileUpload.cloneNode(true);
      x[i].parentNode.appendChild(clone);
      x[i].relatedElement = clone.getElementsByTagName('input')[0];
      x[i].onchange = x[i].onmouseout = function () {
        this.relatedElement.value = this.value;
      }
    }
  }

   /* --START--
   Button interaction (flip) */

  $('div.fileinputs').hover(function(){
    $(this).find("button, a").addClass("hovered")
  },
  function(){
    $(this).find("button, a").removeClass("hovered")
  });

  /* --END--
   Button interaction (flip) */
}
