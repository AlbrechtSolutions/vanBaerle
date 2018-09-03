/**
 * Description: javascript functions relative to aTools
 * User: ShareIT
 * Date: 18/May/2015
 **/

var ShareIT = {},
    str_projectFolder = "RizzoDesignFolder",
    obj_globalOptions = {
        obj_strings:{
            str_htmlBr:"<br/>",
            str_jsBr:"\n",
            str_active:"active",
            str_error:"fieldError",
            str_deviceDim:""
        },
        obj_cssClass:{
            str_hidden:"hidden",
            str_block:"block"
        },
        obj_urlPaths:{
            str_ajaxRetriever:"LayoutTemplate=Designs/" + str_projectFolder + "/ajaxRetriever.cshtml",
            str_ajaxRetrieverCoded:"LayoutTemplate=Designs%2f" + str_projectFolder + "%2fajaxRetriever.cshtml",
            str_ajaxDataManagement:"Public/DataManagementPublishingOutput.ashx?PubID=",
            str_ajaxGetFolderContents:"/public/getfoldercontents.ashx",
            str_GetNavigationXML:"/Admin/Public/GetNavigationXML.aspx",
            str_getImage:"/admin/public/getimage.ashx?Image=",
            str_theme:"/Files/Templates/Designs/" + str_projectFolder
        },
        obj_bools:{
            bol_windowResizeFired:false
        },
        obj_nums:{
            num_animationSpeed:500
        },
        obj_objects:{
            $obj_window:$(window),
            obj_resizeDelay:null
        },
        obj_browsers:{
            is_chrome:navigator.userAgent.indexOf('Chrome') > -1,
            is_safari:navigator.userAgent.indexOf("Safari") != -1 && navigator.userAgent.indexOf('Chrome') == -1,
            is_android:((navigator.userAgent.indexOf('Mozilla/5.0') > -1 && navigator.userAgent.indexOf('Android ') > -1 && navigator.userAgent.indexOf('AppleWebKit') > -1) && !(navigator.userAgent.indexOf('Chrome') > -1))
        }
    };
    ShareIT.obj_helpers = {
        fancybox:function ($obj) {
            $obj.fancybox({
                openEffect:'elastic',
                closeEffect:'elastic',
                helpers:{
                    media:{}
                }
            });
        }
    };

window.alert = function (str_txt, str_warning, str_button) {
    customAlert(str_txt, str_warning, str_button)
};

$(function () {
    onLoadManageWindowResize();
});

$(window).load(function () {
    windowAToolsResize();
});

function onLoadManageWindowResize() {
    //variables to confirm window height and width
    var lastWindowHeight = $(window).height(),
        lastWindowWidth = $(window).width();

    updateMediaQuery(Math.max(document.documentElement.clientWidth, window.innerWidth || 0));
    obj_globalOptions.obj_objects.$obj_window.resize(function () {

        if (!obj_globalOptions.obj_bools.bol_windowResizeFired && ($(window).height() != lastWindowHeight || $(window).width() != lastWindowWidth)) {
            obj_globalOptions.obj_bools.bol_windowResizeFired = true;
            //set this windows size
            lastWindowHeight = $(window).height();
            lastWindowWidth = $(window).width();

            clearTimeout(obj_globalOptions.obj_objects.obj_resizeDelay);
            obj_globalOptions.obj_objects.obj_resizeDelay = setTimeout("windowAToolsResize()", 400);
        }
    });
}

function onLoadScreenSize() {
    $('body').append("<div id='screenSize'></div>");
}

function checkDemoMode() {
    if ($('.demoMode').length) {
        $('.demoMode').click(function () {
            $(".demoMode, #screenSize, #toggleGuides").remove();
            $.cookie("demoMode", "noShow", { expires:1 });
        });
    }
}

function customAlert(str_txt, str_warning, str_button, str_labelBtn, bol_cancel, fcn_callback) {
    var str_generalTitle = typeof obj_formErrorMessages == "undefined" ? "" : obj_formErrorMessages.str_errorMessagesGeneralTitle,
        str_title = typeof str_warning != "undefined" ? str_warning : str_generalTitle,
        str_buttonClass = typeof str_button != "undefined" ? str_button : "btn btn-bg";

    str_labelBtn = str_labelBtn == null ? "OK" : str_labelBtn;

    $("body").css("overflow-y", "scroll");
    if (bol_cancel) {
        bootbox.confirm({
            message:str_txt,
            title:str_title,
            callback:function (result) {
                $("body").css("overflow-y", "auto");

                if (fcn_callback != null && result) fcn_callback();
            }
        });
    }
    else {
        bootbox.dialog({
            message:str_txt,
            title:str_title,
            buttons:{
                main:{
                    label:str_labelBtn,
                    className:str_buttonClass,
                    callback:function () {
                        $("body").css("overflow-y", "auto");
                    }
                }
            }
        });
    }
}

