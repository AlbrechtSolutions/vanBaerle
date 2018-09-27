var google_geocoder,
    google_map,
    google_defaultIcon,
    google_initLatLng,
    google_directionDisplay,
    google_directionsService,
    google_stepDisplay,
    google_mapOptions,
    google_mapConfig,
    google_rendererOptions,
    google_mapMarkers = [],
    google_mapInfoWindows = [],
    google_markerArray = [],
    marker_pointer,
    google_bounds;

function onLoadInitializeGmap(obj_mapSettings) {
  var mapLoaded = false;

  google_mapMarkers = [];
  google_mapInfoWindows = [];
  //google_markerArray = [];

  google_mapConfig = obj_mapSettings;

  var myStyles = [
    {
      featureType:"poi",
      elementType:"labels",
      stylers:[
        { visibility:"off" }
      ]
    }
  ];

  google_initLatLng = new google.maps.LatLng(google_mapConfig.google_initLatLng.split(',')[0],google_mapConfig.google_initLatLng.split(',')[1]);
  google_mapOptions = {
    zoom:google_mapConfig.google_zoom,
    center:google_initLatLng,
    panControl:false,
    zoomControl:true,
    mapTypeControl:false,
    scaleControl:true,
    streetViewControl:false,
    overviewMapControl:true,
    rotateControl:true,
    styles: myStyles,
    mapTypeId:google.maps.MapTypeId.ROADMAP, //ROADMAP SATELLITE HYBRID TERRAIN
    scrollwheel:false
  };
  google_map = new google.maps.Map(document.getElementById(google_mapConfig.google_mapId), google_mapOptions);
  google_geocoder = new google.maps.Geocoder();

  google_defaultIcon = new google.maps.MarkerImage(
      google_mapConfig.template_path + 'images/marker.png',
      new google.maps.Size(50, 67), // The Size
      new google.maps.Point(0, 0), // The origin
      new google.maps.Point(24, 60) // The anchor
  );

  if(google_mapConfig.google_markers.length > 1){
    google_mapConfig.google_fitBounds = true;
  }else{
    google_mapConfig.google_fitBounds = false;
    google_mapOptions.google_zoom = 12;
  }

  google_directionsService = new google.maps.DirectionsService();
  google_rendererOptions = {
    map:google_map
  };

  directionsDisplay = new google.maps.DirectionsRenderer(google_rendererOptions)
  google_stepDisplay = new google.maps.InfoWindow();

  google_bounds = new google.maps.LatLngBounds();

  loadMarkers(google_defaultIcon); // carregamento inicial

  if(google_mapConfig.google_initLatLng.split(',')[0] != ''){
    if(google_mapConfig.google_fitBounds){
      google_map.fitBounds( google_bounds );
    }else{
      google_map.setCenter(google_initLatLng);
    }
  }else{
    google_map.fitBounds( google_bounds );
  }

  if(google_mapConfig.google_markers.length > 1){
    return;
  }else{
    var markerLatLong = google_mapConfig.google_markers[0].marker_latlng.split(',');
    google_map.setCenter(new google.maps.LatLng(markerLatLong[0],markerLatLong[1]));
  }
  /*
   selectListMarkers( '.smartChoices' );
   */

}

function attachInstructionText(marker, text) {
  google.maps.event.addListener(marker, 'click', function () {
    // Open an info window when the marker is clicked on,
    // containing the text of the step.
    google_stepDisplay.setContent(text);
    google_stepDisplay.open(google_map, marker);
  });
}

function calcRoute() {

  // First, remove any existing markers from the map.
  for ( i = 0 ; i < google_markerArray.length ; i++ ) {
    google_markerArray[i].setMap(null);
  }

  // Now, clear the array itself.
  google_markerArray = [];

  // Retrieve the start and end locations and create
  // a DirectionsRequest using WALKING directions.
  var start = document.getElementById("start").value;
  var end = document.getElementById("end").value;
  var request = {
    origin:start,
    destination:end,
    travelMode:google.maps.DirectionsTravelMode.DRIVING
  };

  // Route the directions and pass the response to a
  // function to create markers for each step.
  google_directionsService.route(request, function (response, status) {
    if ( status == google.maps.DirectionsStatus.OK ) {
      var warnings = document.getElementById("warnings_panel");
      warnings.innerHTML = "<b>" + response.routes[0].warnings + "</b>";
      directionsDisplay.setDirections(response);
      directionsDisplay.setPanel(document.getElementById("directionsPanel"));

      setTimeout(function () {
        setScrollDirections();
      }, 1000);

      showSteps(response);
    }
  });

}

