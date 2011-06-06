var ProjectsManager = function (targetContainerId, labelsBefore) {
    this._targetUrl = "/Projects/UserProjects";
    this._containerClass = "projects_list";
    this._targetContainerId = targetContainerId;
    this._labels = Array();
    this._labels["SendPrivateTweetOnSuccess"] = "DM On Success";
    this._labels["SendPublicTweetOnSuccess"] = "Tweet Success";
    this._labels["SendPrivateTweetOnFailure"] = "DM Failure";
    this._labels["SendPublicTweetOnFailure"] = "Tweet Failure";
    this._labels["SendTextOnSuccess"] = "SMS Success";
    this._labels["SendTextOnFailure"] = "SMS Failure";
    if (labelsBefore) {
        this._labelsBefore = true;
    }
    else {
        this._labelsBefore = false;
    }
    this._buffer = "";
}

ProjectsManager.prototype.Load = function (callback) {
    var _this = this;
    $.getJSON(this._targetUrl,
        function (res) {
            _this._projects = res;
            _this.RenderProjects();
            callback(this);
        });
}

ProjectsManager.prototype.appendBuffer = function (s) {
    this._buffer += s;
}

ProjectsManager.prototype.RenderProjects = function () {
    $("#" + this._targetContainerId).html("<ul class='" + this._containerClass + "'></ul>");
    this._container = $("#" + this._targetContainerId);
    this._list = this._container.children().first();
    for (var i = 0; i < this._projects.length; i++) {
        this.RenderProject(this._projects[i]);
    }

    this._list.append(this._buffer);

    $(':checkbox').iphoneStyle();

    this.bindTemplateDisplay();
    this.bindTemplateSaveButton();
    this.bindSwitches();
    this.bindRecipientAddButton();
    this.bindRemoveHandler();
    this.bindTemplateInsertButton();

}

ProjectsManager.prototype.RenderProject = function (project) {

    var status = project.ProjectNotifications.length > 0 && project.ProjectNotifications[0].Build.status == "succeeded" ? "build_succeeded" : "build_failed";
    this.appendBuffer("<li class='list_project'>");
    this.appendBuffer("<h3 class='" + status + "'>" + project.ProjectName + "</h3>");
    this.RenderSwitch("SendPrivateTweetOnSuccess", "ProjectNotificationToggle", project.ProjectName, "SendPrivateTweetOnSuccess", project.SendPrivateTweetOnSuccess);
    this.RenderSwitch("SendPublicTweetOnSuccess", "ProjectNotificationToggle", project.ProjectName, "SendPublicTweetOnSuccess", project.SendPublicTweetOnSuccess);
    this.RenderSwitch("SendPrivateTweetOnFailure", "ProjectNotificationToggle", project.ProjectName, "SendPrivateTweetOnFailure", project.SendPrivateTweetOnFailure);
    this.RenderSwitch("SendPublicTweetOnFailure", "ProjectNotificationToggle", project.ProjectName, "SendPublicTweetOnFailure", project.SendPublicTweetOnFailure);
    this.RenderSwitch("SendTextOnSuccess", "ProjectNotificationToggle", project.ProjectName, "SendTextOnSuccess", project.SendTextOnSuccess);
    this.RenderSwitch("SendTextOnFailure", "ProjectNotificationToggle", project.ProjectName, "SendTextOnFailure", project.SendTextOnFailure);
    this.appendBuffer("<div class='clear'></div><h5>Templates</h5>");
    this.RenderTemplateEditor("SuccessTemplate", project.ProjectName, project.SuccessTemplate);
    this.RenderTemplateEditor("FailureTemplate", project.ProjectName, project.FailureTemplate);
    this.appendBuffer("<div class='clear'></div><h5>Message Recipients</h5>");
    this.appendBuffer("<div class='messagageRecipientsWrapper'>");
    this.RenderMessageRecipients("MessageRecipients", project.ProjectName, project.MessageRecipients);
    this.RenderMessageRecipients("TextMessageRecipients", project.ProjectName, project.TextMessageRecipients);
    this.appendBuffer("</div>");
    //TODO: Project Notifications
    this.appendBuffer("</li>");
}

