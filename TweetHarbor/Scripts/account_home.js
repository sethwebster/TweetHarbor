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
                if (res.Success) {
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
                if (res.Success) {
                }
                else {
                    $("#toggle_status_message").text(res.Error);
                }
            },
            "json");
    });


    // Templte Editing
    $(".template_edit_button").click(function () {

        $("#templates_" + $(this).attr("rel")).toggle("fast");
    });

    $(".template_display").click(function () {
        $(this).hide().next(".template_edit").show();
    });


    $(".template_field_insert").click(function () {
        $(this).parent().next("textarea").insertAtCaret($(this).attr("rel"));
    });


    $(".template_save_button").click(function () {
        //POST DATA BACK
        var _this = $(this);
        $.post("/Projects/UpdateMessageTemplate/",
        { id: $(this).attr("project"), TemplateType: $(this).attr("templateType"), Value: $(this).parent().next("textarea").val() },
        function (res) {
            if (res.Success) {
                $("#template_edit_" + _this.attr("project").toString().replace(" ", "_") + "_" + _this.attr("templateType")).hide();
                $("#template_display_" + _this.attr("project").toString().replace(" ", "_") + "_" + _this.attr("templateType")).text(_this.parent().next("textarea").val()).show();
            }
            else {
                // error
            }
        }, "json");
    });

    // Recipient Editing
    $(".recipient_edit_button").click(function () {

        $("#recipients_" + $(this).attr("rel")).toggle("fast");
    });

    $(".recipient_add_button").click(function () {
        var proj = $(this).attr("project");
        var val = $(this).prev("input").val();
        var _this = $(this);
        if ($.trim(val).length == 0) {
            _this.prevAll(".recipients_status_message").first().text("Please enter a Twitter screen name");
        }
        $.post("/Projects/AddMessageRecipient",
        { id: proj, value: val },
        function (res) {
            if (res.Success) {
                _this.prevAll(".recipients_status_message").first().text("");
                _this.prev("input").val("");
                var list = _this.nextAll(".recipient_list").first();
                if (null != list && list.attr("rel") == "empty") {
                    list.text("");
                    list.attr("rel", "");
                }
                _this.nextAll(".recipient_list").first().append("<li><a class='recipient_remove_button' project='" + proj + "' recipient='" + (val.toString().replace("@", "")) + "'>x</a>" + val + "</li>");
                bindRemoveHandler();
            }
            else {
                //display error
                _this.prevAll(".recipients_status_message").first().hide().text(res.Error).fadeIn("fast");
            }
        },
        "json");
    });

    function bindRemoveHandler() {
        $(".recipient_remove_button").unbind("click");
        $(".recipient_remove_button").click(function () {
            var proj = $(this).attr("project");
            var val = $(this).attr("recipient");
            var _this = $(this);
            $.post("/Projects/RemoveMessageRecipient",
        { id: proj, recipient: val },
        function (res) {
            if (res.Success) {
                var el = _this.parent().parent();
                _this.parent().remove();
                if (el.children().length == 0) {
                    el.hide().html("<li><em>No message recipients specified - <strong>No direct messages will be sent</strong></em></li>").fadeIn();
                    el.attr('rel', 'empty');
                }
            }
            else {
                //display error
                _this.prevAll(".recipients_status_message").first().hide().text(res.Error).fadeIn("fast");
            }
        },
        "json");
        });
    }
    bindRemoveHandler();


    // Notifications
    $(".show_notifications_button").click(function () {
        $("#notifications_" + $(this).attr("rel")).toggle("fast");
    });


});

jQuery.fn.extend({
    insertAtCaret: function (myValue) {
        return this.each(function (i) {
            if (document.selection) {
                this.focus();
                sel = document.selection.createRange();
                sel.text = myValue;
                this.focus();
            }
            else if (this.selectionStart || this.selectionStart == '0') {
                var startPos = this.selectionStart;
                var endPos = this.selectionEnd;
                var scrollTop = this.scrollTop;
                this.value = this.value.substring(0, startPos) + myValue + this.value.substring(endPos, this.value.length);
                this.focus();
                this.selectionStart = startPos + myValue.length;
                this.selectionEnd = startPos + myValue.length;
                this.scrollTop = scrollTop;
            } else {
                this.value += myValue;
                this.focus();
            }
        })
    }
});