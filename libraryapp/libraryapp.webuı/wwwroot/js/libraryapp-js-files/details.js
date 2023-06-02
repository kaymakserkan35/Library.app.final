var connection = new signalR.HubConnectionBuilder().withUrl("/commentHub").build();
connection
    .start()
    .then(function () { document.getElementById("sendButton").disabled = false; })
    .catch(function (err) {
        return console.error(err.toString());
    });

connection.on("ReceiveComment", function (username) {
    console.log("username");
    console.log("receive method is runned");
    var li = document.createElement("li");
    li.textContent = username;
    document.getElementById("messagesList").appendChild(li);
});

document.getElementById("sendButton").addEventListener("click", function (event) {
    console.log("clikced send button");
    var comment = document.getElementById("messageInput").value.toString();
    connection.invoke("SendComment", "1", comment).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});