ProjectsManager.prototype.RenderMessageRecipients = function (type, projectName, recipients) {
    var statusDivClass = "recipients_status_message";
    var fillerText = type == "MessageRecipients" ? "Direct" : "SMS";
    var title = type == "MessageRecipients" ? "Twitter DM Recipients" : "SMS Text Recipients";
    this.appendBuffer("<div class='recipients'><h6>" + title + "</h6>");
    this.appendBuffer("<div class='error " + statusDivClass + "'></div>");
    this.appendBuffer("<input type='text' /><a href='javascript:void(0);' class='recipient_add_button' project='" + projectName + "' type='" + type + "'>Add</a><div class='clear'></div>");
    this.appendBuffer("<ul class='recipient_list'>");
    if (recipients.length > 0) {
        for (var r in recipients) {
            this.RenderMessageRecipient(type, projectName, recipients[r]);
        }
    }
    else {
        this.appendBuffer("<li class='empty'><em>No " + fillerText + " recipients specified. <strong>No " + fillerText + " messages will be sent</strong></em></li>");
    }
    this.appendBuffer("</ul>");
    this.appendBuffer("</div>");
}

ProjectsManager.prototype.RenderMessageRecipient = function (type, projectName, recipient) {
    var idx = type == "MessageRecipients" ? "ScreenName" : "PhoneNumber";
    this.appendBuffer("<li class='recipient'>");
    this.appendBuffer("<a class='recipient_remove_button' project='" + projectName + "' recipient='" + recipient[idx] + "' recipientType='" + type + "'>x</a>");
    this.appendBuffer(recipient[idx]);
    this.appendBuffer("</li>");
}

ProjectsManager.prototype.RenderSwitch = function (type, toggleType, projectName, notification, checked) {
    var label = this._labels[type];
    var id = projectName + "_" + toggleType + "_" + notification;
    var label = "<label id='" + id + "_label' for='" + id + "'>" + label + "</label>";
    this.appendBuffer("<div class='switch'>");
    if (this._labelsBefore)
        this.appendBuffer(label);
    this.appendBuffer("<input type='checkbox' id='" + id + "' class='notification_toggle' project='" + projectName + "' toggleType='" + toggleType + "' notification='" + notification + "' " + (checked ? "checked" : "") + "/>");
    if (!this._labelsBefore) {
        this.appendBuffer(label);
    }
    this.appendBuffer("</div>");

}

ProjectsManager.prototype.RenderTemplateEditor = function (type, project, currentValue) {
    this.appendBuffer("<div class='template'>");
    var projectId = project.replace(/ /g, "_");
    if (currentValue == null || currentValue.length <= 0) {
        switch (type) {
            case "SuccessTemplate":
                currentValue = "We just deployed another build of {application:name}  via @AppHarbor!";
                break;
            case "FailureTemplate":
                currentValue = "{application:name} build failed: {build:commit:message}{build:commit:id}";
                break;
        }
    }
    var s = "";
    s += ("<h4>" + (type == "SuccessTemplate" ? "Success Template" : "Failure Template") + "</h4>");
    s += ("<div class='template_display' id='template_display_" + projectId + "_" + type + "'>" + currentValue + "</div>");
    s += ("<div class='template_edit' id='template_edit_" + projectId + "_" + type + "'>");
    s += ("<div class='template_edit_toolbar'>");
    s += ("<a href='javascript:void(0);' title='Save Template' class='template_save_button' project='" + project + "' templateType='" + type + "'>");
    s += ("<img src='/Content/disc.png' height='25' />");
    s += ("</a>");
    s += ("<a class='template_field_insert'>{application:name}</a>");
    s += ("<a class='template_field_insert'>{build:commit:message}</a>");
    s += ("<a class='template_field_insert'>{build:commit:id}</a>");
    s += ("</div>");
    s += ("<textarea rows='5' cols='10' class='template_edit_field'>" + currentValue + "</textarea>");
    s += "</div>";
    this.appendBuffer(s);
    this.appendBuffer("</div>");
}




ProjectsManager.prototype.bindTemplateInsertButton = function () {
    $(".template_field_insert").unbind("click");
    $(".template_field_insert").click(function () {
        $(this).parent().next("textarea").insertAtCaret($(this).text());
    });

}

