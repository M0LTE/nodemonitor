"use strict";

function htmlEncode(s) {
    return s.replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/'/g, '&#39;')
        .replace(/"/g, '&#34;')
}

var connection = new signalR.HubConnectionBuilder()
    .withUrl("/nodeHub")
    .withAutomaticReconnect()
    .build();

connection.on("ReceiveMessage", function (message) {
    var li = document.createElement("li");
    var list = document.getElementById("messagesList")
    list.appendChild(li);

    if (list.childNodes.length > 1000) {
        list.removeChild(list.firstChild)
    }

    li.innerHTML = `<pre>${message.timestamp} ${message.modemId} ${htmlEncode(message.data)}</pre>`;
    if (message.direction == '>') {
        li.style.color = 'red'
    } else if (message.direction == '<') {
        if (message.data.includes(' > GB7RDG')) { // sorry
            li.style.color = 'green'
        } else {
            li.style.color = 'gray'
        }
    }

    $("html, body").animate({ scrollTop: $(document).height() }, 50);
});

connection.start().catch(function (err) {
    return console.error(err.toString());
});
