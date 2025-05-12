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
let hasVoted = false;

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

function clearDebateUI(updateStatusDiv = true) {
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

    if (updateStatusDiv) {
        debateStatusDiv.textContent = "Waiting for opponent...";
        debateStatusDiv.style.display = "block";
    }

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
        const isCurrentUser = user === getUsernameFromId(currentUserId);

        if (isCurrentUser) {
            entry.classList.add("me");
            entry.innerHTML = `${message}`;
        } else {
            entry.classList.add("other");
            entry.innerHTML = `<strong>${user}</strong><div class="message-text">${message}</div>`;
        }
    }

    chatBox.appendChild(entry);

    chatBox.scrollTo({
        top: chatBox.scrollHeight,
        behavior: "smooth",
    });
}

function addMessageToNonDebateChat(user, message) {
    const entry = document.createElement("div");
    entry.classList.add("chat-message");

    const isCurrentUser = user === getUsernameFromId(currentUserId);

    if (isCurrentUser) {
        entry.classList.add("me");
        entry.innerHTML = `<div class="message-bubble">${message}</div>`;
    } else {
        entry.classList.add("other");
        entry.innerHTML = `<strong>${user}</strong><div class="message-bubble">${message}</div>`;
    }

    chatMessagesNonDebateDiv.appendChild(entry);

    chatMessagesNonDebateDiv.scrollTo({
        top: chatMessagesNonDebateDiv.scrollHeight,
        behavior: "smooth",
    });
}

function getUsernameFromId(userId) {
    const participantElement = document.querySelector(
        `#participantsList li[data-user-id="${userId}"]`
    );
    if (participantElement) {
        return participantElement.textContent.trim();
    }

    if (userId === debater1UserIdGlobal) {
        return debater1Global;
    } else if (userId === debater2UserIdGlobal) {
        return debater2Global;
    }

    return "Unknown User";
}

connection.on("JoinedRoomSuccess", (message) => {
    console.log(message);
    connectionStatusP.textContent = "Connected to room!";
    connectionStatusP.className = "text-success";
});