function createPointCoords(marker_point, marker_title, marker_icon, marker_infoWindow, bol_defaultMarker) {

  if ( bol_defaultMarker ) {
    var google_marker = new google.maps.Marker({
      position:marker_point,
      map:google_map,
      icon:marker_icon,
      title:marker_title,
      animation:google.maps.Animation.DROP,
      zIndex:999999
    });
  }
  else {
    var google_marker = new google.maps.Marker({
      position:marker_point,
      map:google_map,
      icon:marker_icon,
      title:marker_title,
      animation:google.maps.Animation.DROP
    });
  }
  //google_map.setCenter(marker_point);
  google_mapMarkers.push(google_marker);

  //goToMarker(google_mapMarkers);

  var form_directions = '';//'<form class="directions_form"><span>'+dir_titleform+'</span><input type="hidden" id="end" value="'+marker_point.lat()+', '+marker_point.lng()+'" disabled="disabled"/><input type="text" id="start" value="'+dirvalue+'" onblur="set_dirvalue()" onfocus="set_dirvalue()"/><button type="button" class="enviar" onclick="calcRoute();">OK</button><button type="button" class="enviar" onclick="load_map_agencias();">Reset</button></form>';

  marker_infoWindow = marker_infoWindow + form_directions;

  var google_infoWindow = new google.maps.InfoWindow({
    content:marker_infoWindow,
    maxWidth:300
  });
  google_mapInfoWindows.push(google_infoWindow);

  google.maps.event.addListener(google_infoWindow, 'closeclick', function () {
    google_marker.setAnimation(null);
  });

  google.maps.event.addListener(google_marker, 'click', function () {
    closeAllInfoWindows();
    google_infoWindow.open(google_map, google_marker);
    google_map.setCenter(marker_point);
    google_map.panBy(0, -120);
    google_marker.setAnimation(google.maps.Animation.BOUNCE);
    openInfoWindow();
    /*setTimeout(function(){
      console.log($('.gm-style-iw').children('div').first());
    },300);*/
  });

  /*google.maps.event.trigger(google_marker, 'click');*/
}

function createPointAddress(marker_address, marker_title, marker_icon, marker_infoWindow) {
  google_geocoder = new google.maps.Geocoder();
  google_geocoder.geocode({'address':marker_address}, function (results, status) {
    if ( status == google.maps.GeocoderStatus.OK ) {
      google_map.setCenter(results[0].geometry.location);
      google_map.setZoom(14);

      // Version Circle

      var google_circle = new google.maps.Circle({
        map:google_map,
        radius:300, // 3 km
        position:results[0].geometry.location,
        title:marker_title,
        fillColor:'transparent',
        fillOpacity:0,
        strokeColor:'#ff0000',
        strokeOpacity:0.9,
        strokeWeight:4,
        zIndex:999
      });

      google_circle.bindTo('center', google_circle, 'position');

      var form_directions = '';//<form class="directions_form"><span>'+dir_titleform+'</span><input type="hidden" id="end" value="'+results[0].geometry.location.lat()+', '+results[0].geometry.location.lng()+'" disabled="disabled"/><input type="text" id="start" value="'+dirvalue+'" onblur="set_dirvalue()" onfocus="set_dirvalue()"/><button type="button" class="enviar" onclick="calcRoute();">OK</button><button type="button" class="enviar" onclick="load_map_agencias();">Reset</button></form>';

      marker_infoWindow = marker_infoWindow + form_directions;

      var google_infoWindow = new google.maps.InfoWindow({
        content:marker_infoWindow,
        size:new google.maps.Size(100, 300)
      });

      google.maps.event.addListener(google_circle, 'click', function () {
        closeAllInfoWindows();
        google_infoWindow.open(google_map, google_circle);
      });

    }
    else {
      alert("Error - " + marker_address + ": " + status);
    }
  });
}

function clearDirections() {
  $('#directionsPanel').html('');
  setScrollDirections();
  onLoadInitializeGmap();
}

