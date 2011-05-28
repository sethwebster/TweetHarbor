$(document).ready(function () {
    // Email Edit Code
    $("#editEmailLink").click(function () {
        $("#email_address_display").toggle();
        $("#email_address_edit").toggle();
        $("#saveEmailLink").click(function () {
            $.post("/Account/UpdateEmail",
                { EmailAddress: $("#email_address_text").val() },
                function (res) {
                    if (res.Success) {
                        $("#email_address_display_text").text($("#email_address_text").val());
                        $("#email_address_display").toggle();
                        $("#email_address_edit").toggle();
                    }
                    else {
                        $("#email_status_message").text(res.Error);
                    }
                }, "json");
        });
    });
    // Global Pub/Priv Tweets
    $(".global_notification_toggle").click(function () { 
        var e = $(this);
        $.post("/Account/UpdateTweetToggle/",
            { TweetType: e.attr("id"), Value: e.is(":checked") },
            function (res) {
                if (res.success) {
                }
                else {
                    $("#global_toggle_status_message").text(res.Error);
                }
            },
            "json");
    });

    // App-Level Tweets
    $(".notification_toggle").click(function () {
        var e = $(this);
        $.post("/Projects/UpdateNotificationToggle/",
            { id: e.attr("rel"), Value: e.is(":checked"), TweetType: e.attr("nt") },
            function (res) {
                if (res.success) {
                }
                else {
                    $("#toggle_status_message").text(res.Error);
                }
            },
            "json");
    });

    $(".template_edit_button").click(function () {

        $("#templates_" + $(this).attr("rel")).toggle("fast");
    });
});