$(function() {
	TryCleanCookieOfMessage();
});


function TryCleanCookieOfMessage() {
	if ($("#MessageToShow").length > 0) {
		$.cookie("MessageText", null, { path: '/' });
		$.cookie("MessageType", null, { path: '/' });
		$.cookie("MessageRoute", null, { path: '/' });
	}
}