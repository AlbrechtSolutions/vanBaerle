/**
 * Description: Product Catalog
 * User: ShareIT
 * Date: 12/Jan/2016
 **/

ShareIT.Ecommerce = {
    Product: function (options) {
        return {
            obj_product: options.obj_product,
            fcn_ifVariantCombExists: function (obj_options) {
                var obj_output = {
                    str_combination: "",
                    bol_exist: false,
                    obj_combination: {}
                },
                    obj_product = this.obj_product;

                for (var i = 0; i < Object.keys(obj_product.product).length; i++) {
                    var arr_variant = obj_product.product[Object.keys(obj_product.product)[i]].id.split("."),
                        bol_combination = true;

                    for (var s = 0; s < obj_options.arr_selectsValue.length; s++) {
                        if ($.inArray(obj_options.arr_selectsValue[s], arr_variant) == -1) {
                            bol_combination = false;
                        }
                    }
                    if (bol_combination) {
                        if (typeof obj_product.product[Object.keys(obj_product.product)[i]] != 'undefined') {
                            obj_output.str_combination = obj_product.product[Object.keys(obj_product.product)[i]].id;
                            obj_output.obj_combination = obj_product.product[Object.keys(obj_product.product)[i]];
                        }
                        obj_output.bol_exist = true;
                        break;
                    }
                }
                return obj_output;
            },
            fcn_getCurrenCombinationInfo: function (obj_options) {
                var arr_selectsValue = [],
                    $obj_selects = obj_options.$obj_container.find("select:not([name=quantitySelect])");

                for (var i = 0; i < $obj_selects.length; i++) {
                    arr_selectsValue.push($obj_selects.eq(i).val());
                }

                return this.fcn_ifVariantCombExists({ arr_selectsValue: arr_selectsValue });

            },
            fcn_poulateSelects: function (obj_options) {

                var arr_selectsValue = [],
                    $obj_selects = obj_options.$obj_container.find("select:not([name=quantitySelect])")
                        .not("#" + obj_options.$obj_currectSelect.attr('id')).not('.noFilterActive'),
                    $obj_loopSelect,
                    $obj_loopSelectOption;

                for (var i = 0; i < $obj_selects.length; i++) {
                    $obj_loopSelect = $obj_selects.eq(i);
                    for (var x = 0; x < $obj_loopSelect.find('option').length; x++) {
                        $obj_loopSelectOption = $obj_loopSelect.find('option').eq(x);
                        arr_selectsValue = [obj_options.$obj_currectSelect.val()];
                        arr_selectsValue.push($obj_loopSelectOption.attr('value'));
                        if (this.fcn_ifVariantCombExists({ arr_selectsValue: arr_selectsValue }).bol_exist) {
                            $obj_loopSelectOption.show().addClass("visible");
                        } else {
                            $obj_loopSelectOption.hide().removeClass("visible");
                        }
                    }
                    $obj_loopSelect.val($obj_loopSelect.find('option.visible').eq(0).val());
                }
                if (typeof obj_options.fnc_callback != "undefined") {
                    obj_options.fnc_callback();
                }
                this.fcn_updateStock();
                $('select').selectpicker('refresh');
            },
            fcn_updateStock: function () {
                var $obj_quantitySelect = $("select[name='quantitySelect']"),
                    $obj_inputQuantity = $("input[name=quantity]");

                if ($obj_quantitySelect.length) {
                    var $obj_selectOption = $obj_quantitySelect.find('option');
                    var quantityAvailable = $obj_inputQuantity.attr("data-stock");

                    if (quantityAvailable <= 0) {
                        $obj_quantitySelect.attr('disabled', true);
                    } else {
                        $obj_quantitySelect.attr('disabled', false);
                        $obj_selectOption.addClass("hidden");
                    }

                    for (var i = 0, j = $obj_inputQuantity.attr("data-stock") ; i < (j || 9) ; i++) {
                        $obj_selectOption.eq(i).removeClass("hidden");
                    }
                }
            }
        }
    },
    // Storing products object in localSorage
    fcn_compareProducts: function (str_action, str_product) {
        /*
         str_action
         - add // add key
         - remove // remove key
         - compare // verify if key exists
         - consult // Consult Localstorage
         */

        var obj_retrievedObject = JSON.parse(localStorage.getItem(str_projectFolder + '_productsToCompare')),
            fcn_save = function (obj_toSave) {
                // clear localstorage
                localStorage.removeItem(str_projectFolder + '_productsToCompare');
                // add to localstorage
                localStorage.setItem(str_projectFolder + '_productsToCompare', JSON.stringify(obj_toSave));
            },
            fcn_add = function () {
                // If *_productsToCompare Not Exists - Create
                if (obj_retrievedObject == undefined) {
                    obj_retrievedObject = {
                        productsToCompare: [str_product]
                    };

                    fcn_save(obj_retrievedObject);
                }
                    // If Exists - Push
                else {
                    if (obj_retrievedObject.productsToCompare.indexOf(str_product) < 0) {
                        obj_retrievedObject.productsToCompare.push(str_product);
                        fcn_save(obj_retrievedObject);
                    }
                }
            },
            fcn_compare = function () {
                return obj_retrievedObject.productsToCompare.indexOf(str_product) > -1;
            },
            fcn_remove = function () {
                if (fcn_compare()) {
                    obj_retrievedObject.productsToCompare.splice(
                        obj_retrievedObject.productsToCompare.indexOf(str_product),
                        1);
                    fcn_save(obj_retrievedObject);
                }
            },
            fcn_consult = function () {
                return JSON.parse(localStorage.getItem(str_projectFolder + '_productsToCompare'));
            };

        switch (str_action) {
            case "add":
                fcn_add();
                break;
            case "remove":
                fcn_remove();
                break;
            case "compare":
                return fcn_compare();
                break;
            case "consult":
                return fcn_consult();
                break;
        }

    },
    // Header Search - Autocomplete
    fcn_typeAhead: function () {
        var $obj_productId = $("#searchForm input[name=ProductID]"),
            $obj_inputText = $("#searchSubmit"),
            $obj_form = $("#searchForm"),
            str_pruductHref = "/Default.aspx?ID=" + $obj_form.find("input[name=ID]").val() + "&ProductID=",
            num_resultsCount,
            fcn_autocomplete = function () {
                $obj_inputText.unbind("keydown").bind("keydown",
                    function (event) {
                        if ((event.keyCode === $.ui.keyCode.ENTER || event.keyCode === $.ui.keyCode.TAB) && $(this).data("ui-autocomplete").menu.active) {
                            event.preventDefault();
                        }
                    }).autocomplete({
                        minLength: 2,
                        appendTo: "#autocompleteWrap",
                        source: function (request, response) {
                            $.ajax({
                                url: obj_pages.str_typeAhead,
                                data: { q: $("#searchSubmit").val() },
                                dataType: "json",
                                success: function (data) {
                                    num_resultsCount = data.Settings.TotalOfProducts;
                                    response(data.Products);
                                }
                            });
                        },
                        focus: function (event, ui) {
                            return false;
                        },
                        select: function (event, ui) {
                            this.value = ui.item.ProductName;
                            $obj_productId.val(ui.item.ProductId);
                            $obj_form.submit();
                            return false;
                        },
                        change: function (event, ui) {
                            if (ui.item == null || ui.item == undefined) {
                                //this.value = '';
                            }
                        },
                        close: function () {
                            $("#autocompleteWrap").removeClass("active");
                        },
                        open: function () {
                            var count = parseInt(num_resultsCount) > $('.ui-autocomplete > li').length ? num_resultsCount : $('.ui-autocomplete > li').length;
                            $("#autocompleteWrap").addClass("active");
                            $(".ui-autocomplete").append("<div class='moreProductsSearch'><a class='submitSearch' href='#'><span>" + obj_translates.str_seeAll + " '" + count + "' " + obj_translates.str_results + "</span></a></div>");
                            $(".submitSearch").unbind("click").click(function (event) {
                                event.preventDefault();

                                $obj_form.submit();
                            });
                        },
                        search: function () {
                            //                $(this).data("uiAutocomplete").menu.element.scrollTop(0);
                        }
                    }).data("ui-autocomplete")._renderItem = function (ul, item) {
                        str_pruductHref = str_pruductHref + item.ProductId;
                        return $("<li>").append("<figure class='text-center col-xs-3 noPadding'><img class='img-responsive' src='/Admin/Public/GetImage.ashx?Image=/Files/Files/" + str_projectFolder + "/Products/Images/" + item.ProductNumber + "_1.jpg&Width=95&Height=123&altFmImage_path=/Files/Templates/Designs/RizzoDesignFolder/images/alternativeImage.jpg&Crop=5' /></figure>"
                            + "<div class='col-xs-9 noPaddingRight'><h2>" + item.ProductName + "</h2><span>" + item.ProductNumber + "</span></div>").appendTo(ul);
                    };
            },
            fcn_formSubmit = function () {
                $obj_form.submit(function () {
                    loading(true);
                    if ($obj_productId.val() == "") {
                        $obj_productId.prop("disabled", true);
                    }
                    return true;
                });
            };

        if ($obj_form.length) {
            fcn_formSubmit();
            fcn_autocomplete();
        }
    }
};

