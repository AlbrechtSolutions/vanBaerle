/**
 * Description:Alencastre Tools
 * User: ShareIT
 * Date: 12/Jan/2016
 **/

function onLoadPartners(str_json, obj_preSelected) {
  var json_regions = $.parseJSON(str_json),
      $obj_country = $("select[name=country]"),
      $obj_locality = $("select[name=locality]"),
      $obj_select = $("#findStoreFilter select"),
      $obj_storeList = $(".partnersList > li"),
      $obj_noMatches = $(".no-matches"),
      str_regions,
      arr_this,
      fcn_resize = function () {
        var num_el = 3;

        if ( obj_globalOptions.obj_strings.str_deviceDim == "xs" ) {
          num_el = 1;
        } else if ( obj_globalOptions.obj_strings.str_deviceDim == "sm" ) {
          num_el = 2;
        } else if ( obj_globalOptions.obj_strings.str_deviceDim == "md" ) {
          num_el = 3;
        }
        else {
          num_el = 3;
        }
        normalizeListItemHeight(".partnersList > li figure", num_el, function () {
          normalizeListItemHeight(".partnersList > li h2", num_el, function () {
            normalizeListItemHeight(".partnersList > li", num_el)
          });
        });
      },
      fcn_findPartners = function () {
        var str_regionSelected = $obj_locality.val(),
            str_countrySelected = $obj_country.val(),
            str_searchFor = str_regionSelected == "" ? "^" + str_countrySelected : str_countrySelected + " " + str_regionSelected,
            bol_results = false;

        $obj_storeList.addClass("hidden");
        $obj_noMatches.addClass("hidden");

        for ( var i = 0 ; i < $obj_storeList.length ; i++ ) {
          if ( $obj_storeList.eq(i).attr("data-filter-values").match(str_searchFor) ) {
            $obj_storeList.eq(i).removeClass("hidden");
            bol_results = true;
          }
        }

        if ( !bol_results ) $obj_noMatches.removeClass("hidden");

        fcn_resize();
        $obj_select.selectpicker("refresh");
      };

  $obj_country.change(function () {
    str_regions = "<option value=''>" + $("select[name=locality] option:first").html() + "</option>";

    arr_this = json_regions[$(this).val()].sort();

    for ( var i = 0 ; i < arr_this.length ; i++ ) {
      str_regions += "<option value='" + arr_this[i] + "'>" + arr_this[i] + "</option>";
    }

    $obj_locality.html(str_regions);
    fcn_findPartners();
  });

  $obj_locality.change(function () {
    fcn_findPartners();
  });

  setTimeout(function () {
    fcn_resize();
  }, 500);

  $(window).resize(function () {
    fcn_resize();
  });
  $obj_select.selectpicker({size:8});

}

function onLoadStoreLocator(str_json) {
  var json_regions = $.parseJSON(str_json),
      $obj_fieldset = $("#findStoreFilter fieldset.regions"),
      $obj_button = $("button[data-filter-value]"),
      $obj_locality = $("select[name=locality]"),
      $obj_select = $("#findStoreFilter select"),
      $obj_storeList = $(".storeList > li"),
      str_regions,
      arr_this;

  $("select[name=country]").change(function() {
    str_regions = "<option value=''>" + $("select[name=locality] option:first").html() + "</option>";

    if ($(this).val().trim() != "") {
      arr_this = json_regions[$(this).val()].sort();

      for (var i = 0; i < arr_this.length; i++) {
        str_regions += "<option value='" + arr_this[i] + "'>" + arr_this[i] + "</option>";
      }

      if (arr_this.length == 0) {
        $obj_fieldset.addClass('hidden');
      } else {
        $obj_fieldset.removeClass('hidden');
      }

      for (var a = 0; a < $obj_storeList.length; a++) {
        $obj_storeList.eq(a).attr("data-filter-values", $obj_storeList.eq(a).attr("data-country"));
      }
    }

    $obj_button.attr("data-filter-value", $(this).val());
    $obj_button.trigger("click");

    $(".storeList li").removeClass("hide");
    $obj_locality.html(str_regions);
    $obj_select.selectpicker("refresh");
  });

  $obj_locality.change(function () {
    if ( $(this).val() == "" ) {
      $obj_button.attr("data-filter-value", $.trim($("select[name=country]").val()));
      $("select[name=country]").trigger('change');

    }
    else {
      for ( var a = 0 ; a < $obj_storeList.length ; a++ ) {
        $obj_storeList.eq(a).attr("data-filter-values", $obj_storeList.eq(a).attr("data-country") + " " + $obj_storeList.eq(a).attr("data-city"));
      }
      $obj_button.attr("data-filter-value", $("select[name=country]").val() + " " + $(this).val());
    }

    $obj_button.trigger("click");

    $obj_storeList.removeClass("hide");
    $obj_select.selectpicker("refresh");

  });

  $obj_select.selectpicker({size:8});

}














