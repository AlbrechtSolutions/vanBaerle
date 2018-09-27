/**
 * Description: Item Publisher module
 * User: ShareIT
 * Date: 12/Jan/2016
**/

/* GOOGLE MAPS - BEGIN */
function onLoadGoogleMaps(obj_mapSettings) {
  /* todo Filter markers - BEGIN */
  var $obj_gMapFilters = $('#typesList');

  obj_mapSettings.google_markers = filterMapTypes(obj_mapSettings.google_type, obj_mapSettings.google_filter);

  if(typeof obj_mapSettings != "undefined"){
    for (key in obj_mapSettings.google_type){
      $obj_gMapFilters.append("<li id='type_"+key+"'><input name='markersRadio' id='input_"+key+"' type='radio' /><label for='input_"+key+"'><span>"+ obj_mapSettings.google_type[key].type_name +"</span></label></li>");
    }
    $obj_gMapFilters.append("<li id='type_all'><input name='markersRadio' id='input_all' type='radio' checked='checked' /><label for='input_all'><span>"+ obj_mapSettings.google_labels.label_viewAll +"</span></label></li>");
  }

  $obj_gMapFilters.find("input").change(function(){
    obj_mapSettings.google_filter = $(this).attr("id").split("input_")[1];
    obj_mapSettings.google_markers = filterMapTypes(obj_mapSettings.google_type, obj_mapSettings.google_filter);
    onLoadInitializeGmap(obj_mapSettings)
  });
  /* todo Filter markers - END */

  obj_mapSettings.google_markers = filterMapTypes(obj_mapSettings.google_type, obj_mapSettings.google_filter);
  $.getScript(obj_globalOptions.obj_urlPaths.str_theme + "/js/libraries/exceptions/10_googleMaps.aNet.js", function (data, textStatus, jqxhr) {
    onLoadInitializeGmap(obj_mapSettings);
  });
}

function filterMapTypes(obj_types, str_filter) {
  var obj_response = [];

  if ( str_filter != null && str_filter != undefined && str_filter != '' && str_filter != 'all' ) {
    obj_response = obj_types[str_filter].type_markers;
  }
  else {
    for ( var str_type in obj_types ) {
      obj_response = obj_response.concat(obj_types[str_type].type_markers);
    }
  }
  return obj_response;
}
/* GOOGLE MAPS - END */