connection.on("ParticipantJoined", (username, participantNumber) => {
    console.log(`${username} joined as participant ${participantNumber}.`);
    debateStatusDiv.textContent = `${username} has joined. Waiting for debate to start...`;
    const li = document.createElement("li");
    li.textContent =
        username +
        (participantNumber === 1
            ? " (Waiting for opponent)"
            : " (Ready to debate)");
    li.id = `user-${username.replace(/\s+/g, "-")}`;
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
    (
        topic,
        debater1Name,
        debater2Name,
        transcript,
        timeRemainingSeconds,
        serverDebater1Id,
        serverDebater2Id, 
        spectatorHasAlreadyVotedParam, 
        currentVotes1,
        currentVotes2 
    ) => {
        console.log(
            `DebateAlreadyInProgress received. Topic: ${topic}, Spectator has voted: ${spectatorHasAlreadyVotedParam}, Time Remaining: ${timeRemainingSeconds}s, Votes: ${currentVotes1}-${currentVotes2}`
        );

        debateStatusDiv.style.display = "none";

        clearDebateUI(false); 

        debateAreaDiv.style.display = "block";
        resultsAreaDiv.style.display = "none";

        if (chatMessagesNonDebateDiv)
            chatMessagesNonDebateDiv.style.display = "none";
        const chatInputNonDebate = document.getElementById(
            "chatInputAreaNonDebate"
        );
        if (chatInputNonDebate) chatInputNonDebate.style.display = "none";

        debateTopicSpan.textContent = topic;
        debater1NameSpan.textContent = debater1Name;
        debater2NameSpan.textContent = debater2Name;

        debater1Global = debater1Name;
        debater2Global = debater2Name;
        debater1UserIdGlobal = serverDebater1Id;
        debater2UserIdGlobal = serverDebater2Id;
        console.log(
            `DebateAlreadyInProgress: Set globals - D1: ${debater1Global} (ID: ${debater1UserIdGlobal}), D2: ${debater2Global} (ID: ${debater2UserIdGlobal})`
        );

        document
            .querySelectorAll(".debater1-vote-name")
            .forEach((el) => (el.textContent = debater1Global));
        document
            .querySelectorAll(".debater2-vote-name")
            .forEach((el) => (el.textContent = debater2Global));

        chatBox.innerHTML = ""; 
        transcript.forEach((msg) => {
            const parts = msg.split(": "); 
            if (parts.length >= 2) {
                addMessageToChat(parts[0], parts.slice(1).join(": "));
            } else {
                addMessageToChat(null, msg, true);
            }
        });

        if (messageInputArea) messageInputArea.style.display = "none";

        if (voteAreaDiv) {
            if (
                currentUserId !== serverDebater1Id &&
                currentUserId !== serverDebater2Id
            ) {
                voteAreaDiv.style.display = "block";
                hasVoted = spectatorHasAlreadyVotedParam; 
                if (voteDebater1Button) voteDebater1Button.disabled = hasVoted;
                if (voteDebater2Button) voteDebater2Button.disabled = hasVoted;
            } else {
                voteAreaDiv.style.display = "none"; 
            }
        }

        if (debater1VotesSpan) debater1VotesSpan.textContent = currentVotes1;
        if (debater2VotesSpan) debater2VotesSpan.textContent = currentVotes2;

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
        debater1Name, 
        debater2Name, 
        serverDebater1Id,
        serverDebater2Id, 
        durationSeconds,
        serverDebater1Avatar, 
        serverDebater2Avatar 
    ) => {
        console.log(
            `DebateStarted received. Topic: ${topic} between ${debater1Name} and ${debater2Name}. Duration: ${durationSeconds}s`
        );

        const statusDivFromDom = document.getElementById("debateStatus");
        console.log(
            "DEBATESTARTED: Is global debateStatusDiv the same as freshly fetched one?",
            debateStatusDiv === statusDivFromDom
        );
        console.log("DEBATESTARTED: Global debateStatusDiv:", debateStatusDiv);
        console.log(
            "DEBATESTARTED: Freshly fetched debateStatusDiv:",
            statusDivFromDom
        );

        if (statusDivFromDom) {
            console.log("DEBATESTARTED: Attempting to hide statusDivFromDom");
            statusDivFromDom.style.display = "none";
            console.log(
                "DEBATESTARTED: statusDivFromDom.style.display is now:",
                statusDivFromDom.style.display
            );
        } else {
            console.error(
                "DEBATESTARTED: Could not find element with ID 'debateStatus' in DOM!"
            );
        }

        clearDebateUI(false);

        if (debateAreaDiv) debateAreaDiv.style.display = "block";
        if (resultsAreaDiv) resultsAreaDiv.style.display = "none";

        if (chatMessagesNonDebateDiv)
            chatMessagesNonDebateDiv.style.display = "none";
        const chatInputNonDebate = document.getElementById(
            "chatInputAreaNonDebate"
        );
        if (chatInputNonDebate) chatInputNonDebate.style.display = "none";

        debateTopicSpan.textContent = topic;
        debater1NameSpan.textContent = debater1Name;
        debater2NameSpan.textContent = debater2Name;

        debater1Global = debater1Name;
        debater2Global = debater2Name;
        debater1UserIdGlobal = serverDebater1Id;
        debater2UserIdGlobal = serverDebater2Id;
        console.log(
            `DebateStarted: Set globals - D1: ${debater1Global} (ID: ${debater1UserIdGlobal}), D2: ${debater2Global} (ID: ${debater2UserIdGlobal})`
        );

        document
            .querySelectorAll(".debater1-vote-name")
            .forEach((el) => (el.textContent = debater1Global));
        document
            .querySelectorAll(".debater2-vote-name")
            .forEach((el) => (el.textContent = debater2Global));

        chatBox.innerHTML = "";

        if (
            currentUserId === serverDebater1Id ||
            currentUserId === serverDebater2Id
        ) {
            if (messageInputArea) messageInputArea.style.display = "flex";
            if (voteAreaDiv) voteAreaDiv.style.display = "none";
        } else {
            if (messageInputArea) messageInputArea.style.display = "none";
            if (voteAreaDiv) {
                hasVoted = false;
                if (voteDebater1Button) voteDebater1Button.disabled = false;
                if (voteDebater2Button) voteDebater2Button.disabled = false;
                voteAreaDiv.style.display = "block";
            }
        }

        if (debater1VotesSpan) debater1VotesSpan.textContent = "0";
        if (debater2VotesSpan) debater2VotesSpan.textContent = "0";

        const debater1AvatarImg = document.getElementById("debater1Avatar");
        if (debater1AvatarImg && serverDebater1Avatar) {
            debater1AvatarImg.src = `/images/avatars/${serverDebater1Avatar}`;
        }
        const debater2AvatarImg = document.getElementById("debater2Avatar");
        if (debater2AvatarImg && serverDebater2Avatar) {
            debater2AvatarImg.src = `/images/avatars/${serverDebater2Avatar}`;
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
            }
        }, 1000);

        const finalStatusDiv = document.getElementById("debateStatus");
        if (finalStatusDiv) {
            console.log(
                "DEBATESTARTED END: finalStatusDiv.style.display is:",
                finalStatusDiv.style.display
            );
        } else {
            console.error("DEBATESTARTED END: finalStatusDiv is null!");
        }
    }
);

