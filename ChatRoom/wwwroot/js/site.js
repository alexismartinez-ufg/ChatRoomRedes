$(document).ready(function () {
    var inputNombre = $("#nombre")
    var inputMessage = $("#messageInput")
    var btnGuardar = $('#guardarNombre');
    var btnEnviar = $('#sendButton');

    $("#guardarNombre").click(function () {
        var nombre = inputNombre.val();

        if (nombre != "" && nombre != " ") {
            User = nombre;
            connection.invoke("SetName", nombre);
            $("#exampleModalCenter").modal("hide");
        }
    });

    $('#sendButton').click(function () {
        var mensaje = inputMessage.val();
        inputMessage.val("");

        if (mensaje != "" && mensaje != " ") {
            connection.invoke("SendMessageToClient", User, mensaje);
        }
    });
    
    inputNombre.on('keydown', function (event) {
        if (event.keyCode === 13) {
            btnGuardar.click();
        }
    });   

    inputMessage.on('keydown', function (event) {
        if (event.keyCode === 13) {
            btnEnviar.click();
        }
    });    
});

var User = "";

var connection = new signalR.HubConnectionBuilder().withUrl("/chat").build();

connection.start().then(() => {
    connection.invoke("ContainsUser");
}).catch((e) => console.error(e));

connection.on("IsConnected", (conectado, nombre) => {

    if (conectado) {
        var mensaje = "<h1>Bienvenido al chat general " + nombre + "</h1>";

        $("#BienvenidaUser").html(mensaje);
        $('#ChatInfoContainers').toggleClass('d-none');

        connection.invoke("SendAllMessages");
    }
    else {
        $("#exampleModalCenter").modal("show");
    }
});

connection.on("SendMessageToClients", (mensaje, hora, usuario) => {

    var mensajeConFormato = getMensajeConFormato(usuario, mensaje, hora);

    var doScroll = isScrollAtBottom();

    $("#MessagesContainer").append(mensajeConFormato);    

    if (doScroll) {
        MoveToLastMessage();
    }
});

connection.on("LoadMessages", function (messagesJson) {
    var messages = JSON.parse(messagesJson);

    var doScroll = isScrollAtBottom();

    messages.forEach(function (message) {
        var mensajeConFormato = getMensajeConFormato(message.NombreRemitente, message.Contenido, message.FechaHora);
        $("#MessagesContainer").append(mensajeConFormato);
    });

    if (doScroll) {
        MoveToLastMessage();
    }    
});

connection.on("ReceiveLog", (mensaje, hora) => {
    var mensajeConFormato = getMensajeConFormato("Sistema", mensaje, hora);
    $("#LogsContainer").append(mensajeConFormato);
    MoveToLastLog();
});


function getMensajeConFormato(usuario, mensaje, hora) {

    return `<div class="message-container">
                                    <div class="mensaje">
                                        <div class="nombre-usuario ${usuario == User ? 'nombre-usuario-I' : 'nombre-usuario-other'}">${usuario}</div>
                                        <div class="contenido-mensaje">${mensaje}</div>
                                        <div class="hora-mensaje">${obtenerHoraLocal(hora)}</div>
                                    </div>
                                </div>`;
}

function obtenerHoraLocal(hora) {
    if (!hora.endsWith('Z')) {
        hora += 'Z';
    }

    const fechaHora = new Date(hora);

    return fechaHora.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit', second: '2-digit' });
}

function MoveToLastMessage() {
    const container = document.getElementById("MessagesContainer");
    const parent = container.parentNode;
    parent.scrollTop = parent.scrollHeight;

}

function MoveToLastLog() {
    const container = document.getElementById("LogsContainer");
    const parent = container.parentNode;
    parent.scrollTop = parent.scrollHeight;
}

function isScrollAtBottom() {
    const element = document.getElementById("MessagesContainer").parentNode;
    return element.scrollHeight - element.scrollTop === element.clientHeight;
}