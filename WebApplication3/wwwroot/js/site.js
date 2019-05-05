// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// #region Question
function getAddQuestionFormData() {
    var data = {};
    data.Options = [];
    data.Title = $("#Title").val();
    data.Score = $("#Score").val();
    var options = $("#item-table__body").children();
    console.log(options);
    for (i = 0; i < options.length; i++) {
        var o = {};
        var optionInput = $(options[i]).find("#Text");
        o.Text = optionInput.val();
        if (optionInput.attr("option-id") != undefined) {
            o.Id = optionInput.attr("option-id");
        }
        o.IsRight = $(options[i]).find("#isRight").prop("checked");
        o.Order = "" + i - -1;
        data.Options.push(o);
    }
    console.log(data);
    return data;
};

function addFormErrors(errors) {
    var ul = $("#validation-summary");
    for (var i = 0; i < errors.length; i++) {
        ul.append('<li>' + errors[i].errorMessage + '</li>');
    }
}

function submitQuestion(actionUrl) {
    $.ajax({
        type: "POST",
        url: actionUrl,
        beforeSend: function (xhr) {
            xhr.setRequestHeader("RequestVerificationToken",
                $('input:hidden[name="__RequestVerificationToken"]').val());
        },
        data: JSON.stringify(getAddQuestionFormData()),
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (response) {
            window.location.href = response;
        },
        error: function (xhr, textStatus, errorThrown) {
            //console.log(textStatus + ": Couldn't add control. " + errorThrown);
            var ul = $("#validation-summary");
            ul.empty();
            if (xhr.status == 500) {
                ul.append('<li>HTTP 500 Ошибка на стороне сервера</li>');
            }
            if (xhr.status == 404) {
                ul.append('<li>HTTP 404 Ресурс не найден</li>');
            }
            addFormErrors(xhr.responseJSON);

        },
    });
}
// #endregion

// #region Test
$("#addByIdForm").submit(function (e) {
    e.preventDefault();
    addById();
});
function addById() {

    var id = $("#testId").val();
    // TODO

    $.ajax({
        type: "POST",
        url: "/User/Tests/AddTestToUserAjax/",
        beforeSend: function (xhr) {
            xhr.setRequestHeader("RequestVerificationToken",
                $('input:hidden[name="__RequestVerificationToken"]').val());
        },
        contentType: "application/x-www-form-urlencoded; charset=utf-8",
        data: $("#addByIdForm").serialize(),
        success: function (response) {
            console.log(response);
            $("#modalSuccessText").text("Успешно!");
            $("#successModal").modal();
            location.reload();
        },
        error: function (data) {
            console.log(data);
            $("#modalErrorText").text(JSON.stringify(data.responseJSON));
            $("#errorModal").modal();
        },
    });

}
// #endregion

// #region Answer
function loadAnswer() {
    var type = getActiveAnswerType();
    var id = getActiveAnswerId();
    var actionUrl = "/" + type + "/" + id + "/";

    $.ajax({
        cache: false,
        method: "GET",
        url: actionUrl,
        dataType: "html",
        beforeSend: function (){

        },
        success: function (response) {
            //заменить html код формой внутри div
            $("#formDiv").html(response);
            // change question # in url
            var stateUrl = window.location.pathname.split("/");
            stateUrl[stateUrl.length-1] = id;
            var stateObj = {id:id,type:type,actionUrl:actionUrl};
            window.history.pushState(stateObj,"Вопрос " + id, stateUrl.join("/"));
        },
        error: function (xhr, textStatus, errorThrown) {
            console.log(xhr.responseJSON);
        }
    });
}

function getActiveAnswerId() {
    var activeID = $($("li").filter($(".active"))[0])[0].getAttribute("btn-id");
    return activeID;
}

function getActiveAnswerOrder() {
    var activeOrder = $($("li").filter($(".active"))[0])[0].getAttribute("btn-order");
    return activeOrder;
}

function getActiveAnswerType() {
    var activeType = $($("li").filter($(".active"))[0])[0].getAttribute("answer-type");
    return activeType;
}