if (voteDebater1Button) {
    voteDebater1Button.addEventListener("click", (event) => {
        console.log("Vote for Debater 1 button CLICKED"); 
        event.preventDefault();
        if (hasVoted) {
            console.log("Vote attempt: Already voted."); 
            addMessageToChat(
                null,
                "You have already voted in this debate.",
                true
            );
            return;
        }
        if (debater1UserIdGlobal && debater1Global) {
            console.log(
                "Vote attempt: debater1UserIdGlobal is set:",
                debater1UserIdGlobal
            );
            const confirmationMessage =
                "Are you sure you want to vote for " +
                (debater1Global || "Debater 1") +
                "? " +
                "This vote is final and cannot be changed. It's best to vote towards the end of the debate.";
            if (confirm(confirmationMessage)) {
                console.log("Vote confirmed by user for Debater 1.");
                voteDebater1Button.disabled = true;
                voteDebater2Button.disabled = true;
                connection
                    .invoke("CastVote", roomId, debater1UserIdGlobal)
                    .then(() => {
                        console.log(
                            "CastVote for Debater 1 successful (then block)."
                        );
                        hasVoted = true;
                        addMessageToChat(
                            null,
                            `You voted for ${debater1Global}.`,
                            true
                        );
                    })
                    .catch((err) => {
                        console.error(
                            `Error invoking 'CastVote' for ${debater1Global}:`,
                            err.toString()
                        );
                        let userErrorMessage =
                            "An error occurred while trying to cast your vote. Please try again.";
                        addMessageToChat(null, userErrorMessage, true);
                        if (!hasVoted) {
                            voteDebater1Button.disabled = false;
                            voteDebater2Button.disabled = false;
                        }
                    });
            }
        } else {
            console.error(
                "Cannot vote for Debater 1: debater1UserIdGlobal or debater1Global is not set.",
                debater1UserIdGlobal,
                debater1Global
            ); 
            addMessageToChat(
                null,
                "Cannot vote: Debater information is missing.",
                true
            );
        }
    });
}

