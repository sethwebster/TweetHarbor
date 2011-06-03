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


    // Templte Editing
    $(".template_edit_button").click(function () {

        $("#templates_" + $(this).attr("rel")).toggle("fast");
    });

    $(".template_field_insert").click(function () {
        $(this).parent().next("textarea").insertAtCaret($(this).text());
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
        { id: proj, value: val, type: "Twitter" },
        function (res) {
            if (res.Success) {
                _this.prevAll(".recipients_status_message").first().text("");
                _this.prev("input").val("");
                var list = _this.nextAll(".recipient_list").first();
                if (null != list && list.attr("rel") == "empty") {
                    list.text("");
                    list.attr("rel", "");
                }
                _this.nextAll(".recipient_list").first().append("<li><a class='recipient_remove_button' project='" + proj + "' recipient='" + (val.toString().replace("@", "")) + "' recipientType='Twitter'>x</a>" + val + "</li>");
                bindRemoveHandler();
            }
            else {
                //display error
                _this.prevAll(".recipients_status_message").first().hide().text(res.Error).fadeIn("fast");
            }
        },
        "json");
    });

    $(".text_recipient_add_button").click(function () {
        var proj = $(this).attr("project");
        var val = $(this).prev("input").val();
        var _this = $(this);
        if ($.trim(val).length == 0) {
            _this.prevAll(".text_recipients_status_message").first().text("Please enter phone number name");
        }
        $.post("/Projects/AddMessageRecipient",
        { id: proj, value: val, type: "SMS" },
        function (res) {
            if (res.Success) {
                _this.prevAll(".text_recipients_status_message").first().text("");
                _this.prev("input").val("");
                var list = _this.nextAll(".recipient_list").first();
                if (null != list && list.attr("rel") == "empty") {
                    list.text("");
                    list.attr("rel", "");
                }
                _this.nextAll(".recipient_list").first().append("<li><a class='recipient_remove_button' project='" + proj + "' recipient='" + (val.toString().replace("@", "")) + "' recipientType='SMS'>x</a>" + val + "</li>");
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
            var type = $(this).attr("recipientType");
            $.post("/Projects/RemoveMessageRecipient",
        { id: proj, recipient: val, Type: type },
        function (res) {
            if (res.Success) {
                var el = _this.parent().parent();
                _this.parent().remove();
                if (el.children().length == 0) {
                    if (type == "SMS") {
                        el.hide().html("<li><em>No message recipients specified - <strong>No SMS messages will be sent</strong></em></li>").fadeIn();
                    }
                    else {
                        el.hide().html("<li><em>No message recipients specified - <strong>No direct messages will be sent</strong></em></li>").fadeIn();
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
    bindRemoveHandler();


    // Notifications
    $(".show_notifications_button").click(function () {
        $("#notifications_" + $(this).attr("rel")).toggle("fast");
    });


    $(':checkbox').iphoneStyle();

    bindSwitches();
    bindTemplateDisplay();

});

function bindTemplateDisplay() {
    $(".template_display").click(function () {
        $(".template_display").unbind("click");
        $(this).hide().next(".template_edit").show();
    });
}

function bindSwitches() {
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

    // Load Projects
    this.ProjectManager = new ProjectsManager("projects", true);
    this.ProjectManager.Load();

});

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

ProjectsManager.prototype.Load = function () {
    var _this = this;
    $.getJSON(this._targetUrl,
        function (res) {
            _this._projects = res;
            _this.RenderProjects();
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
    bindTemplateDisplay();
    bindSwitches();
}

ProjectsManager.prototype.RenderProject = function (project) {

    this.appendBuffer("<li class='list_project'>");
    for (var k in project) {
        switch (k) {
            case "ProjectName":
                this.appendBuffer("<h5>" + project[k] + "</h5>");
                break;
            case "SendPrivateTweetOnSuccess":
            case "SendPublicTweetOnSuccess":
            case "SendPrivateTweetOnFailure":
            case "SendPublicTweetOnFailure":
            case "SendTextOnSuccess":
            case "SendTextOnFailure":
                this.RenderSwitch(k, "ProjectNotificationToggle", project.ProjectName, k, project[k]);
                break;
            case "SuccessTemplate":
            case "FailureTemplate":
                this.RenderTemplateEditor(k, project.ProjectName, project[k]);
                break;
            default:
                this.appendBuffer("<div class='display_" + k + "'>" + k + " " + project[k] + "</div>");
                break;
        }
    }
    this.appendBuffer("</li>");
}

ProjectsManager.prototype.RenderSwitch = function (type, toggleType, projectName, notification, checked) {
    var label = this._labels[type];
    var id = projectName + "_" + toggleType + "_" + notification;
    var label = "<label id='" + id + "_label' for='" + id + "'>" + label + "</label>";
    var s = "";
    if (this._labelsBefore)
        s += label;
    s += "<input type='checkbox' id='" + id + "' class='notification_toggle' project='" + projectName + "' toggleType='" + toggleType + "' notification='" + notification + "' " + (checked ? "checked" : "") + "/>";
    if (!this._labelsBefore) {
        s += label;
    }

    this.appendBuffer(s);
}

/*
*<div class='template_display' id="template_display_@(Model.Project.ProjectName.Replace(" ","_"))_@(Model.TemplateEditorType)">@Html.Raw(currentTemplateText)</div>
<div class='template_edit' id="template_edit_@(Model.Project.ProjectName.Replace(" ","_"))_@(Model.TemplateEditorType)">
<div class='template_edit_toolbar'>
<a href="javascript:void(0);" title="Save Template" class='template_save_button' project="@Model.Project.ProjectName" templateType="@(Model.TemplateEditorType)">
<img src="@Url.Content("~/Content/disc.png")" height="25"  /></a> <a href="javascript:void(0);"
rel="{application.name}" class='template_field_insert'>{application:name}</a>
<a href="javascript:void(0);" rel="{build:commit:message}" class='template_field_insert'>
{build:commit:message}</a> <a href="javascript:void(0);" rel="{build:commit:id}"
class='template_field_insert'>{build:commit:id}</a>
</div>
<textarea rows="5" cols="10" class='template_edit_field'>@currentTemplateText</textarea>
</div>
*/

ProjectsManager.prototype.RenderTemplateEditor = function (type, project, currentValue) {
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
}


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