function closeAllInfoWindows(str_closeStatus) {
  $.each(google_mapConfig.google_markers, function (i) {
    google_mapInfoWindows[i].close(google_map, google_mapMarkers[i]);
    if ( str_closeStatus == 'reset' ) {
      google_mapMarkers[i].setAnimation(google.maps.Animation.DROP);
      google_map.fitBounds(google_bounds);
    }
    else {
      google_mapMarkers[i].setAnimation(null);
    }
  });
}

function loadMarkers(marker_defaultIcon) {
  arr_allMarkers = [];
  for ( var i = 0 ; i < google_mapConfig.google_markers.length ; i++ ) {
    //google_mapConfig.google_markers[i].marker_image;

    //    alert(google_mapConfig.google_markers[i].marker_image);
    if ( google_mapConfig.google_markers[i].marker_image != undefined ) {
      var marker_img = "<img src='" + google_mapConfig.google_markers[i].marker_image + "'/>";
    }
    else {
      var marker_img = '';
    }


    var marker_infoWindow = '<div class="infowindow"><span class="infowindowTitle">' + google_mapConfig.google_markers[i].marker_title + '</span>' + marker_img + '<div class="infowindowText">' + google_mapConfig.google_markers[i].marker_infoText + '</div></div>',
        marker_icon = new google.maps.MarkerImage(
            google_mapConfig.google_markers[i].marker_icon.image,
            new google.maps.Size(google_mapConfig.google_markers[i].marker_icon.width, google_mapConfig.google_markers[i].marker_icon.height), // The Size
            new google.maps.Point(google_mapConfig.google_markers[i].marker_icon.originX, google_mapConfig.google_markers[i].marker_icon.originY), // The origin
            new google.maps.Point(google_mapConfig.google_markers[i].marker_icon.anchorX, google_mapConfig.google_markers[i].marker_icon.anchorY) // The anchor
        );

    if ( google_mapConfig.google_markers[i].marker_latlng != '' ) {
      var googleLatLog = google_mapConfig.google_markers[i].marker_latlng.split(',');
      var marker_point = new google.maps.LatLng(googleLatLog[0], googleLatLog[1]);
      createPointCoords(marker_point, google_mapConfig.google_markers[i].marker_title, marker_icon, marker_infoWindow,google_mapConfig.google_markers[i].defaultMarker);

      marker_point = new google.maps.LatLng(googleLatLog[0], googleLatLog[1]);

      arr_allMarkers.push(marker_icon);

      google_bounds.extend( marker_point );

    }
    else {
      createPointAddress(google_mapConfig.google_markers[i].marker_geocoderAddress, google_mapConfig.google_markers[i].marker_title, marker_icon, marker_infoWindow);
    }

  }
}

function selectListMarkers(str_selectContainer) {

  var str_markersListId = 'markersList';

  if ( google_mapConfig.google_markers.length > 0 ) {

    $('#' + str_markersListId).select_unskin();

    $(str_selectContainer).html('<select id="' + str_markersListId + '" ><option value="">' + str_smartChoicesLabel + '</option></select>');
    $.each(google_mapConfig.google_markers, function (i) {
      $('#' + str_markersListId).append('<option value="' + i + '">' + google_mapConfig.google_markers[i].marker_title + '</option>')
    });
    $('#' + str_markersListId).change(function (event) {
      event.preventDefault();
      if ( $(this).val() != '' ) {
        closeAllInfoWindows();
        google.maps.event.trigger(google_mapMarkers[$(this).val()], 'click');
      }
      else {
        closeAllInfoWindows('reset');
      }
    });
    $('#' + str_markersListId).select_skin();
  }
}

function showSteps(directionResult) {
  // For each step, place a marker, and add the text to the marker's
  // info window. Also attach the marker to an array so we
  // can keep track of it and remove it when calculating new
  // routes.
  var myRoute = directionResult.routes[0].legs[0];

  for ( var i = 0 ; i < myRoute.steps.length ; i++ ) {
    var marker = new google.maps.Marker({
      position:myRoute.steps[i].start_point,
      map:google_map
    });
    attachInstructionText(marker, myRoute.steps[i].instructions);
    google_markerArray[i] = marker;

  }
}