function onLoadCompareDetail() {
    var fcn_resize = function () {
        if (obj_globalOptions.obj_strings.str_deviceDim == "xs") {
            num_el = 1;
        } else if (obj_globalOptions.obj_strings.str_deviceDim == "sm") {
            num_el = 2;
        } else if (obj_globalOptions.obj_strings.str_deviceDim == "md") {
            num_el = 3;
        } else {
            num_el = 4;
        }

        for (var i = 0; i < $("#compareDetailList > li > div").length; i++) {
            normalizeListItemHeight("#compareDetailList > li > .div_" + i, num_el);
        }
    },
        fcn_addToCompare = function () {
            var $obj_removeItem = $(".removeItem");

            $obj_removeItem.click(function () {
                var $obj_this = $(this),
                    $obj_interest = $("#ofInterest");

                ShareIT.Ecommerce.fcn_compareProducts("remove", $obj_this.attr("data-link"));

                $obj_this.closest("li").remove();
                history.pushState({},
                    '',
                    window.location.href.replace($obj_this.attr("data-link"), "").replace(",,", ","));
                if ($("#compareDetailList > li").length <= 3) {
                    $obj_interest.find(".btn").attr("href",
                        (window.location.href + "," + $obj_interest.find(".btn").attr("data-link")).replace(",,", ","));
                    $obj_interest.removeClass("hidden");
                }
                if ($("#compareDetailList > li").length <= 2) {
                    $obj_removeItem.remove();
                }
            });
        },
        fcn_compare = function () {
            var $obj_highlightDifferences = $("#highlightDifferences"),
                $obj_list = $("#compareDetailList"),
                num_elements = $obj_list.find("li:last>div").not(".noCompare"),
                $obj_li = $obj_list.find(">li").not(".noCompare");

            $obj_highlightDifferences.unbind("click").click(function () {
                var str_temp;

                if (!$(this).hasClass("active")) {
                    $(this).addClass("active");
                    $(this).html($(this).attr("data-remove"));
                    for (var div = 0; div < num_elements.length; div++) {
                        str_temp = null;

                        for (var li = 0; li < $obj_li.length; li++) {
                            var str_divText = $obj_list.find("li").not(".noCompare").eq(li).find(">div")
                                .not(".noCompare")
                                .eq(div).text();

                            if (str_temp == null) {
                                str_temp = str_divText;
                            } else {
                                if (str_temp != str_divText) {
                                    $("." + num_elements.eq(div).attr("class")).addClass("bgColor");
                                    break;
                                }
                            }
                        }
                    }
                } else {
                    $(this).html($(this).attr("data-add"));
                    $("#compareDetailList .bgColor").removeClass("bgColor");
                    $(this).removeClass("active");
                }

            });

        },
        fcn_addToCart = function ($obj_this) {
            var obj_ajax;

            loading(true);
            if (typeof obj_ajax != "undefined") {
                obj_ajax.abort();
            }

            obj_ajax = $.ajax({
                url: $obj_this.closest("form").attr("action"),
                data: $obj_this.closest("form").serialize(),
                success: function (obj_data) { },
                complete: function () {
                    loading(false);
                    $obj_this.find("span").html($obj_this.find("span").attr("data-added"));
                    $obj_this.addClass("highlighted");

                    setTimeout(function () {
                        $obj_this.find("span").html($obj_this.find("span").attr("data-add"));
                        $obj_this.removeClass("highlighted");
                    },
                        3000);
                    minicart();
                }
            });
        },
        fcn_backButton = function () {
            /*if (localStorage.getItem(str_projectFolder + '_lastProdPage') != undefined) {
                $(".goBack").click(function(event) {
                    event.preventDefault();
                    window.location = localStorage.getItem(str_projectFolder + '_lastProdPage');
                });
            } else {
                $(".goBack").hide()
            }*/
        };

    fcn_addToCompare();
    fcn_compare();
    fcn_backButton();
    // add to cart
    $(".addToCart").unbind("click").click(function (event) {
        event.preventDefault();
        fcn_addToCart($(this));
    });

    setTimeout(function () {
        fcn_resize();
    },
        200);
    $(window).resize(function () {
        setTimeout(function () {
            fcn_resize();
        },
            200);
    });

}

