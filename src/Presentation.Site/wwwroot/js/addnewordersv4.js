var bFinished = false;


function internalHandler(e) {
    if (!bFinished)
        e.preventDefault();
    e.returnValue = "¡Tiene órdenes por procesar! You have attempted to leave this page. Are you sure?";
    return "¡Tiene órdenes por procesar! You have attempted to leave this page. Are you sure?";
}


if (window.addEventListener) {
    window.addEventListener('beforeunload', internalHandler, true);
} else if (window.attachEvent) {
    window.attachEvent('onbeforeunload', internalHandler);
}


const queryString = window.location.search;
const urlParams = new URLSearchParams(queryString);
const storeId = urlParams.get('store');
var ordersId = urlParams.has('id[]') ? urlParams.getAll('id[]') : [urlParams.get('id')];
var orderLength = ordersId.length

var iter = 0;
var nError = 0;


function start_process() {
    $('#prgrss').attr("max", ordersId.length)
    $('#prgrss').attr("value", iter);
    $('#progressCounter').text("0 de " + orderLength);
    $('#output').html("");
}

iprocnx = setInterval(proc_next, 1000);


function end_process() {
    if (0 < nError) {
        if (nError == ordersId.length) {
            if (1 < nError)
                $('#prgrsshead').html("¡No pudimos registrar ninguna de las órdenes!");
            else
                $('#prgrsshead').html("¡No pudimos registrar su orden!");
        }
        else {
            if (1 < ordersId.length - nError)
                $('#prgrsshead').html("¡Registramos " + (ordersId.length - nError) + " órdenes con éxito!");
            else
                $('#prgrsshead').html("¡Registramos una orden con éxito!");
        }
        $('#prgrsstail').html("¡Tenés órdenes por revisar!");
        $('#buttonReview').removeAttr("hidden");
    }
    else {
        if (1 < ordersId.length)
            $('#prgrsshead').html("¡Ya registramos tus órdenes!");
        else if (1 == ordersId.length)
            $('#prgrsshead').html("¡Ya registramos tu orden!");
        else
            $('#prgrsshead').html("¡No encontramos ninguna orden para el Correo Argentino!");
        $('#prgrsstail').html("Ingresá a MiCorreo para confirmar tus envíos");
    }
    $('#prgrss').attr("hidden", true);
    $('#progressCounter').attr("hidden", true);
    $('#buttons').removeAttr("hidden");
    $('#buttonGoTo').removeAttr("hidden");
    $('#buttonBack').removeAttr("hidden");
}



function showResult(id) {
    $('#prgrsshead').attr("hidden", true);
    $('#prgrsstail').attr("hidden", true);
    $('#buttonReview').attr("hidden", true);
    $('#table').removeAttr("hidden");
}



function proc_next() {
    if (window.addEventListener) {
        window.addEventListener('beforeunload', internalHandler, true);
    } else if (window.attachEvent) {
        window.attachEvent('onbeforeunload', internalHandler);
    }

    if (iter == 0) {
        start_process();
    }

    $.ajax({
        type: "GET",
        url: "AddNewOrdersProcNext?store=" + storeId + "&id=" + ordersId[iter++],
        async: false,
        beforeSend: function () {
        },
        success: function (response) {
            if (response != null) {
                if (response.berror) {
                    nError++;
                    $('#output').append("<tr style=\"font-size:12px;\"><td>" + response.orderNumber + "</td><td>" + response.message + "</td></tr>");
                }
                //else  // to show successfully imported lines:
                //    $('#output').append("<tr style=\"font-size:12px;\"><td>" + response.orderNumber + "</td><td>" + response.message + "</td></tr>");

                $('#prgrss').attr("value", iter);
                $('#progressCounter').text(iter + " de " + orderLength);
            }

            if (ordersId.length <= iter) {
                clearInterval(iprocnx);
                end_process();
            }
        },
        complete: function () {
        },
        error: function () {
            end_process();
            var error = eval("(" + XMLHttpRequest.responseText + ")");
            aler(error.Message);
        }
    });
}
