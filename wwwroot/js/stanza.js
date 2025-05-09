"use strict";

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/stanzaHub")
    .configureLogging(signalR.LogLevel.Information)
    .build();

let debater1Global = null;
let debater2Global = null;
let debater1UserIdGlobal = null;
let debater2UserIdGlobal = null;
let countdownInterval = null;

// DOM Elements
const debateStatusDiv = document.getElementById("debateStatus");
const debateAreaDiv = document.getElementById("debateArea");
const debateTopicSpan = document.getElementById("debateTopic");
const debater1NameSpan = document.getElementById("debater1Name");
const debater2NameSpan = document.getElementById("debater2Name");
const timerSpan = document.getElementById("timer");
const chatBox = document.getElementById("chatBox");
const messageInput = document.getElementById("messageInput");
const sendButton = document.getElementById("sendButton");
const voteAreaDiv = document.getElementById("voteArea");
const voteDebater1Button = document.getElementById("voteDebater1");
const voteDebater2Button = document.getElementById("voteDebater2");
const debater1VotesSpan = document.getElementById("debater1Votes");
const debater2VotesSpan = document.getElementById("debater2Votes");
const resultsAreaDiv = document.getElementById("resultsArea");
const winnerNameSpan = document.getElementById("winnerName");
const finalDebater1VotesSpan = document.getElementById("finalDebater1Votes");
const finalDebater2VotesSpan = document.getElementById("finalDebater2Votes");
const aiAnalysisResultDiv = document.getElementById("aiAnalysisResult");
const participantsListUl = document.getElementById("participantsList");
const connectionStatusP = document.getElementById("connectionStatus");
const messageInputArea = document.getElementById("messageInputArea");
const readyForNewDebateButton = document.getElementById("readyForNewDebate");

const chatMessagesNonDebateDiv = document.getElementById(
    "chatMessagesNonDebate"
);
const chatMessageInput = document.getElementById("chatMessageInput");
const sendChatMessageButton = document.getElementById("sendChatMessageButton");

function clearDebateUI() {
    debateAreaDiv.style.display = "none";
    resultsAreaDiv.style.display = "none";
    voteAreaDiv.style.display = "none";
    messageInputArea.style.display = "none";
    chatBox.innerHTML = "";
    debateTopicSpan.textContent = "";
    debater1NameSpan.textContent = "";
    debater2NameSpan.textContent = "";
    timerSpan.textContent = "00:00";
    debater1VotesSpan.textContent = "0";
    debater2VotesSpan.textContent = "0";
    if (countdownInterval) clearInterval(countdownInterval);
    debateStatusDiv.textContent = "Waiting for opponent...";
    debateStatusDiv.style.display = "block";

    // Show general chat
    chatMessagesNonDebateDiv.style.display = "block";
    document.getElementById("chatInputAreaNonDebate").style.display = "flex";
}

function addMessageToChat(user, message, isSystem = false) {
    const entry = document.createElement("div");
    entry.classList.add("message-entry");
    if (isSystem) {
        entry.classList.add("system-message");
        entry.innerHTML = `<em>${message}</em>`;
    } else {
        entry.innerHTML = `<strong>${user}:</strong> ${message}`;
    }
    chatBox.appendChild(entry);
    chatBox.scrollTop = chatBox.scrollHeight;
}

function addMessageToNonDebateChat(user, message) {
    const entry = document.createElement("div");
    entry.innerHTML = `<strong>${user}:</strong> ${message}`;
    chatMessagesNonDebateDiv.appendChild(entry);
    chatMessagesNonDebateDiv.scrollTop = chatMessagesNonDebateDiv.scrollHeight;
}

connection.on("JoinedRoomSuccess", (message) => {
    console.log(message);
    connectionStatusP.textContent = "Connected to room!";
    connectionStatusP.className = "text-success";
});

connection.on("ParticipantJoined", (username, participantNumber) => {
    console.log(`${username} joined as participant ${participantNumber}.`);
    debateStatusDiv.textContent = `${username} has joined. Waiting for debate to start...`;
    // Could update a list of participants here
    const li = document.createElement("li");
    li.textContent =
        username +
        (participantNumber === 1
            ? " (Waiting for opponent)"
            : " (Ready to debate)");
    li.id = `user-${username.replace(/\s+/g, "-")}`; // Create an ID for potential removal
    participantsListUl.appendChild(li);
});

