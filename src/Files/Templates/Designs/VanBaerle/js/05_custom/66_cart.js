$(function(){
	$('body').on("click",'[ref="cart-plus"]', function(){
		var link = $(this).attr("data-update");
		var value = parseFloat($(this).parent().find(".orderline-quantity").html());
		var stock = parseFloat($(this).attr("data-limit-stock"));


		value++;
		link = link + value;
		if (value > stock) {
			alert("Not enough stock");
		} else {
			$(this).parent().find(".orderline-quantity").val(value);
			$.ajax({
				url: link,
				type: 'GET'
			})
			.done(function() {
				console.log("success");
				var currentLink = window.location.href;
				window.location.href = currentLink;
			})
			.fail(function() {
				console.log("error");
			});
		}
		
		
	});
	$('body').on("click",'[ref="cart-minus"]', function(){
		var link = $(this).attr("data-update");
		var value = parseFloat($(this).parent().find(".orderline-quantity").html());
		value--;
		link = link + value;
		if(value > 0 ) {
			$(this).parent().find(".orderline-quantity").val(value);
			$.ajax({
				url: link,
				type: 'GET'
			})
			.done(function() {
				// console.log("success");
				var currentLink = window.location.href;
				window.location.href = currentLink;
			})
			.fail(function() {
				console.log("error updating orderline");
			});
		}
		
		
	});
});