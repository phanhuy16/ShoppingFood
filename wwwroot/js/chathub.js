"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/chathub").build();

//Disable the send button until connection is established.
document.getElementById("sendButton").disabled = true;

// Khi nhận được tin nhắn từ WebSocket (từ server)
connection.on("ReceiveMessage", function (user, message) {
    var li = document.createElement("li");
    li.classList.add("mb-2", "p-2", "rounded");

    // Thêm màu nền nếu là tin nhắn của bot
    if (user.toLowerCase() === "bot") {
        li.classList.add("bg-secondary", "text-white");
    } else {
        li.classList.add("bg-light");
    }

    li.textContent = `${user}: ${message}`;

    document.getElementById("messagesList").appendChild(li);
    scrollToBottom();
});

connection.start().then(function () {
    document.getElementById("sendButton").disabled = false;
}).catch(function (err) {
    return console.error(err.toString());
});

// Xử lý sự kiện gửi tin nhắn
document.getElementById("sendButton").addEventListener("click", function (event) {
    var user = document.getElementById("userInput").value.trim();
    var message = document.getElementById("messageInput").value.trim();

    if (user && message) {
        connection.invoke("SendMessage", user, message).catch(function (err) {
            return console.error(err.toString());
        });

        // Xóa nội dung ô nhập tin nhắn
        document.getElementById("messageInput").value = "";
    }
    event.preventDefault();
});

// Tự động cuộn xuống khi có tin nhắn mới
function scrollToBottom() {
    var chatBox = document.querySelector(".chat-box");
    chatBox.scrollTop = chatBox.scrollHeight;
}