connection.on("ParticipantLeft", (username) => {
    console.log(`${username} left.`);
    addMessageToChat(null, `${username} has left the waiting area.`, true);
    debateStatusDiv.textContent = `Waiting for opponent...`;
    const userLi = document.getElementById(
        `user-${username.replace(/\s+/g, "-")}`
    );
    if (userLi) userLi.remove();
});

connection.on("SpectatorJoined", (debater1, debater2) => {
    console.log("Joined as spectator.");
    debateStatusDiv.textContent = `Waiting for debate between ${
        debater1 || "Debater 1"
    } and ${debater2 || "Debater 2"} to start.`;
});

connection.on(
    "DebateAlreadyInProgress",
    (topic, debater1, debater2, transcript, timeRemainingSeconds) => {
        console.log("Debate already in progress. Joining as spectator.");
        debateStatusDiv.style.display = "none";
        debateAreaDiv.style.display = "block";
        resultsAreaDiv.style.display = "none";

        // Hide general chat
        chatMessagesNonDebateDiv.style.display = "none";
        document.getElementById("chatInputAreaNonDebate").style.display =
            "none";

        debateTopicSpan.textContent = topic;
        debater1NameSpan.textContent = debater1;
        debater2NameSpan.textContent = debater2;
        debater1Global = debater1; // For vote button text
        debater2Global = debater2; // For vote button text
        document
            .querySelectorAll(".debater1-vote-name")
            .forEach((el) => (el.textContent = debater1Global));
        document
            .querySelectorAll(".debater2-vote-name")
            .forEach((el) => (el.textContent = debater2Global));

        chatBox.innerHTML = ""; // Clear previous messages
        transcript.forEach((msg) => {
            const parts = msg.split(": ");
            addMessageToChat(parts[0], parts.slice(1).join(": "));
        });

        messageInputArea.style.display = "none"; // Spectators can't send messages to debate
        voteAreaDiv.style.display = "block";

        if (countdownInterval) clearInterval(countdownInterval);
        let timeLeft = timeRemainingSeconds;
        timerSpan.textContent = formatTime(timeLeft);
        countdownInterval = setInterval(() => {
            timeLeft--;
            timerSpan.textContent = formatTime(timeLeft);
            if (timeLeft <= 0) {
                clearInterval(countdownInterval);
                timerSpan.textContent = "Time's up!";
            }
        }, 1000);
    }
);

connection.on(
    "DebateStarted",
    (
        topic,
        debater1,
        debater2,
        debater1UserId,
        debater2UserId,
        durationSeconds
    ) => {
        console.log(
            `Debate started on: ${topic} between ${debater1} and ${debater2}`
        );
        debateStatusDiv.style.display = "none";
        debateAreaDiv.style.display = "block";
        resultsAreaDiv.style.display = "none";

        // Hide general chat
        chatMessagesNonDebateDiv.style.display = "none";
        document.getElementById("chatInputAreaNonDebate").style.display =
            "none";

        debateTopicSpan.textContent = topic;
        debater1NameSpan.textContent = debater1;
        debater2NameSpan.textContent = debater2;
        debater1Global = debater1;
        debater2Global = debater2;
        debater1UserIdGlobal = debater1UserId;
        debater2UserIdGlobal = debater2UserId;

        document
            .querySelectorAll(".debater1-vote-name")
            .forEach((el) => (el.textContent = debater1Global));
        document
            .querySelectorAll(".debater2-vote-name")
            .forEach((el) => (el.textContent = debater2Global));

        chatBox.innerHTML = ""; // Clear previous messages

        if (
            currentUserId === debater1UserId ||
            currentUserId === debater2UserId
        ) {
            messageInputArea.style.display = "flex";
            voteAreaDiv.style.display = "none";
        } else {
            messageInputArea.style.display = "none";
            voteAreaDiv.style.display = "block";
        }

        if (countdownInterval) clearInterval(countdownInterval);
        let timeLeft = durationSeconds;
        timerSpan.textContent = formatTime(timeLeft);
        countdownInterval = setInterval(() => {
            timeLeft--;
            timerSpan.textContent = formatTime(timeLeft);
            if (timeLeft <= 0) {
                clearInterval(countdownInterval);
                timerSpan.textContent = "Time's up!";
                // Server will send DebateEnded
            }
        }, 1000);
    }
);