function onLoadProductDetail(obj_options) {

    // START Side Filtering for product details page

    $("li.l2 > a").each(function(index, el){
        if($(this).next(".M2").length == 0) {
            $(this).hide();
        }
    });

    $(".l3 > a").each(function(index,el){
        var url = $(this).attr("href");
        var windowLocation = window.location.href;


        if(windowLocation.indexOf(url) != -1) {
            // $(this).css({
            //     "color" : "#23527c",
            //     "font-weight" : "bold"
            // });

            // show active ul
            $(this).parent().parent().css("display" , "block");
            $(this).parent().parent().parent().addClass("active");
        }

    });

    $("li.l2 > a").on("click" , function(event){
        event.preventDefault();



        if($(this).next(".M2").length > 0) {

            if(!$(this).parent("li").hasClass("active")) {
                $(this).parent("li").addClass("active");
            } else {
                $(this).parent("li").removeClass("active");
            }

            
            $(this).next(".M2").slideToggle("slow");

            var siblings = $(this).parent().siblings().children("ul");

            $.each(siblings , function(index , element){

              if($(element).not(":hidden")) {
                $(this).hide('slow');
              }

              if($(element).parent().hasClass("active")){
                $(element).parent().removeClass("active");
              }

            });
        } 

    });

    // END Side Filtering for product details page


    var // todo change the default group ID
        str_groupDefault = "VARGRP1",
        thumbsSlider,
        $obj_thumbs = $("#imgThumbs ul"),
        $obj_productContainer = $('#product-container'),
        product = new ShareIT.Ecommerce.Product({ obj_product: obj_options.obj_product }),
        fcn_addToCart = function ($el) {
            var obj_ajax,
                $obj_form = $el.closest('form'),
                $obj_stock = $("input[data-stock]"),
                $obj_giftCard = $(".giftCardAmount");
            console.log("asdf");
            if (typeof obj_ajax != "undefined") {
                obj_ajax.abort();
            }
            loading(true);
            obj_ajax = $.ajax({
                url: $obj_form.attr("action"),
                data: $obj_form.serialize(),
                success: function (data) {
                    obj_data = data;
                },
                complete: function () {
                    // minicart(obj_data);
                    $el.find("span").html($el.find("span").attr("data-added"));
                    $el.addClass("highlighted");

                    setTimeout(function () {
                        $el.find("span").html($el.find("span").attr("data-add"));
                        $el.removeClass("highlighted");
                    },
                        3000);
                    loading(false);
                }
            });

        },
        fnc_favoritesClick = function () {
            var $obj_favoriteBtn = $("#favorite");

            $obj_favoriteBtn.unbind("click").click(function (e) {
                e.preventDefault();
                loading(true);
                if ($obj_favoriteBtn.attr("data-user") == "True") {
                    fcn_favoriteActions({
                        str_action: 'changeVariant',
                        str_newVariant: $("#variantID").val()
                    });
                    requestAjax({
                        str_url: $obj_favoriteBtn.attr("href") + "&" + obj_globalOptions.obj_urlPaths.str_ajaxRetriever,
                        str_type: "get",
                        obj_data: null,
                        fnc_callback: function (obj_response) {
                            if ($obj_favoriteBtn.attr("data-favorite") == "True") {
                                $obj_favoriteBtn.html('<i class="fa fa-heart-o"></i> ' +
                                    $obj_favoriteBtn.attr("data-addText"));
                                $obj_favoriteBtn.attr("data-favorite", "False");
                                product.obj_product.product[$("#variantID").val()].productIsFavorite = "False";
                            } else {
                                $obj_favoriteBtn.html('<i class="fa fa-heart"></i> ' +
                                    $obj_favoriteBtn.attr("data-removeText"));
                                $obj_favoriteBtn.attr("data-favorite", "True");
                                product.obj_product.product[$("#variantID").val()].productIsFavorite = "True";
                            }
                            loading(false);
                        }
                    });
                }
            });
        },
        fcn_favoriteActions = function (obj_options) {
            var $obj_favorite = $('#favorite'),
                str_inFavorites,
                str_addFavorites,
                str_removeFavorites;

            if ($obj_favorite.length) {
                if (obj_options.str_action == "changeVariant") {

                    str_inFavorites = product.obj_product.product[obj_options.str_newVariant].productIsFavorite;
                    str_addFavorites = product.obj_product.product[obj_options.str_newVariant].addToList;
                    str_removeFavorites = product.obj_product.product[obj_options.str_newVariant].removeFromList;

                    $obj_favorite.attr("data-favorite", str_inFavorites);
                    $obj_favorite.attr("data-add", str_addFavorites);
                    $obj_favorite.attr("data-remove", str_removeFavorites);

                    if (str_inFavorites == "True") {
                        $obj_favorite.attr('href', str_removeFavorites).html('<i class="fa fa-heart"></i> ' +
                            $obj_favorite.attr("data-removeText"));
                    } else {
                        $obj_favorite.attr('href', str_addFavorites).html('<i class="fa fa-heart-o"></i> ' +
                            $obj_favorite.attr("data-addText"));
                    }
                }
            }
        },
        fcn_tabs = function ($el) {
            $el.find(">ul>li>a").click(function (event) {
                event.preventDefault();

                $el.find(">ul>li").removeClass("active");
                $(this).parent().addClass("active");
                $el.find(">div").hide().removeClass("active");
                $($(this).attr("href")).fadeIn();

            });

            // copy content into the LI - XS solution

            for (var i = 0; i < $el.find(">ul>li>a").length; i++) {
                $el.find(">ul>li").eq(i).append($($el.find(">ul>li>a").eq(i).attr("href")).clone());
            }
            $el.find(">ul>li > div").removeAttr("id");

        },
        fcn_changeQuantity = function () {
            var $obj_inputQuantity = $(".quantityInput"),
                $obj_quantitySelect = $("select[name=quantitySelect]"),
                fcn_onChange = function ($el) {
                    if ($el.val() == "10+") {
                        $el.closest("form").find(".quantityInput.hidden").removeClass("hidden");
                        $el.closest("form").find(".quantity-container .bootstrap-select").addClass("hidden").val("10");
                        $el.closest("form").find("input.quantityInput").val("10");
                    } else {
                        $el.closest("form").find("input.quantityInput").val($el.val());
                    }
                };

            $obj_quantitySelect.change(function () {
                fcn_onChange($(this));
            });

            /*$obj_inputQuantity.change(function() {
                fcn_validateStock($(this));
            });*/
            $obj_inputQuantity.blur(function () {
                if ($obj_inputQuantity.val() < 10) {
                    $obj_inputQuantity.addClass("hidden");
                    $obj_inputQuantity.closest("fieldset").find("select").val($obj_inputQuantity.val());
                    $obj_inputQuantity.closest("fieldset").find(".bootstrap-select").removeClass("hidden");
                    $("select").selectpicker("refresh");
                }
            });
        },
        fcn_pageChanges = function () {
            // Hide page title
            $("div.h1").addClass("hidden");
            $("#centralContent").addClass("noTopMargin");
            // breadcrumb menu

        },
        fcn_zoomImage = function () {
            var str_productLargeImg = $("#product-lg-image img");
            str_productLargeImg.wrap('<span style="display:inline-block"></span>')
                .css('display', 'block')
                .parent()
                .zoom({ url: $("#imgThumbs li.active img").attr("data-large") });
            str_productLargeImg.attr('src', $("#imgThumbs li.active img").attr('data-image'));
        },
        fcn_reviewsRating = function () {
            // On click a rating star in the review form
            $("#commentRating > li").click(function () {
                $("#commentRating > li").removeClass("star");
                $(this).addClass("star").find("input").prop("checked", true);
            });
            // On submit a review
            $("#commentform").submit(function () {
                return generalValidations($(this), true);
            });
            // If review was submited
            if (window.location.hash == "#reviewsContainer") $("#reviewWithSuccess").removeClass("hidden");
        };

    //run
  
    function animationAddedToCart(el){
        var added = $(el).find("span").attr("data-added");
        var add = $(el).find("span").attr("data-add");
        $(el).addClass("highlighted");
        $(el).find("span").html(added);
        setTimeout(function(){
            $(el).removeClass("highlighted");
            $(el).find("span").html(add);
        },1400);
    }

    $('.product-detail__add-cart').on("click", function(){
        var link = $(this).attr("data-href");
        var quantity = $(this).parents(".variant-item").find(".product-detail__qtt-value").val();
        var $this = $(this);
        console.log(link+quantity);
        $.ajax({
            url: link+quantity,
            type: 'GET'
        })
        .done(function(response) {
            console.log("success");
            animationAddedToCart($this);
            minicart();
        })
        .fail(function() {
            console.log("error");
        });
        
    });    

    $("#product-description").append("<ul class='hidden' id='thumbsBackup'>" + $obj_thumbs.html() + "</ul>");

    $obj_productContainer.find('select#' + str_groupDefault).addClass('noFilterActive');
    $obj_productContainer.find('select:not([name=quantitySelect])').change(function () {
        product.fcn_poulateSelects({
            $obj_container: $obj_productContainer,
            $obj_currectSelect: $(this),
            fnc_callback: function () {
                var obj_combinationInfo = product.fcn_getCurrenCombinationInfo({
                    $obj_container: $obj_productContainer
                });

                $("#imgThumbs ul").html($("#thumbsBackup").html());
                if (obj_combinationInfo.bol_exist) {
                    $('.product-price').html(obj_combinationInfo.obj_combination.price);
                    $('input[data-stock]').attr("data-stock", obj_combinationInfo.obj_combination.stock);

                    $("#stockStatus i").removeClass("fa-check").removeClass("fa-close");

                    if (parseInt(obj_combinationInfo.obj_combination.stock) > 0) {
                        $("#stockStatus").find("i").addClass("fa-check");
                    } else {
                        $("#stockStatus").find("i").addClass("fa-close");
                    }

                    fcn_favoriteActions({
                        str_action: 'changeVariant',
                        str_newVariant: obj_combinationInfo.obj_combination.id
                    });
                    $obj_productContainer.attr('data-variantid', obj_combinationInfo.obj_combination.id);
                    $obj_productContainer.find('#variantID').attr('disabled', false)
                        .val(obj_combinationInfo.obj_combination.id);

                    // Show variant images
                    for (var img = 0; img < $("#imgThumbs ul > li").length; img++) {
                        if ($("#imgThumbs ul > li").eq(img).attr("data-type")
                            .indexOf("_" + $("#" + str_groupDefault).val() + "_") ==
                            -1) {
                            $("#imgThumbs ul > li").eq(img).addClass("remove");
                        }
                    }
                    if ($("#imgThumbs ul > li.remove").length != $("#imgThumbs ul > li").length)
                        $("#imgThumbs ul > li.remove").remove();

                    $('#product-lg-image img').attr('src', $('#imgThumbs ul li:first').find('img').attr('data-image'));
                    $('#imgThumbs ul li:first').addClass("active");
                } else {
                    $obj_productContainer.find('#variantID').attr('disabled', true);
                    alert($obj_productContainer.attr('data-errorCombination1'));
                }
                /* Thumbs slideshow */
                if (!$("#imgThumbs .bx-wrapper").length && $("#imgThumbs li").length > 0) {
                    thumbsSlider = $('#imgThumbs > ul').bxSlider({
                        slideMargin: 0,
                        autoReload: true,
                        controls: true,
                        infiniteLoop: false,
                        breaks: [
                            { screen: 0, slides: 2, pager: false },
                            { screen: 460, slides: 3 },
                            { screen: 768, slides: 4 }
                        ],
                        pager: false,
                        hideControlOnEnd: true
                    });
                } else if (thumbsSlider != undefined) {
                    thumbsSlider.reloadSlider();
                }
                // change image
                $('#imgThumbs li').click(function () {
                    var video = document.getElementById("video"),
                        $obj_img = $("#product-lg-image img:first"),
                        $obj_imgZoom = $("#product-lg-image img:last"),
                        $obj_i = $("#product-lg-image i");

                    video.pause();
                    if ($(this).find("img").length) {
                        $obj_img.attr("src", $(this).find("img").attr("data-image"));
                        $obj_imgZoom.attr("src", $(this).find("img").attr("data-large"));
                        $("#product-lg-image #videoContainer").addClass("hidden");
                        $obj_img.removeClass("hidden");
                    } else {
                        $("#product-lg-image video").attr("src", $(this).find("video").attr("data-image"));
                        $obj_img.addClass("hidden");
                        $("#product-lg-image #videoContainer").removeClass("hidden");
                        video.play();
                        $obj_i.removeClass().addClass("fa fa-pause");
                        $("#playPause").unbind().click(function (event) {
                            event.preventDefault();

                            if (video.paused) {
                                video.play();
                                $obj_i.removeClass().addClass("fa fa-pause");
                            } else {
                                video.pause();
                                $obj_i.removeClass().addClass("fa fa-play");
                            }

                        });
                    }
                    $('#imgThumbs li').removeClass("active");
                    $(this).addClass("active");
                });
            }
        });
        $("#product-lg-image img:last").attr('src', $("#imgThumbs li.active img").attr('data-large'));
    });

    fnc_favoritesClick();
    fcn_changeQuantity();
    fcn_reviewsRating();

    // Poulate Variant Selects
    product.fcn_poulateSelects({
        $obj_container: $obj_productContainer,
        $obj_currectSelect: $obj_productContainer.find('select#' + str_groupDefault)
    });

    // Change to default variant
    if ($("#" + str_groupDefault).length) {
        $("#" + str_groupDefault).change();
        $("select").selectpicker({ size: 8 });
    }

    // DOM changes
    fcn_pageChanges();

    fcn_zoomImage();

    /* Add To Cart */
    $obj_productContainer.find('.addToCartSubmit').click(function (event) {
        event.preventDefault();
        $(this).closest("[name=cartcmd]").val("add");
        console.log("clickez")
        fcn_addToCart($(this));
    });
    $obj_productContainer.find('#addToCartPointsSubmit').click(function (event) {
        event.preventDefault();
        $("[name=cartcmd]").val("addwithpoints");
        fcn_addToCart($(this));
    });
    $("form[name=addToCart]").submit(function () {
        fcn_addToCart($(this).find("#addToCartSubmit"));
        return false;
    });

    fcn_tabs($("#tabsContainer"));

}

