// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function getAddQuestionFormData() {
    var data = {};
    data.Options = [];
    data.Title = $("#Title").val();
    data.Score = $("#Score").val();
    var options = $("#itemTableBody").children();
    console.log(options);
    for (i = 0; i < options.length; i++) {
        var o = {};
        var optionInput = $(options[i]).find("#Text");
        o.Text = optionInput.val();
        if (optionInput.attr("option-id") != undefined){
            o.Id = optionInput.attr("option-id"); 
        }
        o.IsRight = $(options[i]).find("#isRight").prop("checked");
        data.Options.push(o);
    }
    console.log(data);
    return data;
};

function addFormErrors(errors) {
    var form = $("form");
    var ul = form.find("ul");
    for (var i =0;i<errors.length;i++){
        ul.append('<li>'+errors[i].errorMessage+'</li>');
    }
}

function submitQuestion(actionUrl){
    $.ajax({
        type: "POST",
        url: actionUrl,
        beforeSend: function(xhr) {
            xhr.setRequestHeader("RequestVerificationToken",
                $('input:hidden[name="__RequestVerificationToken"]').val());
        },
        data: JSON.stringify(getAddQuestionFormData()),
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function(response) {
            window.location.href = response;
        },
        error: function(xhr, textStatus, errorThrown) {
            //console.log(textStatus + ": Couldn't add control. " + errorThrown);
            var ul = form.find("ul");
            if (xhr.status == 500){
                ul.append('<li>HTTP 500 Ошибка на стороне сервера</li>');
            }
            if (xhr.status == 404){
                ul.append('<li>HTTP 404 Ресурс не найден</li>');
            }
            addFormErrors(xhr.responseJSON);

        },
    });
}

$("#addByIdForm").submit(function(e) {
    e.preventDefault();
    addById();
});
function addById() {

    var id = $("#testId").val();
    // TODO     

    $.ajax({
        type: "POST",
        url: "/User/Tests/AddTestToUserAjax/",
        beforeSend: function(xhr) {
            xhr.setRequestHeader("RequestVerificationToken",
                $('input:hidden[name="__RequestVerificationToken"]').val());
        },
        contentType: "application/x-www-form-urlencoded; charset=utf-8",
        data: $("#addByIdForm").serialize(),
        success: function(response) {
            console.log(response);
            $("#modalSuccessText").text("Успешно!");
            $("#successModal").modal();
            location.reload();
        },
        error: function(data) {
            console.log(data);
            $("#modalErrorText").text(JSON.stringify(data.responseJSON));
            $("#errorModal").modal();
        },
    });

}

// Answer

function loadAnswer() {
    var type = getActiveAnswerType();
    var id = getActiveAnswerId();
    var actionUrl = "/"+type+"/"+id+"/";

    $.ajax({
        cache: false,
        method: "GET",
        url: actionUrl,
        dataType: "html",

        success: function(response) {
            //заменить html код формой внутри div
            $("#formDiv").html(response);
        },
        error: function(xhr, textStatus, errorThrown) {
            console.log(xhr.responseJSON);
        }
    });
}

function getActiveAnswerId() {
    var activeID = $($("li").filter($(".active"))[0])[0].getAttribute("btn-id");
    return activeID;
}

function getActiveAnswerType() {
    var activeType = $($("li").filter($(".active"))[0])[0].getAttribute("answer-type");
    return activeType;
}

function getActiveAnswerOrder() {
    return $("#" + getActiveAnswerId()).children()[0].text;
}

function submitAnswer() {
    var id = getActiveAnswerId();
    var type = getActiveAnswerType();
    var actionUrl = "/"+type+"/"+id+"/";
    // выбрать url и собрать data
    if (type == "SingleChoiceAnswer") {
        var data = {};
        var option = $("input[name='option']:checked");
        data.OptionId = option.attr("option-id");

    }
    else if (type == "MultiChoiceAnswer") {

    }
    else if (type == "TextAnswer") {

    }
    else if (type == "DragAndDropAnswer") {

    } else throw new exception("Not valid answer type!s");

    console.log(data);
    // запрос
    $.ajax({
        method: "POST",
        url: actionUrl,
        beforeSend: function (xhr) {
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