function formatTime(totalSeconds) {
    const minutes = Math.floor(totalSeconds / 60);
    const seconds = totalSeconds % 60;
    return `${String(minutes).padStart(2, "0")}:${String(seconds).padStart(
        2,
        "0"
    )}`;
}

connection.on("ReceiveMessage", (user, message) => {
    addMessageToChat(user, message);
});

connection.on("ReceiveChatMessage", (user, message) => {
    addMessageToNonDebateChat(user, message);
});

connection.on("UpdateVotes", (votes1, votes2) => {
    debater1VotesSpan.textContent = votes1;
    debater2VotesSpan.textContent = votes2;
});

connection.on("DebateEnded", (winnerName, aiAnalysis, d1Votes, d2Votes) => {
    console.log("Debate ended. Winner: " + winnerName);
    if (countdownInterval) clearInterval(countdownInterval);

    debateAreaDiv.style.display = "none"; // Hide active debate area
    resultsAreaDiv.style.display = "block"; // Show results

    winnerNameSpan.textContent = winnerName;
    finalDebater1VotesSpan.textContent = d1Votes;
    finalDebater2VotesSpan.textContent = d2Votes;
    aiAnalysisResultDiv.textContent = aiAnalysis;

    // Reset debater globals for next round safety
    debater1Global = null;
    debater2Global = null;
    debater1UserIdGlobal = null;
    debater2UserIdGlobal = null;
});

// Event Listeners
if (sendButton) {
    sendButton.addEventListener("click", (event) => {
        const message = messageInput.value;
        if (message.trim() !== "") {
            connection
                .invoke("SendMessage", roomId, message)
                .catch((err) => console.error(err.toString()));
            messageInput.value = "";
        }
        event.preventDefault();
    });
}
if (messageInput) {
    messageInput.addEventListener("keypress", (event) => {
        if (event.key === "Enter") {
            event.preventDefault(); // Prevent form submission if it's in a form
            sendButton.click();
        }
    });
}

if (sendChatMessageButton) {
    sendChatMessageButton.addEventListener("click", (event) => {
        const message = chatMessageInput.value;
        if (message.trim() !== "") {
            // For non-debate chat, we still use "SendMessage" but the hub logic will differentiate
            connection
                .invoke("SendMessage", roomId, message)
                .catch((err) => console.error(err.toString()));
            chatMessageInput.value = "";
        }
        event.preventDefault();
    });
}

if (chatMessageInput) {
    chatMessageInput.addEventListener("keypress", (event) => {
        if (event.key === "Enter") {
            event.preventDefault();
            sendChatMessageButton.click();
        }
    });
}

if (voteDebater1Button) {
    voteDebater1Button.addEventListener("click", (event) => {
        if (debater1UserIdGlobal) {
            connection
                .invoke("CastVote", roomId, debater1UserIdGlobal)
                .catch((err) => console.error(err.toString()));
            voteDebater1Button.disabled = true;
            voteDebater2Button.disabled = true;
            addMessageToChat(null, `You voted for ${debater1Global}.`, true);
        }
        event.preventDefault();
    });
}

if (voteDebater2Button) {
    voteDebater2Button.addEventListener("click", (event) => {
        if (debater2UserIdGlobal) {
            connection
                .invoke("CastVote", roomId, debater2UserIdGlobal)
                .catch((err) => console.error(err.toString()));
            voteDebater1Button.disabled = true;
            voteDebater2Button.disabled = true;
            addMessageToChat(null, `You voted for ${debater2Global}.`, true);
        }
        event.preventDefault();
    });
}

