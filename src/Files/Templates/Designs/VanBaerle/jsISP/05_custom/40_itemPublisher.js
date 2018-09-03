/**
 * Description: Item Publisher
 * User: ShareIT
 * Date: 12/Jan/2016
 **/

function onLoadFaqs() {
  var $obj_faqsList = $("#faqsList");

  $obj_faqsList.find("li").find(">div").removeClass("hidden").hide();

  $obj_faqsList.find("h2").click(function () {
    var $obj_this = $(this);

    if ( $obj_this.find(".fa-sort-desc").length ) {
      $obj_this.find("i").removeClass("fa-sort-desc").addClass("fa-sort-up");
    }
    else {
      $obj_this.find("i").removeClass("fa-sort-up").addClass("fa-sort-desc");
    }

    $obj_this.closest("li").find(">div").slideToggle();
  });
}

function onLoadMedia() {
  var num_el = 3,
      fcn_resize = function () {
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
      };

  ShareIT.obj_helpers.fancybox($(".fancybox"));

  if ( $(".gallery").length ) {
    $("#leftMenuContainer").prepend("<div class='submenuTitle h2'>" + $(".gallery").attr("data-title") + "</div>");
  }

  $("img").load(function () {
    fcn_resize();
    normalizeListItemHeight(".gallery > li > figure", num_el, function () {
      normalizeListItemHeight(".gallery > li > div", num_el, function () {
        normalizeListItemHeight(".gallery > li", num_el)
      });
    });
  });

  $(window).resize(function () {
    fcn_resize();
  });
}

function onLoadPageSlideshow() {
    if ($("#slideShowContainer").length) {
        $('#slideShowContainer > ul').bxSlider({
            slideMargin:0,
            autoReload:true,
            controls:false
        });
        ShareIT.obj_helpers.fancybox($(".fancybox"));
    }
}