function submitAnswer() {
    var id = getActiveAnswerId();
    var type = getActiveAnswerType();
    var actionUrl = "/" + type + "/" + id + "/";
    // выбрать url и собрать data
    if (type == "SingleChoiceAnswer") {
        var data = {};
        var option = $("input[name='option']:checked");
        data.OptionId = option.attr("option-id");
    }
    else if (type == "MultiChoiceAnswer") {
        var data = {};
        var checked=[];
        var options = $("input[name='option']:checked");
        for (i = 0; i < options.length; i++) {
            var o = {};
            o.OptionId = $(options[i]).attr("option-id");
            checked.push(o.OptionId)
        }
        data.checkedOptionIds = checked;
    }
    else if (type == "TextAnswer") {
        var data = {};
        data.Text = $("textarea[name='option']").val();
    }
    else if (type == "DragAndDropAnswer") {
        var data = {};
        data.Options = [];
        var options = $("label[name='option']");
        for (i = 0; i < options.length; i++) {
            var o = {};
            o.OptionId = $(options[i]).attr("option-id");
            o.ChosenOrder = "" + i - -1;
            data.Options.push(o);
        }
    }
    else if (type == "CodeAnswer") {
        var data = {};
        data.Code = {};
        data.Code.Value = editor.getValue();
        data.Code.Args = elements.args.val();
        data.Code.Output = $("#output").html();
    }
    else throw new exception("Not valid answer type!s");

    console.log(data);
    // запрос
    $.ajax({
        method: "POST",
        url: actionUrl,
        beforeSend: function (xhr) {
            $("#formDiv").html("<img src=\"/images/honkler.gif\" class=\"img-responsive center\"/>");
            xhr.setRequestHeader("RequestVerificationToken",
                $('input:hidden[name="__RequestVerificationToken"]').val());
        },
        contentType: "application/json",
        data: JSON.stringify(data),
        success: function (response) {
            console.log(response);
        },
        error: function (xhr, textStatus, errorThrown) {
            //console.log(textStatus + ": Couldn't add control. " + errorThrown);
            console.log(xhr);
        },
    });
}
function switchAnswer(e) {
    if (e.target.parentElement.classList.contains("next-btn")) {
        if (!e.target.parentElement.classList.contains("disabled")) {
            var tmp = getActiveAnswerOrder() - -1;
            $(".active").removeClass("active");
            $("li[btn-order=" + tmp + "]").addClass("active");
            refreshButtons()
        }
    }
    else
        if (e.target.parentElement.classList.contains("prev-btn")) {
            if (!e.target.parentElement.classList.contains("disabled")) {
                var tmp = getActiveAnswerOrder() - 1;
                $(".active").removeClass("active");
                $("li[btn-order=" + tmp + "]").addClass("active");
                refreshButtons();
            }
        }
        else
            if (e.target.parentElement.classList.contains("num-btn")) {
                $(".active").removeClass("active");
                $("li[btn-Order=" + e.target.parentElement.getAttribute("btn-Order") + "]").addClass("active");
                refreshButtons();
            }

}
function submitSwitchAnswer(e) {
    if (e.target.parentElement.classList.contains("next-btn")) {
        if (!e.target.parentElement.classList.contains("disabled")) {
            submitAnswer();
            var tmp = getActiveAnswerOrder() - -1;
            $(".active").removeClass("active");
            $("li[btn-order=" + tmp + "]").addClass("active");
            refreshButtons()
        }
    }
    else
        if (e.target.parentElement.classList.contains("prev-btn")) {
            if (!e.target.parentElement.classList.contains("disabled")) {
                submitAnswer();
                var tmp = getActiveAnswerOrder() - 1;
                $(".active").removeClass("active");
                $("li[btn-order=" + tmp + "]").addClass("active");
                refreshButtons();
            }
        }
        else
            if (e.target.parentElement.classList.contains("num-btn")) {
                submitAnswer();
                $(".active").removeClass("active");
                $("li[btn-Order=" + e.target.parentElement.getAttribute("btn-Order") + "]").addClass("active");
                refreshButtons();
            }
}
function refreshButtons() {
    var activeOrder = getActiveAnswerOrder();
    if (activeOrder == firstOrder)
        $(".prev-btn").addClass("disabled");
    else
        $(".prev-btn").removeClass("disabled");
    if (activeOrder == lastOrder)
        $(".next-btn").addClass("disabled");
    else
        $(".next-btn").removeClass("disabled");
}
var disableArrowKeys = function (e) {
    if ($("input[type=radio]").is(':focus')) {
        var arrowKeys = [37, 39];
        if (arrowKeys.indexOf(e.which) !== -1) {
            $(this).blur();
            return false;
        }
    }
}
// #endregion

// #region Code
function submitCode(actionUrl) {
    var data = {};
    data.Value = editor.getValue();
    data.Args = elements.args.val();
    console.log(data);
    // запрос
    $.ajax({
        async: false,
        method: "POST",
        url: actionUrl,
        beforeSend: function (xhr) {
            $("#output").html("Waiting for server...");
            xhr.setRequestHeader("RequestVerificationToken",
                $('input:hidden[name="__RequestVerificationToken"]').val());
        },
        contentType: "application/json",
        data: JSON.stringify(data),
        success: function (response) {
            console.log(response);
        },
        error: function (xhr, textStatus, errorThrown) {
            //console.log(textStatus + ": Couldn't add control. " + errorThrown);
            console.log(xhr);
        },
    });
}
function loadOutput(actionUrl) {
    $.ajax({
        cache: false,
        method: "GET",
        url: actionUrl,
        dataType: "html",
        beforeSend: function () {

        },
        success: function (response) {
            //заменить html код формой внутри div
            $("#outputDiv").html(response);
        },
        error: function (xhr, textStatus, errorThrown) {
            console.log(xhr.responseJSON);
        }
    });
}
function getAddCodeQuestionFormData() {
    var data = {};
    data.Code = {};
    data.Title = $("#Title").val();
    data.Score = $("#Score").val();
    data.Code.Value = editor.getValue();
    data.Code.Args = elements.args.val();
    data.Code.Output = $("#output").html();
    return data;
};
function submitCodeQuestion(actionUrl) {
    $.ajax({
        type: "POST",
        url: actionUrl,
        beforeSend: function (xhr) {
            xhr.setRequestHeader("RequestVerificationToken",
                $('input:hidden[name="__RequestVerificationToken"]').val());
        },
        data: JSON.stringify(getAddCodeQuestionFormData()),
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (response) {
            window.location.href = response;
        },
        error: function (xhr, textStatus, errorThrown) {
            //console.log(textStatus + ": Couldn't add control. " + errorThrown);
            var ul = $("#validation-summary");
            ul.empty();
            if (xhr.status == 500) {
                ul.append('<li>HTTP 500 Ошибка на стороне сервера</li>');
            }
            if (xhr.status == 404) {
                ul.append('<li>HTTP 404 Ресурс не найден</li>');
            }
            addFormErrors(xhr.responseJSON);

        },
    });
}

function truncateString(str, maxlength, sym) {
    if (str.length > maxlength) {
        return str.slice(0, maxlength - 3) + sym;
    }
    return str;
}
// #endregion