function onLoadMaps() {
  var error_message = $("#findAStore").attr("data-error");
  var str_themePath = window.location.origin + obj_globalOptions.obj_urlPaths.str_theme + "/",
      createWhereToBuyMap = function (arr_stores) {
        var obj_mapSettings = {
          template_path:str_themePath,
          google_initLatLng:"",
          google_zoom:12,
          google_mapId:'gmap',
          google_fitBounds:false,
          google_markers:arr_stores,
          google_checkGeolocation:arr_stores.length == 0 ? $('#zipCode').val() : "",
          google_checkGeolocationCountryCode:"US"
        };

        $("#gmapContainer").removeClass("hidden");

        onLoadInitializeGmap(obj_mapSettings);

        $("#storesList > li").click(function () {
          google.maps.event.trigger(google_mapMarkers[$(this).index()], 'click');
        });
      },

      init = function () {
        var $obj_form = $("#Maps-85 form");
        $obj_form.find("select").selectpicker();
        $obj_form.unbind().submit(function () {
          var $obj_fieldset = $obj_form.find('fieldset'),
              str_error = "",
              bol_valid = true;

          for ( var i = 0, l = $obj_fieldset.length ; i < l ; i += 1 ) {
            var $obj_this = $($obj_fieldset[i]);

            $obj_this.removeClass('fieldError');

            if ( $obj_this.hasClass('mandatory') && $obj_this.find('input').val() == '' ) {
              $obj_this.addClass('fieldError');
              str_error += " " + $obj_this.find('input').attr('name') + " ";
              bol_valid = false;
            }
          }

          if ( str_error != "" ) {
            str_error += " - Is Required";
            alert(str_error);
          }


          if ( bol_valid ) {
            loading(true);
            $.ajax({
              type:"GET",
              url:$obj_form.attr("action"),
              data:$obj_form.serialize(),
              dataType:"xml",
              success:function (obj_data) {
                var $xmlResponse = JSON.parse(JSON.stringify(xmlToJson(obj_data))).response,
                    str_stores = "",
                    arr_stores = [];

                $('.callResult').html('');
                if ( typeof $xmlResponse.store == "undefined" ) {
                  var errorMessage = $('.callResult').attr("data-error");
                  $('.callResult').append('<p>' + errorMessage + '</p>');
                }
                else {
                  str_stores = "<ul id='storesList'>";
                  var len = $xmlResponse.store.length;
                  if(typeof len != 'undefined') {
                      for ( var i = 0 ; i < len ; i++ ) {
                          str_stores = renderStores($xmlResponse.store[i],str_stores,arr_stores,str_themePath);
                      }
                  }
                  else {
                      str_stores = renderStores($xmlResponse.store,str_stores,arr_stores,str_themePath);
                  }
                  str_stores += "</ul>";
                  $('.callResult').append(str_stores);
                }

                createWhereToBuyMap(arr_stores);

                loading(false);
              },
              error: function(error) {
                  loading(false);
                  alert(error_message);
              }
            });
          }
          return false;
        });
      };

  init();
}