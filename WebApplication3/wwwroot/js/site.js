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
        o.Text = $(options[i]).find("#Text").val();
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