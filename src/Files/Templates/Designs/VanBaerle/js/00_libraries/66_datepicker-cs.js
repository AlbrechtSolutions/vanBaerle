/* Czech initialisation for the jQuery UI date picker plugin. */
/* Written by Tomas Muller (tomas@tomas-muller.net). */
( function( factory ) {
	if ( typeof define === "function" && define.amd ) {

		// AMD. Register as an anonymous module.
		define( [ "../widgets/datepicker" ], factory );
	} else {

		// Browser globals
		factory( jQuery.datepicker );
	}
}( function( datepicker ) {

datepicker.regional.cs = {
	closeText: "Zavřít",
	prevText: "&#x3C;Dříve",
	nextText: "Později&#x3E;",
	currentText: "Nyní",
	monthNames: [ "Leden","Únor","Březen","Duben","Květen","Červen",
	"Červenec","Srpen","Září","Říjen","Listopad","Prosinec" ],
	monthNamesShort: [ "Led","Úno","Bře","Dub","Kvě","Čer",
	"Čvc","Srp","Zář","Říj","Lis","Pro" ],
	dayNames: [ "Neděle", "Pondělí", "Úterý", "Středa", "Čtvrtek", "Pátek", "Sobota" ],
	dayNamesShort: [ "Ne", "Po", "Út", "St", "Čt", "Pá", "So" ],
	dayNamesMin: [ "Ne","Po","Út","St","Čt","Pá","So" ],
	weekHeader: "Týd",
	dateFormat: "dd.mm.yy",
	firstDay: 1,
	isRTL: false,
	showMonthAfterYear: false,
	yearSuffix: "" };
datepicker.setDefaults( datepicker.regional.cs );

return datepicker.regional.cs;

} ) );