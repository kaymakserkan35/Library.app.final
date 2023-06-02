var connection = new signalR.HubConnectionBuilder().withUrl("/commentHub").build();
connection
    .start()
    .then(function () { })
    .catch(function (err) {
        return console.error(err.toString());
    });

console.log("script içindeyim");
connection.on("loginmethod", function (msg) {
    console.log("method içindeyim");
    console.log(msg);
    var span = document.createElement("span");
    span.textContent = email;
    document.getElementById("messages").innerHTML += msg;
});