function onLoadProductList(num_min, num_max, str_selected, str_groupIds) {
    var // Elements
        $obj_addToCard = $(".addToCart"),
        $obj_productsList = $(".productsList"),
        $obj_slideRange = $("#slider-range"),
        // Cart
        fcn_addToCart = function ($obj_this) {
            var obj_ajax;

            loading(true);
            if (typeof obj_ajax != "undefined") {
                obj_ajax.abort();
            }

            obj_ajax = $.ajax({
                url: $obj_this.parent().attr("action"),
                data: $obj_this.parent().serialize(),
                success: function (data) {
                    obj_data = data;
                },
                complete: function () {
                    minicart(obj_data);

                    $obj_this.find("span").html($obj_this.find("span").attr("data-added"));
                    $obj_this.addClass("highlighted");

                    setTimeout(function () {
                        $obj_this.find("span").html($obj_this.find("span").attr("data-add"));
                        $obj_this.removeClass("highlighted");
                    },
                        3000);

                    // If variant exists
                    var $obj_variantSelected = $obj_this.closest("form").find("select > option:selected"),
                        $obj_quantity = $obj_this.closest("form").find("input[name=quantity]"),
                        num_finalStock = (parseInt($obj_variantSelected.attr("data-stock")) -
                            parseInt($obj_quantity.val()));

                    $obj_variantSelected.attr("data-stock", num_finalStock);
                    $obj_quantity.attr("data-available", num_finalStock);
                    // ----------

                    loading(false);
                }
            });
        },
        fcn_clickAddToCart = function () {
            $obj_addToCard = $(".addToCart");
            $obj_addToCard.unbind("click").click(function (event) {
                var $obj_input = $(this).closest("form").find(".quantityInput"),
                    $obj_giftCard = $(this).closest("form").find(".giftCardAmount");

                event.preventDefault();

                if ($obj_input.val() == undefined && !$obj_giftCard.length) {
                    alert($(this).closest("form").attr("data-outofstock"));
                } else if ($obj_giftCard.length && $obj_giftCard.find("input").val() == "") {
                    alert($("#listContainer").attr("data-amountrequired"))
                } else if (fcn_validateStock($obj_input)) {
                    fcn_addToCart($(this));
                }
            });
        },
        fcn_sortProducts = function () {
            var $obj_form = $("#orderProdList");

            $obj_form.find("select").change(function () {
                var $obj_sortSelect = $("#sortSelect"),
                    $obj_orderSelect = $("#orderSelect"),
                    $obj_joiner = $("#sortOrderJoin"),
                    $obj_filtersForm = $("#filtersForm"),
                    $obj_joinerSelected = $obj_joiner.find("option:selected"),
                    $obj_pageSize = $("#pageSize"),
                    $obj_amount = $("#amount"),
                    $obj_hiddenFields = $obj_filtersForm.find("input[type=hidden]").not("[name=GroupID]");

                $obj_sortSelect.val($obj_joinerSelected.attr("data-sort"));
                $obj_orderSelect.val($obj_joinerSelected.attr("data-order"));
                $obj_joiner.prop("disabled", true);
                if ($obj_filtersForm.find("[name=SortOrder]").length) {
                    $obj_filtersForm.find("[name=SortOrder]").val($obj_joinerSelected.attr("data-order"));
                } else {
                    $obj_filtersForm.prepend('<input type="hidden" name="SortOrder" value="' +
                        $obj_joinerSelected.attr("data-order") +
                        '">')
                }
                if ($obj_filtersForm.find("[name=sortby]").length) {
                    $obj_filtersForm.find("[name=sortby]").val($obj_joinerSelected.attr("data-sort"));
                } else {
                    $obj_filtersForm.prepend('<input type="hidden" name="sortby" value="' +
                        $obj_joinerSelected.attr("data-sort") +
                        '">')
                }
                if ($obj_filtersForm.find("[name=PageSize]").length) {
                    $obj_filtersForm.find("[name=PageSize]").val($obj_pageSize.val());
                } else {
                    $obj_filtersForm.prepend(
                        '<input type="hidden" name="PageSize" value="' + $obj_pageSize.val() + '">')
                }
                if ($obj_amount.length) {
                    $obj_amount.val($obj_amount.val().replace("$", "").replace("$", "").replace(" - ", ","));
                }
                loading(true);
                // Disable empty fields
                $("input[name=group]").prop("disabled", true);
                for (var filter = 0; filter < $obj_hiddenFields.length; filter++) {
                    if ($obj_hiddenFields.eq(filter).val() == "") {
                        $obj_hiddenFields.eq(filter).prop("disabled", true);
                    }
                }
                $obj_filtersForm.trigger("submit");
            });
        },
        // Change variant
        fcn_updateStock = function () {
            var $obj = $("select[name=variantID]");
            var $obj_quantitySelect = $obj.closest("form").find("select[name='quantitySelect']");

            if ($obj_quantitySelect.length) {
                var $obj_selectOption = $obj_quantitySelect.find('option');
                var quantityAvailable = $obj.closest("form").find("input[name=quantity]").attr("data-available");

                if (quantityAvailable <= 0) {
                    $obj_quantitySelect.attr('disabled', true);
                    $obj.closest("form").find(".stockStatus i").removeClass("fa-check").addClass("fa-close");
                } else {
                    $obj_quantitySelect.attr('disabled', false);
                    $obj_selectOption.addClass("hidden");
                    $obj.closest("form").find(".stockStatus i").removeClass("fa-close").addClass("fa-check");
                }

                for (var i = 0, j = $obj.closest("form").find("input[name=quantity]").attr("data-available") ; i < (j || 9) ; i++) {
                    $obj_selectOption.eq(i).removeClass("hidden");
                }

                $("select").selectpicker('refresh');
            }
        },
        fcn_changeVariantComb = function () {
            var $obj_select = $("select[name=variantID]");

            fcn_updateStock();

            $obj_select.change(function () {
                var $obj_this = $(this),
                    $obj_favorite = $obj_this.closest("form").find(".favorite");

                $obj_this.closest("form").find("input[name=quantity]").attr("data-available", $obj_this.find("option:selected").attr("data-stock"));

                /* Remove comment if variants not exists
                $obj_this.closest("form").find(".favorite").attr("href", $obj_favorite.attr("href").split("CCAddToListVariantID=")[0] + "CCAddToListVariantID=" + $obj_this.closest("form").find("select[name=variantID]").val() + "&CCAddToListLanguageID=" + $obj_favorite.attr("href").split("&CCAddToListLanguageID=")[1]);
                */
                $obj_this.closest("form").find(".favorite").addClass("hidden");
                $obj_this.closest("form").find(".favorite[data-variant=" + $obj_this.val().replace(".", "_") + "]").removeClass("hidden");

                fcn_updateStock();
            });
        },
        // change quantity select
        fcn_changeQuantity = function () {
            var $obj_inputQuantity = $(".quantityInput"),
                $obj_quantitySelect = $("select[name=quantitySelect]"),
                fcn_onChange = function ($el) {
                    if ($el.val() == "10+") {
                        $el.closest("form").find("input.hidden").removeClass("hidden");
                        $el.closest("form").find(".quantity-container .bootstrap-select").addClass("hidden").val("10");
                        $el.closest("form").find("input.quantityInput").val("10");
                    } else {
                        $el.closest("form").find("input.quantityInput").val($el.val());
                    }
                };

            $obj_quantitySelect.change(function () {
                fcn_onChange($(this));
            });
            $obj_inputQuantity.change(function () {
                var $obj_this = $(this);

                fcn_validateStock($obj_this);
                $obj_this.blur(function () {
                    if ($obj_this.val() < 10) {
                        $obj_this.addClass("hidden");
                        $obj_this.closest("fieldset").find("select").val($obj_this.val());
                        $obj_this.closest("fieldset").find(".bootstrap-select").removeClass("hidden");
                        $("select").selectpicker("refresh");
                    }
                });
            });
        },
        fcn_validateStock = function ($obj_el) {
            var bol_return = true;

            if (parseInt($obj_el.val()) > parseInt($obj_el.attr("data-available")) ||
                parseInt($obj_el.val()) <= 0 ||
                parseInt($obj_el.attr("data-available")) < 1) {
                //                alert($obj_el.attr("data-stocktranslate") + " " + $obj_el.attr("data-available"));
                alert($obj_el.attr("data-outofstock"));
                bol_return = false;
            }

            return bol_return;
        },
        // Favorites
        fnc_favoritesClick = function () {
            var $obj_favoriteBtn = $(".favorite");

            $obj_favoriteBtn.unbind("click").click(function (e) {
                var $obj_this = $(this);

                e.preventDefault();
                loading(true);
                if ($obj_this.attr("data-user") == "True") {
                    requestAjax({
                        str_url: $obj_this.attr("href"),
                        str_type: "get",
                        obj_data: null,
                        fnc_callback: function (obj_response) {
                            if ($obj_this.attr("data-favorite") == "True") {
                                $obj_this.html('<i class="fa fa-heart-o"></i> ');
                                $obj_this.attr("data-favorite", "False");
                                $obj_this.attr("href", $obj_this.attr("data-add"));
                            } else {
                                $obj_this.html('<i class="fa fa-heart"></i> ');
                                $obj_this.attr("data-favorite", "True");
                                $obj_this.attr("href", $obj_this.attr("data-remove"));
                            }
                            loading(false);
                        }
                    });
                }
            });
        },
        // Filter functions
        fcn_filtersChange = function () {
            var $obj_select = $('#filtersForm select'),
                $obj_input = $("#filtersForm input[type=text], #filtersForm input[type=hidden]").not("[name=GroupID]");

            loading(true);

            if ($(".toSearch input:checked").length > 0) {
                $("#filtersForm").attr("action", $("#filtersForm").attr("data-href"));
                $("#filtersForm").attr("method", "get");
            }

            for (var a = 0; a < $obj_input.length; a++) {
                if ($obj_input.eq(a).val() == "") {
                    $obj_input.eq(a).prop("disabled", true);
                }
            }
            for (var i = 0; i < $obj_select.length; i++) {
                if ($obj_select.eq(i).val() == "") {
                    $obj_select.eq(i).prop("disabled", true);
                }
            }

            // if price range exists in the filters
            if ($("#amount").length) {
                $("#amount").val($("#amount").val().replace("$", "").replace("$", "").replace(" - ", ","));
            }

            var groupIDs = "";
            // Remove check for the parent groups
            //$(".groupFieldset.level_0 legend input").prop("checked", false);

            for (var group = 0; group < $("input[name=group]:checked").length; group++) {
                groupIDs += groupIDs != "" ? "," : "";
                groupIDs += $("input[name=group]:checked").eq(group).val();
                $("input[name=group]:checked").eq(group).prop("disabled", true);
            }
            if (groupIDs != "") {
                $("input[name=GroupID]").val(groupIDs);
            }
            else {
                $("input[name=GroupID]").prop("disabled", true);
            }

            $("#filtersForm").submit();
        },
        fcn_toogleFilters = function ($obj_el) {
            $obj_el.siblings("div:not(.containerDeep_1, .containerDeep_2)").slideToggle(function () {
                if ($(this).parent().hasClass("active")) {
                    $(this).parent().removeClass("active");
                } else {
                    $(this).parent().addClass("active");
                }
            });
        },
        fcn_verifyValueIsNotEmpty = function () {
            var $obj_form = $("#filtersForm"),
                $obj_fieldset = $obj_form.find("fieldset");

            for (var i = 0; i < $obj_fieldset.length; i++) {
                if (($obj_fieldset.eq(i).find("input[type=text]").length && $obj_fieldset.eq(i).find("input[type=text]").val() != "") ||
                    ($obj_fieldset.eq(i).find("select").length && $obj_fieldset.eq(i).find("select").val() != null) ||
                    $obj_fieldset.eq(i).find("input[type=checkbox]:checked").length) {
                    fcn_toogleFilters($obj_fieldset.eq(i).not(".level_1").not(".level_2").find("legend").not(".notClick"));
                }
            }
        },
        fcn_priceRange = function () {
            var str_minRange = localStorage.getItem(str_projectFolder + "minRange"),
                str_maxRange = localStorage.getItem(str_projectFolder + "maxRange");

            if (str_minRange != undefined) {
                if (str_minRange < num_min && num_min == 0) {
                    num_min = str_minRange;
                }
                if (str_maxRange > num_max) {
                    num_max = str_maxRange;
                }
            }
            localStorage.setItem(str_projectFolder + "minRange", num_min);
            localStorage.setItem(str_projectFolder + "maxRange", num_max);

        },
        fcn_selectGroupFilters = function () {
            if (str_groupIds != "" && str_groupIds.indexOf(",") > -1) {
                var groupSplited = str_groupIds.split(",");

                for (var group = 0; group < groupSplited.length; group++) {
                    $("input[value=" + groupSplited[group] + "]").prop("checked", true);
                }
            }
            else if (str_groupIds != "") {
                $("input[value=" + str_groupIds + "]").prop("checked", true);
            }
        },
        // Compare products
        fcn_addToCompare = function () {
            var num_maxToCompare = 3,
                $obj_addToCompare = $(".addToCompare input"),
                bol_firstCompare = true,
                fcn_ajax = function () {
                    var obj_savedProd = ShareIT.Ecommerce.fcn_compareProducts("consult"),
                        str_prodKeys = obj_savedProd != null ? obj_savedProd.productsToCompare.toString() : "";

                    // Get products to footer
                    if (str_prodKeys != "") {
                        if (!bol_firstCompare) loading(true);
                        bol_firstCompare = false;
                        requestAjax({
                            str_url: window.location.href.split("?")[0] +
                                "?Compare=" +
                                str_prodKeys +
                                "&referrer=" + encodeURIComponent((location.pathname + location.search).substr(1)) +
                                "&list=true&" +
                                obj_globalOptions.obj_urlPaths.str_ajaxRetriever,
                            str_type: "get",
                            obj_data: null,
                            fnc_callback: function (obj_response) {
                                $("#compareProducts").remove();
                                $("#centralContainer").after($(obj_response).find("#compareProductsAjax").html());
                                fcn_compareBtn();
                                if (localStorage.getItem(str_projectFolder + '_compareProductsOpen') == undefined) {
                                    $("#compareProducts").css("visibility", "hidden");
                                    var $obj_header = $("#compareProducts header");
                                    $("#compareProducts").animate({
                                        bottom: "-" + ($("#compareProducts").outerHeight() - $obj_header.outerHeight())
                                    },
                                        1,
                                        function () {
                                            $obj_header.find("i:first").removeClass().addClass("fa fa-caret-up");
                                            $obj_header.addClass("active");
                                            $("#compareProducts").css("visibility", "visible");
                                        });
                                }

                                // Remove product from compare list
                                $(".removeItem").click(function () {
                                    var $obj_this = $(this);

                                    ShareIT.Ecommerce.fcn_compareProducts("remove", $obj_this.attr("data-link"));
                                    $("#addToCompare_" + $obj_this.attr("data-link").split("$")[0]).trigger("click")
                                        .prop("checked", false);
                                    fcn_ajax();
                                });

                                // Show and Hide Compare list
                                $("#compareProducts header").click(function () {
                                    var num_bottom = 0,
                                        str_class = "fa fa-caret-down",
                                        $obj_this = $(this);

                                    if ($obj_this.hasClass("active")) {
                                        num_bottom = 0;
                                        $obj_this.removeClass("active");
                                        localStorage.setItem(str_projectFolder + '_compareProductsOpen', "true");
                                    } else {
                                        num_bottom = "-" + ($obj_this.parent().outerHeight() - $obj_this.outerHeight());
                                        $obj_this.addClass("active");
                                        str_class = "fa fa-caret-up";
                                        localStorage.removeItem(str_projectFolder + '_compareProductsOpen');
                                    }
                                    $("#compareProducts").animate({
                                        bottom: num_bottom
                                    },
                                        500,
                                        function () {
                                            $obj_this.find("i:first").removeClass().addClass(str_class);
                                        });
                                });

                                loading(false);
                            }
                        });
                    } else {
                        $("#compareProducts").remove();
                    }
                };

            $obj_addToCompare.change(function () {
                var $obj_this = $(this),
                    obj_savedProd = ShareIT.Ecommerce.fcn_compareProducts("consult");

                bol_firstCompare = false;
                if ($obj_this.is(':checked')) {
                    if (obj_savedProd == null || obj_savedProd.productsToCompare.length < num_maxToCompare) {
                        ShareIT.Ecommerce.fcn_compareProducts("add", $obj_this.attr("data-link"));
                        fcn_ajax();
                    } else {
                        $obj_this.prop("checked", false);
                        alert($("[data-maxcompare]").attr("data-maxcompare"))
                    }
                } else {
                    ShareIT.Ecommerce.fcn_compareProducts("remove", $obj_this.attr("data-link"));
                    fcn_ajax();
                }

            });
            fcn_ajax();
        },
        fcn_checkAddToCompare = function () {
            var obj_productsAdded = ShareIT.Ecommerce.fcn_compareProducts("consult");

            if (obj_productsAdded != null) {
                for (var i = 0; i < obj_productsAdded.productsToCompare.length; i++) {
                    $("input[data-link='" + obj_productsAdded.productsToCompare[i] + "']").prop("checked", true);
                }
            }
        },
        fcn_compareBtn = function () {
            /*$(".compareButton").click(function() {
                localStorage.setItem(str_projectFolder + '_lastProdPage', window.location.href);
            });*/
        },
        // Initialize
        fcn_init = function () {


            // START Sidebar filter navigation


            // hide the lis without children
            $("li.l2 > a").each(function(index, el){
                if($(this).next(".M2").length == 0) {
                    $(this).hide();
                }
            });


            // check level 3 links -- can be replaced with new functionality based on list-open-active class
            $(".l3 > a").each(function(index,el){
                var url = $(this).attr("href");
                var windowLocation = window.location.href;


                if(windowLocation.indexOf(url) != -1) {
                    // $(this).css({
                    //     "color" : "#23527c",
                    //     "font-weight" : "bold"
                    // });

                    // show active ul
                    $(this).parent().parent().css("display" , "block");
                    $(this).parent().parent().parent().addClass("active");
                }

            });


            $("li.l2 > a").on("click" , function(event){
                event.preventDefault();



                if($(this).next(".M2").length > 0) {

                    if(!$(this).parent("li").hasClass("active")) {
                        $(this).parent("li").addClass("active");
                    } else {
                        $(this).parent("li").removeClass("active");
                    }

                    
                    $(this).next(".M2").slideToggle("slow");

                    var siblings = $(this).parent().siblings().children("ul");

                    $.each(siblings , function(index , element){

                      if($(element).not(":hidden")) {
                        $(this).hide('slow');
                      }

                      if($(element).parent().hasClass("active")){
                        $(element).parent().removeClass("active");
                      }

                    });
                } 

            });



            // END Sidebar filter navigation



        	//active filters events
            $('.removeFiltersContainer a.filter--active').on("click", function(e){
                e.preventDefault();
                  // loadingBF(true);
                var query = $(this).attr("data-query-name");
                var value = $(this).attr("data-query-value");
                var currentUrl = window.location.href;
                // console.log(query+'='+value);
                

                var stringSecondPart = _.indexOf(currentUrl.split(query+'='+value)[1], "&") == '0' ? currentUrl.split(query+'='+value)[1].substring(1) : currentUrl.split(query+'='+value)[1];      
                var stringFirstPart = currentUrl.split(query+'='+value)[0];
                var newUrl = stringFirstPart + stringSecondPart;
                newUrl = _.lastIndexOf(newUrl, "&") == _.size(newUrl) - 1 ? newUrl.substring(0, _.size(newUrl) - 1) : newUrl;
                // console.log(newUrl);
                // console.log(newUrl)
                window.location.href = newUrl;
                
              });
            //remove all filters events
            $('.removeAll').on("click", function(e){
					    e.preventDefault();					    
					    $('#filtersForm input[type="text"]','select').val("");
					    $('#filtersForm input[type="checkbox"]').prop("checked", false);
					    $('#filtersForm').submit();

					  });

            $('.checkBoxContainer ').each(function(){
            	var $input = $(this).find('input[type="checkbox"]')
            	console.log($input.prop("checked"))
            	if ($input.prop("checked")) {
            		$(this).addClass("selected");
            	}
            });

            
            var str_maxValueSelected = str_selected != "" ? str_selected.split(",")[1] : num_max.toString();
            // Init functions
            fcn_clickAddToCart();
            fcn_sortProducts();
            fnc_favoritesClick();
            fcn_changeQuantity();
            fcn_changeVariantComb();
            fcn_addToCompare();
            fcn_checkAddToCompare();
            fcn_priceRange();
            fcn_selectGroupFilters();

            $(window).resize(function () {
                fcn_resize();
            });

            // style filters form
            styleForm("#filtersForm");
            $("select").selectpicker({ size: 8 });

            $("#filtersForm ul.selectpicker")
                .before(
                    "<div class='fieldContainer'><label><input class='allChecks' type='checkbox'> All</label></div>")
                .after(
                    "<div class='selectButtons'><button type='reset' class='btn'>Clear</button><button type='button' class='btn btn-bg'>Apply</button></div>");

            $("#filtersForm .selectButtons button[type=reset]").click(function (event) {
                event.preventDefault();

                $(this).closest("fieldset").find("select").selectpicker('deselectAll');
                fcn_filtersChange();
            });
            $("#filtersForm .selectButtons button[type=button]").click(function (event) {
                event.preventDefault();

                fcn_filtersChange();
            });

            $(".allChecks").change(function () {
                $(this).closest(".bootstrap-select").find(".dropdown-toggle").trigger("click");
                if ($(this).prop("checked") == true) {
                    $(this).closest("fieldset").find("select").selectpicker('selectAll');
                } else {
                    $(this).closest("fieldset").find("select").selectpicker('deselectAll');
                }
            });

            /* Price range */
            $obj_slideRange.slider({
                range: true,
                min: parseInt(num_min),
                max: parseInt(num_max),
                values: [parseInt(num_min), parseInt(num_max)],
                slide: function (event, ui) {
                    $("#amount").val("$" + ui.values[0] + " - $" + ui.values[1]);
                }
            });

            $("#amount").val("$" + $("#slider-range").slider("values")[0] + " - $" + str_maxValueSelected);

            if (str_selected != "") {
                str_selected = str_selected == "0,0" ? num_min + "," + num_max : str_selected;

                $("#slider-range").slider({
                    values: [parseInt(str_selected.split(",")[0]), parseInt(str_selected.split(",")[1])]
                });
            }

            /* Show/Hide filter group */
            $("#filtersForm .fieldset > div.hide, #filtersForm .btn-group").removeClass("hide").hide();
            $("#filtersForm legend:not(.deep_1):not(.deep_2)").not(".notClick").click(function () {
                fcn_toogleFilters($(this));
            });



            //activating first category drawer
            $('#filtersForm .fieldset').each(function(){
            	var children = $(this).find(".filtersContainer").children();
            	if (children.length > 0) {
            		console.log("children")
            		$(this).addClass("active", children);
            		$(this).find(".filtersContainer").show();
            		console.log("executed")
            		return false;
            	}

            });




            $(".seeMore").click(function () {
                var $obj_this = $(this);

                if ($obj_this.siblings("label.hide").length) {
                    $obj_this.siblings("label.hide").addClass("hide_").removeClass("hide");
                    $obj_this.html($(this).attr("data-hide"));
                    $obj_this.addClass("active");
                } else {
                    $obj_this.siblings("label.hide_").addClass("hide").removeClass("hide_");
                    $obj_this.html($(this).attr("data-more"));
                    $obj_this.removeClass("active");
                }
            });

            /* Change list grid */
            if ($obj_productsList.hasClass("tile")) $("body").addClass("tileStyle");
            $(".listMode > li").click(function () {
                var $obj_this = $(this),
                    $obj_productsList = $(".productsList");

                if (!$obj_this.hasClass("active")) {
                    $(".listMode > li").removeClass("active");
                    $obj_this.addClass("active");
                    $obj_productsList.removeClass("list tile row").addClass($obj_this.attr("data-list"));

                    if ($obj_this.attr("data-list") == "list") {

                        $obj_productsList.find("> li").removeClass("col-lg-3 col-md-4 col-sm-6").addClass("border");
                        $("body").removeClass("tileStyle");
                        /* We are using cookies instead localsotrage because we can control the cookie in C# and the localstorage not */
                        $.cookie("dataList", "list", { expires: 365 });
                        //localStorage.setItem(str_projectFolder + '_dataList', "list");
                    } else {
                        $obj_productsList.find("> li").removeClass("border").addClass("col-lg-3 col-md-4 col-sm-6");
                        $("body").addClass("tileStyle");
                        $obj_productsList.addClass("row");
                        /* We are using cookies instead localsotrage because we can control the cookie in C# and the localstorage not */
                        $.cookie("dataList", "tile", { expires: 365 });
                        //localStorage.setItem(str_projectFolder + '_dataList', "tile");
                    }

                    //fcn_resize();
                    setTimeout(function () {
                        fcn_resize();
                    }, 200);

                }
            });

            if ($obj_productsList.attr("data-list") == "tile") {
                $(".listMode > li[data-list=tile]").trigger("click");
            }
            $obj_productsList.removeClass("hidden");

            // Index filters - change
            $("#filtersForm input[type=checkbox]:not(.allChecks), #filtersForm select:not([data-live-search])").change(
                function () {
                    fcn_filtersChange();
                });

            // $(".removeFiltersContainer > a").click(function (event) {
            //     event.preventDefault();

            //     if (!$(this).hasClass("removeAll")) {
            //         $("[name=" + $(this).attr("data-queryname") + "]").val("").prop("checked", false);
            //     } else {
            //         $("input, select").val("").prop("checked", false);
            //     }
            //     fcn_filtersChange();
            // });

            $(".ui-slider-handle").blur(function () {
                fcn_filtersChange();
            });

            // Variant change
            if ($(".removeFiltersContainer a").length == 1) $(".removeFiltersContainer").remove();

            $("select[name=variantID]").change(function () {
                if ($(this).find("option:selected").attr("data-price") != undefined) {
                    $(this).closest("li").find(".listPrice").html($(this).find("option:selected").attr("data-price"));
                }
            });

            fcn_verifyValueIsNotEmpty();

            $(window).load(function () {
                fcn_resize();
            });
        },
        fcn_resize = function () {
            var $obj_productList = $(".productsList");

            if (obj_globalOptions.obj_strings.str_deviceDim == "xs") {
                num_el = 1;
                $obj_productList.addClass("tile");
            } else if (obj_globalOptions.obj_strings.str_deviceDim == "sm") {
                num_el = 2;
                if ($(".listMode > li[data-list=list]").hasClass("active")) {
                    $obj_productList.removeClass("tile");
                }
            } else if (obj_globalOptions.obj_strings.str_deviceDim == "md") {
                num_el = 3;
                if ($(".listMode > li[data-list=list]").hasClass("active")) {
                    $obj_productList.removeClass("tile");
                }
            } else {
                num_el = 4;
                if ($(".listMode > li[data-list=list]").hasClass("active")) {
                    $obj_productList.removeClass("tile");
                }
            }

            if ($(".productsList.tile").length) {
                normalizeListItemHeight(".productsList > li .priceContainer ",
                    num_el,
                    function () {
                        normalizeListItemHeight(".productsList > li figure",
                            num_el,
                            function () {
                                normalizeListItemHeight(".productsList > li h2",
                                    num_el,
                                    function () {
                                        normalizeListItemHeight(".productsList > li > div > div", num_el,
                                            function () {
                                                //                                                normalizeListItemHeight(".productsList > li", num_el);
                                            });
                                    });
                            });
                    });
            } else {
                $(".productsList > li > div > div, " +
                    ".productsList > li figure," +
                    ".productsList > li > div > div," +
                    ".productsList > li h2," +
                    ".productsList > li .priceContainer," +
                    ".productsList > li").removeAttr("style");
            }

        };


    /* todo infinite scroll
     infiniteScroll({$obj_holder:$('#productsList'), $obj_nextAnchor:$('.next a'), num_maxPages:$('.next a').attr("data-numPages")}, null, fcn_clickAddToCart);
     */

    fcn_init();
}

