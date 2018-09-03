$(function(){
  $(".main-nav__nav .dropdown-toggle").on("click", function(){
    var currentTarget = $(this);
    var parent = $(this).parent();
    var parentSiblings = parent.siblings();
    if(!parent.hasClass("open")) {
     
      parentSiblings.removeClass("open");
      parent.addClass("open");
    } else {
      parent.removeClass("open");
    }
    //console.log(parent);
    //console.log(parent.hasClass("open"));
  });
});