function guideLines(num_columns, bol_outGutter) {
    var str_div = "<div class=\"col-xs-1 col-sm-1 col-md-1 col-lg-1\"></div>",
        str_outGutter = bol_outGutter ? "outGutter" : "";
    String.prototype.repeat = function (num) {
        return new Array(num + 1).join(this);
    }

    $('body').append("<button id=\"toggleGuides\">Toggle Guides</button><div id=\"guides\" class=\" hidden\"><div class=\"container\"><div class=\"row " + str_outGutter + "\"><div class=\"firstGuide col-xs-1 col-sm-1 col-md-1 col-lg-1\"></div>" + str_div.repeat(num_columns - 2) + "<div class=\"lastGuide col-xs-1 col-sm-1 col-md-1 col-lg-1\"></div></div></div></div>")
    $('#toggleGuides').click(function () {
        $('#guides').toggleClass("hidden")
    });
}

function debugTimers(str_name) {
    /*var dat_currentTime = new Date(),
     dat_startTime = new Date().getTime(),
     dat_lastTime = new Date().getTime(),;

     console.log("Name: " + str_name + " - From last: " + (dat_currentTime.getTime() - dat_lastTime) + "ms - Total Time elapsed: " + (dat_currentTime.getTime() - dat_startTime) + "ms");
     console.log("------------------------------------------------------------")
     dat_lastTime = new Date().getTime();
     */
}

function isIE() {
    var str_navigator = navigator.userAgent.toLowerCase();
    return (str_navigator.indexOf('msie') != -1) ? parseInt(str_navigator.split('msie')[1]) : false;
}

function normalizeListItemHeight(str_itemListSelector, num_modLineItems, fnc_callback) {
    var num_maxHeight,
        num_itemHeight,
        num_itemIndex;

    $(str_itemListSelector).each(function (i) {
        if (i % num_modLineItems === 0) {
            num_maxHeight = $(this).height("auto").height();
            num_itemIndex = i;
            for (var x = 0; x <= num_modLineItems - 1; x++) {
                num_itemHeight = $(str_itemListSelector).eq(num_itemIndex + x).height();
                if (num_maxHeight < num_itemHeight) {
                    num_maxHeight = num_itemHeight;
                }
            }
            for (var y = 0; y <= num_modLineItems - 1; y++) {
                $(str_itemListSelector).eq(num_itemIndex + y).height(num_maxHeight);
            }
        }
    });
    if (typeof fnc_callback != 'undefined') {
        fnc_callback();
    }
}

function parseScript(_source, indexScriptTag) {
    var source = _source,
        scripts = new Array();

    // Strip out tags
    if (typeof _source != "undefined") {
        while (source.indexOf("<script") > -1 || source.indexOf("</script") > -1) {
            var s = source.indexOf("<script");
            var s_e = source.indexOf(">", s);
            var e = source.indexOf("</script", s);
            var e_e = source.indexOf(">", e);
            // Add to scripts array
            scripts.push(source.substring(s_e + 1, e));
            // Strip from source
            source = source.substring(0, s) + source.substring(e_e + 1);
        }


        // Loop through every script collected and eval it
        for (var i = 0; i < scripts.length; i++) {
            try {

                //eval(scripts[i]);
                if (indexScriptTag == undefined) {
                    $.globalEval(scripts[i]);
                }
                else if (indexScriptTag == i) {
                    $.globalEval(scripts[indexScriptTag]);
                    //updateOrderLines();
                }
            } catch (ex) {
                // do what you want here when a script fails
            }
        }

        // Return the cleaned source
        return source;
    }
    else {
        //console.log("Source mal definida. (undefined)");
    }
}

function requestAjax(obj_options) {

    var obj_data = [],
        str_type = "post",
        bol_crossDomain = true;

    if (typeof obj_options.obj_data != "undefined") {
        obj_data = obj_options.obj_data;
    }

    if (typeof obj_options.str_type != "undefined") {
        if (obj_options.str_type != "") {
            str_type = obj_options.str_type;
        }
    }


    if (typeof obj_options.str_url != "undefined") {

        if (isIE() && isIE() <= 9) {
            bol_crossDomain = false;
        }

        $.ajax({
            type:str_type,
            url:obj_options.str_url,
            data:obj_data,
            headers:{
                "Pragma":"no-cache",
                "Cache-Control":"no-store, no-cache, must-revalidate, post-check=0, pre-check=0",
                "Expires":0,
                "Last-Modified":new Date(0),
                "If-Modified-Since":new Date(0)
            },
            crossDomain:bol_crossDomain,
            cache:false,
            async:true,
            dataType:'html',
            success:function (obj_response) {
                obj_options.fnc_callback(obj_response);
            }
        });
    }
}