function infiniteScroll(obj_params, fcn_callback, fcn_afterAjax) {
    var $obj_window = $(window),
        $obj_images,
        obj_ajax,
        bol_loading = false,
        str_href =
            typeof obj_params.str_extraParams != "undefined" &&
                !obj_params.$obj_nextAnchor.attr("href").contains(obj_params.str_extraParams)
                ? obj_params.$obj_nextAnchor.attr("href") + obj_params.str_extraParams
                : obj_params.$obj_nextAnchor.attr("href"),
        str_infiniteLoader = obj_params.bol_blockContent ? "<div class='infiniteLoaderDiv'></div>" : "",
        str_html = "",
        num_page = 1,
        num_maxPages = obj_params.num_maxPages,
        num_oldHolderHeight,
        num_bottom = $obj_window.height() > (obj_params.$obj_holder.height() + obj_params.$obj_holder.offset().top)
            ? (obj_params.$obj_holder.height() + obj_params.$obj_holder.offset().top)
            : (($(document).height() - $obj_window.height()) - $('footer').height());


    $obj_window.unbind("scroll").scroll(function (event) {
        if ($(window).scrollTop() >= num_bottom) {
            if (!bol_loading && num_page < num_maxPages && !$('.infiniteLoaderDiv').length) {
                str_href =
                    typeof obj_params.str_extraParams != "undefined" &&
                    !obj_params.$obj_nextAnchor.attr("href").contains(obj_params.str_extraParams)
                    ? obj_params.$obj_nextAnchor.attr("href") + obj_params.str_extraParams
                    : obj_params.$obj_nextAnchor.attr("href");
                bol_loading = true;
                obj_params.$obj_holder.append(str_infiniteLoader +
                    "<img src='Files/Templates/Designs/" +
                    str_projectFolder +
                    "/images/ajaxLoader.gif' alt='Loading...' class='infiniteLoader'/>");
                num_oldHolderHeight = obj_params.$obj_holder.height();

                if (typeof obj_ajax != "undefined") {
                    obj_ajax.abort();
                }

                obj_ajax = $.ajax({
                    url: str_href,
                    success: function (obj_data) {
                        str_html = obj_data;
                    },
                    complete: function () {
                        num_page++;
                        obj_params.$obj_holder.children('.infiniteLoader, .infiniteLoaderDiv').remove();

                        obj_params.$obj_holder.append($(str_html).find('#' + obj_params.$obj_holder.attr("id"))
                            .children());

                        obj_params.$obj_nextAnchor.attr("href",
                            $(str_html).find('#' + obj_params.$obj_nextAnchor.attr("id")).attr("href"));
                        $obj_images = obj_params.$obj_holder.find('img');
                        $obj_images.load(function () {
                            num_bottom =
                                $obj_window.height() >
                                (obj_params.$obj_holder.height() + obj_params.$obj_holder.offset().top)
                                ? (obj_params.$obj_holder.height() + obj_params.$obj_holder.offset().top)
                                : (($(document).height() - $obj_window.height()) - $('footer').height());
                            bol_loading = false;
                        });

                        if (fcn_callback != null) {
                            fcn_callback(str_html);
                        }
                        if (fcn_afterAjax != null) {
                            fcn_afterAjax();
                        }
                    }
                });
            }
        }
    });
}


