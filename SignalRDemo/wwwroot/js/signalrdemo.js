"use strict";
var registered = false;
var connection = new signalR.HubConnectionBuilder().withUrl("/demoHub").build();
document.getElementById("join-game").disabled = true;
document.getElementById("game-area").style.display = "None";
document.getElementById("start-area").style.display = "None";


connection.start().then(() => {
    document.getElementById("join-game").disabled = false;
}).catch((err) => {
    return console.error(err.toString());
});

document.getElementById("join-game").addEventListener("click", (event) => {
    var user = document.getElementById("user-name-controle").value;    
    connection.invoke("RegisterUser", user).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});

connection.on("GameNotStarted", () => {
    createMessage("Game has not started yet, start a new one!");
});

connection.on("Elements", (elements) => {
    createMessage("Possible elements are:")
    for (const element of elements) {
        createMessage(element);
    }
});

connection.on("Hint", (hints) => {
    createMessage("Hints entered by gamehost are:")
    for (const hint of hints) {
        createMessage(element);
    }
});

connection.on("UnknownUser", (connectionId) => {
    createMessage(`User with connectionId ${connectionId} is not known!`);
});

connection.on("ChangeColor", (elementId, color, user) => {
    createMessage(`User ${user.Name} changed your element ${elementId} to color ${color}!`);
    const element = document.getElementById(elementId);
    if (element != null) {
        element.style.color = color;
    }
});

connection.on("LogColor", (target, color, elementId, user) => {
    createMessage(`User ${user.Name} changed element ${elementId} of ${target.Name} to color ${color}!`);
});

connection.on("LogWon", (password, target, color, elementId, user) => {
    createMessage(`Team ${user.Team} WON!, User ${user.Name} changed the correct element ${elementId} of ${target.Name} to color ${color}.. password for connection Id was: ${password}!`);
    document.getElementById("start-new-game").style.display = "Inherited";
});

connection.on("Won", (password, target, color, elementId, user) => {
    createMessage(`Your team WON! congratulations, ${user.Name} changed the correct element ${elementId} of ${target.Name} to color ${color}.. password for connection Id was: ${password}!`);
    document.getElementById("start-new-game").style.display = "Inherited";
});

connection.on("Lost", (password, target, color, elementId, user) => {
    createMessage(`Your team LOST!, ${user.Name} of team ${user.Team} changed the correct element ${elementId} of ${target.Name} to color ${color}.. password for connection Id was: ${password}!`);
    document.getElementById("start-new-game").style.display = "Inherited";
});

connection.on("GameStarted", (gamehost) => {
    createMessage(`User ${gamehost} started a new game, good luck!`);
    if (registered) {
        document.getElementById("start-area").style.display = "None";
        document.getElementById("game-area").style.display = "Flex";
    }    
});

const createMessage = (message) => {
    const li = document.createElement("li");
    document.getElementById("message-list").appendChild(li);
    li.textContent = message;
}