function updateMediaQuery(num_width) {
    var num_deviceXS = 480,
        num_deviceSM = 768,
        num_deviceMD = 992,
        num_deviceLG = 1200;

    if (num_width < (num_deviceSM - 1)) {
        obj_globalOptions.obj_strings.str_deviceDim = "xs";
    }
    if (num_width >= num_deviceSM && num_width < (num_deviceMD - 1)) {
        obj_globalOptions.obj_strings.str_deviceDim = "sm";
    }
    if (num_width >= num_deviceMD && num_width < (num_deviceLG - 1)) {
        obj_globalOptions.obj_strings.str_deviceDim = "md";
    }
    if (num_width >= num_deviceLG) {
        obj_globalOptions.obj_strings.str_deviceDim = "lg";
    }
}

function verticalTable($obj_table, bol_vertical, fcn_callback) {
    /// <summary>Turns a horizontal table to vertical, and back.</summary>
    /// <param name="$obj_table" type="jQuery Object">The table to turn vertical</param>
    /// <param name="bol_vertical" type="Boolean">If the table has already been changed, will it become vertical or horizontal? (default: horizontal)</param>

    var $obj_newTableHolder = $("<div class='verticalTableHolder'></div>");

    if (typeof obj_verticalTables == "undefined" || typeof obj_verticalTables[$obj_table.attr('id')] == "undefined") {
        if (bol_vertical) {
            var $obj_newTable = $obj_table.clone(),
                $obj_headers = $obj_table.find('th'),
                $obj_cols = $obj_table.find('td');

            $obj_table.attr("data-tableType", "horizontal").after($obj_newTableHolder);
            $obj_newTable.attr("data-tableType", "vertical").html("");

            for (var i = 0; i < $obj_headers.length; i++) {
                var $obj_tempRow = $("<tr></tr>");
                $obj_newTable.append($obj_tempRow);
                $obj_tempRow.append($($obj_headers[i]).clone());
                for (var col = i; col < $obj_cols.length; col += $obj_headers.length) {
                    $obj_tempRow.append($($obj_cols[col]).clone().removeAttr("width"));
                }
            }

            $obj_newTableHolder.append($obj_newTable);
            if (typeof obj_verticalTables == "undefined") {
                window.obj_verticalTables = [];
            }
            obj_verticalTables[$obj_table.attr('id')] = {horizontalTable:$obj_table, verticalTable:$obj_newTable};

            $obj_table.remove();
        }
    }
    else {
        if (bol_vertical) {
            if ($obj_table.attr("data-tableType") != "vertical") {
                $obj_newTableHolder.append(obj_verticalTables[$obj_table.attr('id')].verticalTable);
                $obj_table.after($obj_newTableHolder);
                $obj_table.remove();
            }
        }
        else {
            if ($obj_table.attr("data-tableType") != "horizontal") {
                $obj_table.after(obj_verticalTables[$obj_table.attr('id')].horizontalTable);
                $obj_table.remove();
            }
        }
    }

    if (fcn_callback != null) {
        fcn_callback();
    }
}

function windowAToolsResize() {
    obj_globalOptions.obj_bools.bol_windowResizeFired = false;
    var str_lastWindowDevice = obj_globalOptions.obj_strings.str_deviceDim;

    /* --START--
     Update Media Queries */
    updateMediaQuery(Math.max(document.documentElement.clientWidth, window.innerWidth || 0));

    /* --START--
     Manage Window Resize (se mudar de tamanho do dispositivo apenas) */
    if (typeof windowResize == "function" && str_lastWindowDevice != obj_globalOptions.obj_strings.str_deviceDim) {
        str_lastWindowDevice = obj_globalOptions.obj_strings.str_deviceDim;

        windowResize();
    }

    /* --END--
     Manage Window Resize */


    /* --START--
     Update Screen Size Text */

    if ($('body > #screenSize').length) {
        $('body > #screenSize').text("window: " + Math.max(document.documentElement.clientWidth, window.innerWidth || 0) + " / type: " + obj_globalOptions.obj_strings.str_deviceDim)
    }

}

