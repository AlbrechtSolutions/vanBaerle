$(function(){
	$('body').on("click",'[ref="plus"]', function(){
		var link = $(this).attr("data-update");
		var value = parseFloat($(this).parent().find(".orderline-quantity").html());
		value++;
		link = link + value;
		$(this).parent().find(".orderline-quantity").val(value);
		$.ajax({
			url: link,
			type: 'GET'
		})
		.done(function() {
			console.log("success");
			minicart();
		})
		.fail(function() {
			console.log("error");
		});
		
	});
	$('body').on("click",'[ref="minus"]', function(){
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
				minicart();
			})
			.fail(function() {
				console.log("error updating orderline");
			});
		}
		
		
	});
});