if (readyForNewDebateButton) {
    readyForNewDebateButton.addEventListener("click", async (event) => {
        event.preventDefault();
        clearDebateUI();
        // Re-join logic might be needed if connection dropped or to signal readiness explicitly
        // For simplicity, just clearing UI. The server's RoomStateService handles new debate setup on user join.
        // A "Ready" button on client could invoke a "SignalReadiness" on hub.
        // For now, simply re-joining the room via a full page refresh or navigating away and back
        // would be the simplest way to trigger the join logic again if the server has fully reset the room state.
        // Or, we can re-invoke JoinRoom if the connection is still alive.
        if (connection.state === signalR.HubConnectionState.Connected) {
            try {
                await connection.invoke("JoinRoom", roomId); // This will re-trigger the server's join logic
                debateStatusDiv.textContent =
                    "Signaled readiness for a new debate. Waiting for opponent...";
            } catch (err) {
                console.error("Error signaling readiness: ", err.toString());
                connectionStatusP.textContent =
                    "Error re-joining. Please refresh.";
                connectionStatusP.className = "text-danger";
            }
        } else {
            connectionStatusP.textContent =
                "Connection lost. Please refresh the page.";
            connectionStatusP.className = "text-danger";
        }
    });
}

async function start() {
    try {
        clearDebateUI(); // Assumendo che questa funzione esista e sia definita altrove

        if (typeof roomId === "undefined" || roomId === null || roomId === 0) {
            console.error(
                "Start function: Room ID is not defined or invalid. Cannot start SignalR connection. Value:",
                roomId
            );
            if (connectionStatusP) {
                connectionStatusP.textContent =
                    "Error: Room ID missing or invalid for connection.";
                connectionStatusP.className = "text-danger";
            }
            return;
        }

        if (connection.state === signalR.HubConnectionState.Disconnected) {
            await connection.start();
            console.log("SignalR Connected via start().");
            if (connectionStatusP) {
                connectionStatusP.textContent = "Connecting to room...";
                connectionStatusP.className = "text-warning";
            }
            await connection.invoke("JoinRoom", roomId);
        } else if (connection.state === signalR.HubConnectionState.Connected) {
            console.log(
                "SignalR already connected. Attempting to join room if not already in logic."
            );
            // Potresti voler richiamare JoinRoom qui se la logica lo richiede
            // o assumere che se è connesso, JoinRoom è già stato gestito o lo sarà.
            // Per sicurezza, se start() viene chiamata e si è già connessi,
            // assicurarsi che l'utente sia nel gruppo corretto potrebbe essere utile.
            // await connection.invoke("JoinRoom", roomId); // Valuta se necessario
        }
    } catch (err) {
        console.error("SignalR Connection Error during start(): ", err);
        if (connectionStatusP) {
            connectionStatusP.textContent =
                "Failed to connect. Please refresh.";
            connectionStatusP.className = "text-danger";
        }
        // Considera di non fare un retry automatico qui dentro se start() è già chiamata da onclose
        // setTimeout(start, 5000);
    }
}

connection.onclose(async () => {
    console.log("SignalR Connection Closed.");
    connectionStatusP.textContent =
        "Connection lost. Attempting to reconnect...";
    connectionStatusP.className = "text-danger";
    await start(); // Attempt to reconnect
});

// Alla fine del file stanza.js
if (typeof roomId !== "undefined" && roomId !== null && roomId !== 0) {
    start(); // Chiama start se roomId è valido
} else {
    // Logica di errore se roomId non è valido al momento del caricamento iniziale dello script
    console.error(
        "Initial script load: Room ID is not defined or invalid. Cannot initiate start(). Value:",
        roomId
    );
    const statusP = document.getElementById("connectionStatus");
    if (statusP) {
        statusP.textContent = "Error: Room ID configuration is missing.";
        statusP.className = "text-danger";
    }
}

connection.onclose(async () => {
    console.log("SignalR Connection Closed. Attempting to reconnect...");
    if (connectionStatusP) {
        connectionStatusP.textContent =
            "Connection lost. Attempting to reconnect...";
        connectionStatusP.className = "text-danger";
    }
    // Attendi un po' prima di tentare la riconnessione
    await new Promise((resolve) => setTimeout(resolve, 5000));
    // Assicurati che start() venga chiamata solo se roomId è ancora valido.
    // Se l'ID della stanza dipendesse da uno stato che può cambiare,
    // questa logica di riconnessione potrebbe aver bisogno di più intelligenza.
    if (typeof roomId !== "undefined" && roomId !== null && roomId !== 0) {
        await start();
    } else {
        console.error(
            "onclose: Cannot restart connection, Room ID is invalid."
        );
        if (connectionStatusP) {
            connectionStatusP.textContent =
                "Cannot reconnect: Room configuration error.";
        }
    }
});
