/*!
 * Scripts for vanBaerle website
 */

// Move to top left of page on load
window.onbeforeunload = function () {
  window.scrollTo(0, 0);
};

(function () {
  document.addEventListener("DOMContentLoaded", function () {
    var $body = $("html, body"),
      elements = {
        menu: document.getElementById("js-menu"),
        menuOpen: document.getElementById("js-menu-open"),
        menuClose: document.getElementById("js-menu-close"),
        navScreen: document.getElementById("js-nav-screen"),
        navScreenOpen: document.getElementById("js-nav-screen-open"),
        navScreenClose: document.getElementById("js-nav-screen-close"),
        slides: document.getElementsByClassName("landing__slides")[0]
      };

    // Menu actions
    elements.menuOpen.addEventListener("click", function () {
      elements.menu.classList.add("main-menu--show");
    });
    elements.menuClose.addEventListener("click", function () {
      elements.menu.classList.remove("main-menu--show");
    });
    // end Menu actions

    // Full screen menu actions
    elements.navScreenOpen.addEventListener("click", function () {
      elements.navScreen.classList.add("show");
    });
    elements.navScreenClose.addEventListener("click", function () {
      elements.navScreen.classList.remove("show");
    });
    // end Full screen menu actions

    // Landing page
    var currentSlide = 1,
      slideLocked = false,
      sliderInitiated = false,
      $slide = $(".landing__slide"),
      $progressBar = $(".landing-progress-bar");

    if (document.getElementsByClassName("landing").length > 0) {
      document.body.classList.add("body--landing");
      if (isLargeScreen()) {
        initSlider();
      }
      window.addEventListener("resize", resizeSlider);
    }

    function isLargeScreen() {
      return document.documentElement.clientWidth >= 992;
    }

    function initSlider() {
      sliderInitiated = true;
      $slide.removeClass("landing__slide--active landing__slide--exit");
      // Clone first slide to the bottom of slides
      elements.slides
        .appendChild(
          document.getElementsByClassName("landing__slide")[0]
            .cloneNode(true)
        );
      // Add event listener for mouse wheel events
      $(window).on("mousewheel DOMMouseScroll MozMousePixelScroll", mouseWheelEvents);
      // Add event listener for keyboard arrow events
      document.addEventListener("keyup", keyboardSlideEvents);
      // For Firefox
      document.addEventListener("keypress", keyboardSlideEvents);
    }

    function destroySlider() {
      currentSlide = 1;
      $progressBar.css("width", 0);
      $slide.removeClass("landing__slide--active landing__slide--exit");
      window.scrollTo(0, 0);
      $body.scrollLeft(0);
      elements.slides.removeChild(elements.slides.lastChild);
      $(window).off("mousewheel DOMMouseScroll MozMousePixelScroll", mouseWheelEvents);
      document.removeEventListener("keyup", keyboardSlideEvents);
      document.removeEventListener("keypress", keyboardSlideEvents);
      sliderInitiated = false;
    }

    function resizeSlider() {
      if (isLargeScreen()) {
        if (!sliderInitiated) {
          initSlider();
        } else if (document.getElementsByClassName("landing__slide--active").length > 0) {
          $body.scrollLeft(document.getElementsByClassName("landing__slide--active")[0].offsetLeft);
        }
      } else if (sliderInitiated) {
        destroySlider();
      }
    }

    function mouseWheelEvents(e) {
      e.preventDefault();
      if (!slideLocked) {
        if (Math.round(e.deltaY) < 0) {
          nextSlide();
        } else if (Math.round(e.deltaY) > 0) {
          prevSlide();
        }
      }
    }

    function keyboardSlideEvents(e) {
      if (!slideLocked) {
        switch (e.keyCode) {
          case 37:
          case 38:
          case 75: {
            e.preventDefault();
            prevSlide();
            break;
          }
          case 39:
          case 40:
          case 74: {
            e.preventDefault();
            nextSlide();
            break;
          }
        }
      }
    }

    function nextSlide() {
      slideLocked = true;
      ++currentSlide;
      if (currentSlide === 2) {
        $body.animate({
          scrollLeft: $(".landing__slide:nth-child(" + currentSlide + ")").position().left
        }, {
          duration: 500,
          easing: "easeInSine",
          start: function () {
            $(".landing__slide:nth-child(" + (currentSlide - 1) + ")")
              .addClass("landing__slide--exit");
            $progressBar.css("width", "50%");
          },
          complete: function () {
            $slide.removeClass("landing__slide--active");
            $(".landing__slide:nth-child(" + currentSlide + ")")
              .addClass("landing__slide--active");
            $(".landing__slide:nth-child(" + (currentSlide - 1) + ")")
              .removeClass("landing__slide--exit");
            animateSlideCompleted();
          }
        });
      } else if (currentSlide === 3) {
        $body.animate({
          scrollLeft: $(".landing__slide:nth-child(" + currentSlide + ")").position().left
        }, {
          duration: 250,
          easing: "easeInSine",
          start: function () {
            $progressBar.css("width", "100%");
          },
          complete: function () {
            $(".landing__slide:nth-child(" + currentSlide + ")")
              .addClass("landing__slide--active");
            $(".landing__slide:nth-child(" + (currentSlide - 1) + ")")
              .removeClass("landing__slide--active");
            animateSlideCompleted();
          }
        });
      } else if (currentSlide === 4) {
        ++currentSlide;
        $body.animate({
          scrollLeft: $(".landing__slide:nth-child(" + currentSlide + ")").position().left
        }, {
          duration: 625,
          start: function () {
            $(".landing__slide:nth-child(" + (currentSlide - 1) + ")")
              .addClass("landing__slide--active");
            $progressBar.addClass("landing-progress-bar--exit");
            setTimeout(function () {
              $progressBar.css("width", 0);
            }, 1000);
            setTimeout(function () {
              $progressBar.removeClass("landing-progress-bar--exit");
            }, 2000);
          },
          complete: function () {
            $body.scrollLeft(0);
            currentSlide = 1;
            $slide.removeClass("landing__slide--active");
            animateSlideCompleted();
          }
        });
      }
    }

    function prevSlide() {
      slideLocked = true;
      if (currentSlide === 2) {
        --currentSlide;
        $body.animate({
          scrollLeft: $(".landing__slide:nth-child(" + currentSlide + ")").position().left
        }, {
          duration: 500,
          start: function () {
            $progressBar.css("width", 0);
          },
          complete: function () {
            $slide.removeClass("landing__slide--active");
            animateSlideCompleted();
          }
        });
      } else if (currentSlide > 2) {
        --currentSlide;
        $body.animate({
          scrollLeft: $(".landing__slide:nth-child(" + currentSlide + ")").position().left
        }, {
          duration: 250,
          start: function () {
            $progressBar.css("width", "50%");
          },
          complete: function () {
            $(".landing__slide:nth-child(" + currentSlide + ")")
              .addClass("landing__slide--active");
            $(".landing__slide:nth-child(" + (currentSlide + 1) + ")")
              .removeClass("landing__slide--active");
            animateSlideCompleted();
          }
        });
      } else {
        slideLocked = false;
      }
    }

    function animateSlideCompleted() {
      $(".landing__slide:nth-child(" + currentSlide + ")")
        .addClass("landing__slide--active");
      setTimeout(function () {
        slideLocked = false;
      }, 1000);
    }
    // end Landing page

    //
    $(".collapse")
      .collapse({ toggle: false })
      .on("show.bs.collapse", function (e) {
        $(this).parent(".collapse__item").addClass("active");
      })
      .on("hidden.bs.collapse", function (e) {
        $(this).parent(".collapse__item").removeClass("active");
      });

    // About page
    // if ($(".page-about")[0]) {
    //   var $sectionSlides = [
    //     $(".section--1"),
    //     $(".section--3"),
    //     $(".section--4"),
    //     $(".section--6"),
    //     $(".section--7")
    //   ];

    //   for (var i = 0; i < $sectionSlides.length; ++i) {
    //     $sectionSlides[i].find(".section__slider").slick({
    //       arrows: true,
    //       prevArrow: "<button class='arrow arrow--prev'><i class='icon vb-chevron-left'></i></button>",
    //       nextArrow: "<button class='arrow arrow--next'><i class='icon vb-chevron-right'></i></button>",
    //       centerMode: true,
    //       dots: true,
    //       dotsClass: "dots",
    //       appendDots: $sectionSlides[i].find(".section__inner"),
    //       infinite: true,
    //       variableWidth: true
    //     }).addClass("init");
    //   }

    //   $(".event-display-list").slick({
    //     adaptiveHeight: true,
    //     arrows: false,
    //     asNavFor: ".timeline",
    //     fade: true,
    //     infinite: false,
    //     slidesToShow: 1
    //   }).addClass("init");

    //   $(".timeline").slick({
    //     arrows: false,
    //     asNavFor: ".event-display-list",
    //     dots: true,
    //     dotsClass: "dots",
    //     appendDots: $(".section--history .section__inner"),
    //     centerMode: true,
    //     focusOnSelect: true,
    //     infinite: false,
    //     variableWidth: true
    //   }).addClass("init");
    // }
    // end About page


    // $(".section__slider").slick({
    //   arrows: true,
    //   prevArrow: "<button class='arrow arrow--prev'><i class='icon vb-chevron-left'></i></button>",
    //   nextArrow: "<button class='arrow arrow--next'><i class='icon vb-chevron-right'></i></button>",
    //   centerMode: true,
    //   dots: false,
    //   dotsClass: "dots",
    //   infinite: true,
    //   variableWidth: true
    // }).addClass("init");


    // Careers page
    // var $sectionCareers = $(".page-careers .employees");

    // $sectionCareers.slick({
    //   arrows: true,
    //   prevArrow: "<button class='arrow arrow--prev'><i class='icon vb-chevron-left'></i></button>",
    //   nextArrow: "<button class='arrow arrow--next'><i class='icon vb-chevron-right'></i></button>",
    //   centerMode: true,
    //   dots: true,
    //   dotsClass: "dots",
    //   appendDots: $(".section--employees .section__inner"),
    //   infinite: true,
    //   variableWidth: true,
    //   centerMode: true,
    //   centerPadding: '40px'
    // }).addClass("init");

    // $("#js-video-play").on("click", function () {
    //   if (youTubeApi) {
    //     $("#js-video-overlay").addClass("fade-out");
    //     playVideo();
    //   }
    // });

    // $(".benefit__inner").tooltip({
    //   html: true
    // });
    // end Careers page

    // Media page
    // $(".page-media .image-list").slick({
    //   arrows: true,
    //   prevArrow: "<button class='arrow arrow--prev'><i class='icon vb-chevron-left'></i></button>",
    //   nextArrow: "<button class='arrow arrow--next'><i class='icon vb-chevron-right'></i></button>",
    //   centerMode: true,
    //   dots: true,
    //   dotsClass: "dots",
    //   appendDots: $(".section--image .section__inner"),
    //   infinite: true,
    //   variableWidth: true
    // }).addClass("init");
    // end Media page

    // Hygiene page
    // var $sectionApps = $(".section__apps"),
    //   $sectionRefs = $(".section__refs");

    // $sectionApps.slick({
    //   arrows: true,
    //   prevArrow: "<button class='arrow arrow--prev'><i class='icon vb-chevron-left'></i></button>",
    //   nextArrow: "<button class='arrow arrow--next'><i class='icon vb-chevron-right'></i></button>",
    //   dots: true,
    //   dotsClass: "dots",
    //   appendDots: $(".section--apps .section__inner"),
    //   centerMode: true,
    //   infinite: true,
    //   slidesToShow: $sectionApps[0] ? $sectionApps[0].children.length - 1 : 1,
    //   variableWidth: true
    // }).addClass("init");

    // $sectionRefs.slick({
    //   arrows: false,
    //   dots: true,
    //   dotsClass: "dots",
    //   appendDots: $(".section--refs .section__inner"),
    //   centerMode: true,
    //   infinite: true,
    //   slidesToShow: $sectionRefs[0] ? $sectionRefs[0].children.length - 1 : 1,
    //   variableWidth: true
    // }).addClass("init");
    // end Hygiene page

    // Silicates page
    // $sectionApps.slick({
    //   arrows: true,
    //   prevArrow: "<button class='arrow arrow--prev'><i class='icon vb-chevron-left'></i></button>",
    //   nextArrow: "<button class='arrow arrow--next'><i class='icon vb-chevron-right'></i></button>",
    //   dots: true,
    //   dotsClass: "dots",
    //   appendDots: $(".section--apps .section__inner"),
    //   centerMode: true,
    //   infinite: true,
    //   slidesToShow: $sectionApps[0] ? $sectionApps[0].children.length - 1 : 1,
    //   variableWidth: true
    // }).addClass("init");
    // end Silicates page

    // Product Details page
    $(".product-detail__qtt-adj--minus").on("click", function () {
      var $val = $($(this).siblings(".product-detail__qtt-value")[0]);
      if ($val.val() > 0) {
        $val.val(+$val.val() - 1);
      }
    });
    $(".product-detail__qtt-adj--plus").on("click", function () {
      var $val = $($(this).siblings(".product-detail__qtt-value")[0]);
      $val.val(+$val.val() + 1);
    });
    // end Product Details page





    // newSlides
    var nr = 0;
    $('.section__slider.general_slide').each(function(){
        var nrColumn = $(this).attr("data-nr");
      	var boolCol = true;
        var oneColumn = 1;
        // if(nrColumn > 1){
        //  	boolCol = true; 
        // }
        if(nrColumn > 1){
           oneColumn = 2; 
        }
            $(this).slick({
              arrows: true,
              prevArrow: "<button class='arrow arrow--prev'><i class='icon vb-chevron-left'></i></button>",
              nextArrow: "<button class='arrow arrow--next'><i class='icon vb-chevron-right'></i></button>",
              centerMode: true,
              infinite: boolCol,
              slidesToShow: nrColumn,
              responsive: [
                  {
                    breakpoint: 1200,
                    settings: {
                      arrows: true,
                      centerMode: true,
                      centerPadding: '40px',
                      slidesToShow:  oneColumn
                    }
                  },
                  {
                    breakpoint: 1000,
                    settings: {
                      arrows: false,
                      centerMode: true,
                      centerPadding: '40px',
                      slidesToShow:  2
                    }
                  },
                  {
                    breakpoint: 480,
                    settings: {
                      arrows: false,
                      centerMode: true,
                      centerPadding: '40px',
                      slidesToShow: 1
                    }
                  }
                ]
            }).addClass("init");
        

        nr++;
    });
   
    $(".section__slider.event-display-list").slick({
      adaptiveHeight: true,
      arrows: false,
      asNavFor: ".timeline",
      fade: true,
      infinite: false,
      slidesToShow: 1
    }).addClass("init");

    $(".section__slider.timeline").slick({
      arrows: false,
      asNavFor: ".event-display-list",
      dots: true,
      dotsClass: "dots",
      appendDots: $(".section--history .section__inner"),
      centerMode: true,
      focusOnSelect: true,
      infinite: false,
      variableWidth: true
    }).addClass("init");



    // same-height-1, same-height-2
    function equalizeHeight(arg) {
      var sameHeightArray = [];
      $('.'+arg).each(function(){
        var height = $(this).height();
        sameHeightArray.push(height);
      });
      
      var largest= 0;
      for (i=0; i<=largest;i++){
          if (sameHeightArray[i]>largest) {
              var largest=sameHeightArray[i];
          }
      }
      $('.'+arg).height(largest);
      //console.log(sameHeightArray,largest);
    }

    if ($(".same-height-1-de").length > 0) {
      equalizeHeight("same-height-1-de");
    }
    if ($(".same-height-1-fr").length > 0) {
      equalizeHeight("same-height-1-fr");
    }
    if ($(".same-height-1-en").length > 0) {
      equalizeHeight("same-height-1-en");
    }


    if ($(".same-height-2-de").length > 0) {
      equalizeHeight("same-height-2-de");
    }
    if ($(".same-height-2-fr").length > 0) {
      equalizeHeight("same-height-2-fr");
    }
    if ($(".same-height-2-en").length > 0) {
      equalizeHeight("same-height-2-en");
    }


    if ($(".same-height-de").length > 0) {
      equalizeHeight("same-height-3-de");
    }
    if ($(".same-height-3-fr").length > 0) {
      equalizeHeight("same-height-3-fr");
    }
    if ($(".same-height-3-en").length > 0) {
      equalizeHeight("same-height-3-en");
    }


    // read more/less
    if ($(".card__description").length > 0) {
      // readMoreLess("card__description");
      $('.card__description').each(function(){
        var maxHeight = 225;
        var currentHeight = $(this).height();
        var dataMore = $(this).attr('data-more');
        var dataLess = $(this).attr('data-less');
        if (currentHeight > maxHeight) {
          $(this).after('<button type="button" class="read-more-card">' + dataMore + '</button>');
          $(this).height(maxHeight);
          $(this).parent().find(".read-more-card").attr("data-height",currentHeight);
        }
      });

      $('body').on("click", ".card__text .read-more-card", function(){
        var maxHeight = 225;
        var normalHeight = $(this).attr("data-height");
        var dataMore = $(this).parent().find(".card__description").attr('data-more');
        var dataLess = $(this).parent().find(".card__description").attr('data-less');
        

        
        if($(this).hasClass("active")) {
            $(this).parent().find(".card__description").height(maxHeight);
            $(this).removeClass("active");
            $(this).html(dataMore);
            $(this).parent(".card__text").removeClass("resetPadding");
        } else {
            $(this).parent().find(".card__description").height(normalHeight);
            $(this).addClass("active");
            $(this).html(dataLess);
            $(this).parent(".card__text").addClass("resetPadding");
        }
      });

    }


    // popover
    $('[data-toggle="popover"]').popover()

    // fixed menu
    $(window).bind('scroll', function() {
      // The value of where the "scoll" is.
      if($(window).scrollTop() > 150){
        $('header.main-menu').addClass('fixed');
      }else{
        $('header.main-menu').removeClass('fixed');
        // Adding padding so it doesn't jump up
        // when the menu gets fixed.
        //$('.container').css('padding-top', '0px');
      }
    })


    // Silikate map slider
    $("#image-map area").on("click" , function(event){
      var slideNr = $(this).attr("data-slide");

      console.log(slideNr);
      $(".oneColumn-slider").slick('slickGoTo', slideNr);
    });

    ImageMap('img[usemap]');
    
	//Newsletter
    $('#NewsletterSubscribe').on("submit", function(e){
        e.preventDefault();
        var messageSuccess = $(this).attr("data-success");
        var errorMessage = $(this).attr("data-error");
  		var apiPageID = $(this).attr("data-api");
        var sendData = {};
        sendData.UserManagement_Form_Email = $(this).find("#UserManagement_Form_Email").val();
        if(!isEmail($("#UserManagement_Form_Email").val())) {
            alert(errorMessage)
            //console.log("error");
            return false;
        }
        console.log(isEmail($("#UserManagement_Form_Email").val()))
        $.ajax({
          url: '/Default.aspx?ID='+apiPageID,
          type: 'POST',      
          data: $('#NewsletterSubscribe').serialize()
        })
        .done(function(response) {
          // console.log("success");
          alert(messageSuccess);
        })
        .fail(function() {
          console.log("error");
        });
        
      });
    
    	function isEmail(email) {
  			var regex = /^([a-zA-Z0-9_.+-])+\@(([a-zA-Z0-9-])+\.)+([a-zA-Z0-9]{2,4})+$/;
  		return regex.test(email);
}

  });
})();