ProjectsManager.prototype.bindRemoveHandler = function () {
    $(".recipient_remove_button").unbind("click");
    $(".recipient_remove_button").click(function () {
        var proj = $(this).attr("project");
        var val = $(this).attr("recipient");
        var _this = $(this);
        var type = $(this).attr("recipientType");
        $.post("/Projects/RemoveMessageRecipient",
        { id: proj, recipient: val, Type: type },
        function (res) {
            if (res.Success) {
                var el = _this.parent().parent();
                _this.parent().remove();
                if (el.children().length == 0) {
                    if (type == "TextMessageRecipients") {
                        el.hide().html("<li class='empty'><em>No message recipients specified - <strong>No SMS messages will be sent</strong></em></li>").fadeIn();
                    }
                    else {
                        el.hide().html("<li class='empty'><em>No message recipients specified - <strong>No direct messages will be sent</strong></em></li>").fadeIn();
                    }
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

ProjectsManager.prototype.bindRecipientAddButton = function () {
    var _this = this;

    $(".recipient_add_button").unbind("click");
    $(".recipient_add_button").click(function () {
        var proj = $(this).attr("project");
        var val = $.trim($(this).prev("input").val());
        var type = $(this).attr("type");
        var _el = $(this);
        if (val.length == 0) {
            _el.prevAll(".recipients_status_message").first().text(
                type == "MessageRecipients" ?
                    "Please enter a twitter screen name" :
                    "Please enter a valid telephone number including area code"
            );
            return;
        }
        $.post("/Projects/AddMessageRecipient",
        { id: proj, value: val, type: type },
        function (res) {
            if (res.Success) {
                _el.prevAll(".recipients_status_message").first().text("");
                _el.prev("input").val("");
                var list = _el.nextAll(".recipient_list").first();
                if (null != list && list.children().length > 0 && list.children().first().attr('class') == 'empty') {
                    list.text("");
                    list.attr("rel", "");
                }
                _el.nextAll(".recipient_list").first().append("<li><a class='recipient_remove_button' project='" + proj + "' recipient='" + (val.toString().replace("@", "")) + "' recipientType='" + type + "'>x</a>" + val + "</li>");
                _this.bindRemoveHandler();
            }
            else {
                //display error
                _el.prevAll(".recipients_status_message").first().hide().text(res.Error).fadeIn("fast");
            }
        },
        "json");
    });
}

ProjectsManager.prototype.bindTemplateSaveButton = function () {
    $(".template_save_button").unbind("click");
    $(".template_save_button").click(function () {
        //POST DATA BACK
        var _this = $(this);
        $.post("/Projects/UpdateMessageTemplate/",
        { id: $(this).attr("project"), TemplateType: $(this).attr("templateType"), Value: $(this).parent().next("textarea").val() },
        function (res) {
            if (res.Success) {
                $("#template_edit_" + _this.attr("project").toString().replace(/ /g, "_") + "_" + _this.attr("templateType")).hide();
                $("#template_display_" + _this.attr("project").toString().replace(/ /g, "_") + "_" + _this.attr("templateType")).text(_this.parent().next("textarea").val()).show();
            }
            else {
                // error
            }
        }, "json");
    });
}

ProjectsManager.prototype.bindTemplateDisplay = function () {
    $(".template_display").unbind("click");
    $(".template_display").click(function () {
        $(this).hide().next(".template_edit").show();
    });
}

ProjectsManager.prototype.bindSwitches = function () {
    $('.iPhoneCheckContainer').unbind("click");
    $('.iPhoneCheckContainer').click(function () {
        var e = $(this).find('input:checkbox');
        var url = "";
        var data = {};
        var statusDivId = "";
        switch (e.attr("toggleType")) {
            case "GlobalNotificationToggle":
                url = "/Account/GlobalNotificationToggle";
                data = { TweetType: e.attr("notification"), Value: e.is(":checked") };
                statusDivId = "#global_toggle_status_message"
                break;
            case "ProjectNotificationToggle":
                url = "/Projects/ProjectNotificationToggle";
                data = { Id: e.attr("project"), TweetType: e.attr("notification"), Value: e.is(":checked") };
                statusDivId = "#toggle_status_message"
                break;
        }
        $.post(url,
            data,
            function (res) {
                if (res.Success) {
                }
                else {
                    $("#toggle_status_message").text(res.Error);
                }
            },
            "json");
    });
}


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

    // Load Projects
    this.ProjectManager = new ProjectsManager("projects", true);
    this.ProjectManager.Load(function () {
    });


});