if (voteDebater2Button) {
    voteDebater2Button.addEventListener("click", (event) => {
        console.log("Vote for Debater 2 button CLICKED"); 
        event.preventDefault();

        if (hasVoted) {
            console.log("Vote attempt: Already voted.");
            addMessageToChat(
                null,
                "You have already voted in this debate.",
                true
            );
            return;
        }

        if (debater2UserIdGlobal && debater2Global) {
            console.log(
                "Vote attempt: debater2UserIdGlobal is set:",
                debater2UserIdGlobal
            ); 
            const confirmationMessage =
                "Are you sure you want to vote for " +
                debater2Global +
                "? " +
                "This vote is final and cannot be changed. It's best to vote towards the end of the debate.";

            if (confirm(confirmationMessage)) {
                console.log("Vote confirmed by user for Debater 2."); 
                voteDebater1Button.disabled = true;
                voteDebater2Button.disabled = true;

                connection
                    .invoke("CastVote", roomId, debater2UserIdGlobal)
                    .then(() => {
                        console.log(
                            "CastVote for Debater 2 successful (then block)."
                        ); 
                        hasVoted = true;
                        addMessageToChat(
                            null,
                            `You voted for ${debater2Global}.`,
                            true
                        );
                    })
                    .catch((err) => {
                        console.error(
                            `Error invoking 'CastVote' for ${debater2Global}:`,
                            err.toString()
                        ); 
                        let userErrorMessage =
                            "An error occurred while trying to cast your vote. Please try again.";
                        addMessageToChat(null, userErrorMessage, true);
                        if (!hasVoted) {
                            voteDebater1Button.disabled = false;
                            voteDebater2Button.disabled = false;
                        }
                    });
            } else {
                console.log("Vote for Debater 2 cancelled by user.");
            }
        } else {
            console.error(
                "Cannot vote for Debater 2: debater2UserIdGlobal or debater2Global is not set.",
                debater2UserIdGlobal,
                debater2Global
            ); 
            addMessageToChat(
                null,
                "Cannot vote: Debater information is missing.",
                true
            );
        }
    });
}

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

    debateAreaDiv.style.display = "none";
    resultsAreaDiv.style.display = "block";

    winnerNameSpan.textContent = winnerName;
    finalDebater1VotesSpan.textContent = d1Votes;
    finalDebater2VotesSpan.textContent = d2Votes;
    aiAnalysisResultDiv.textContent = aiAnalysis;

    debater1Global = null;
    debater2Global = null;
    debater1UserIdGlobal = null;
    debater2UserIdGlobal = null;
    hasVoted = false;
    if (voteDebater1Button) voteDebater1Button.disabled = true;
    if (voteDebater2Button) voteDebater2Button.disabled = true;
});

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
            event.preventDefault(); 
            sendButton.click();
        }
    });
}

if (sendChatMessageButton) {
    sendChatMessageButton.addEventListener("click", (event) => {
        const message = chatMessageInput.value;
        if (message.trim() !== "") {
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

if (readyForNewDebateButton) {
    readyForNewDebateButton.addEventListener("click", async (event) => {
        event.preventDefault();
        clearDebateUI();
        if (connection.state === signalR.HubConnectionState.Connected) {
            try {
                await connection.invoke("JoinRoom", roomId); 
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
        clearDebateUI(); 

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
        }
    } catch (err) {
        console.error("SignalR Connection Error during start(): ", err);
        if (connectionStatusP) {
            connectionStatusP.textContent =
                "Failed to connect. Please refresh.";
            connectionStatusP.className = "text-danger";
        }
    }
}

connection.onclose(async () => {
    console.log("SignalR Connection Closed. Attempting to reconnect...");
    if (connectionStatusP) {
        connectionStatusP.textContent =
            "Connection lost. Attempting to reconnect...";
        connectionStatusP.className = "text-danger";
    }
    await new Promise((resolve) => setTimeout(resolve, 5000));
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

if (typeof roomId !== "undefined" && roomId !== null && roomId !== 0) {
    start(); 
} else {
    console.error(
        "Initial script load: Room ID is not defined or invalid. Cannot initiate start(). Value:",
        roomId
    );
    if (connectionStatusP) {
        connectionStatusP.textContent =
            "Error: Room ID configuration is missing.";
        connectionStatusP.className = "text-danger";
    }
}
