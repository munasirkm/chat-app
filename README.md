## Real-Time Chat App

Simple real-time chat app built with:

- **Backend**: ASP.NET Core 8 (C#), SignalR, SQLite (EF Core)
- **Frontend**: Vue 3 + Vite, @microsoft/signalr

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js](https://nodejs.org/) 18+ and npm

---

## Table of contents

- [Running the app](#running-the-app)
- [Real-time message format](#real-time-message-format)
- [Real-time flow](#real-time-flow)
- [Data model (SQLite)](#data-model-sqlite)
- [Testing](#testing)
- [What I would improve next](#what-i-would-improve-next)

---

## Running the app

### 1. Backend (API + SignalR)

From the repo root:

```bash
dotnet run --project src/RealChat.Api
```

By default the API listens on `http://localhost:5086`.

Key endpoints:

- `GET /` – health/status JSON
- `GET /api/users` – all users (id, name)
- `GET /api/users/{userId}/conversations` – recent chats for a user
- `GET /api/conversations/{userId}/with/{otherUserId}/messages` – message history
- SignalR hub: `/hubs/chat`

> Messages are stored in `realchat.db` (SQLite) in the API project directory.

### 2. Frontend (Vue 3)

In a separate terminal:

```bash
cd client
npm install   # first time only
npm run dev
```

Open `http://localhost:5173` in the browser.

To test two users, open the URL in **two tabs or browsers**, join with different names, and start chatting.

To test two users from a separate device connected to the same network, run:
```bash
npm run dev -- --host
```
Then open http://[your-local-ip]:5173 on the other device’s browser.

---

## Real-time message format

All real-time messages over SignalR use a JSON payload shaped like `ChatMessageDto`:

```json
{
  "type": "chat | connect | typing | error",
  "senderId": "string",
  "receiverId": "string | null",
  "data": "string | null"
}
```

- **type**
  - `connect` – client joins with a user name
  - `chat` – text chat message
  - `typing` – typing indicator
  - `error` – server‑side validation error
- **senderId**
  - For `connect`: the display name sent by the client.
  - For `chat` / `typing`: **set by the server** from the current SignalR connection; the client’s senderId is ignored for authority.
- **receiverId**
  - For `chat` / `typing`: user id of the other participant (string form of the integer id).
  - For `connect` / `error`: not required.
- **data**
  - For `chat`: message text (max length enforced by server).
  - For `typing`: optional (e.g. `"typing"`).
  - For `error`: human‑readable error message.

The backend validates all incoming messages via `MessageValidator`:

- Null messages, invalid `type`, missing `senderId`, missing `receiverId` for `chat`/`typing`, or `data` that is too long are **rejected safely**.
- Invalid payloads never crash the server; instead, the caller receives a `type: "error"` message with details.

---

## Real-time flow

High‑level flow between two users (A and B):

1. **Connect / join**
   - Each client opens a SignalR connection to `/hubs/chat`.
   - Client invokes `Join(userName)`.
   - Server:
     - Creates or looks up a `User` row by name.
     - Tracks connection ↔ user id (`IConnectionTracker`).
     - Adds connection to:
       - `user_{userId}` group (for direct messages)
       - `all` group (for presence broadcasts)
     - Broadcasts `UserOnline(userId)` to `all`.
     - Returns `{ success, userId }` to the caller.

2. **Presence**
   - On join, client also invokes `GetOnlineUserIds()` to fetch all currently‑online user ids.
   - Server maintains online ids from connection tracking.
   - Server broadcasts:
     - `UserOnline(userId)` to group `all` when someone joins.
     - `UserOffline(userId)` to group `all` when a connection disconnects.
   - Frontend keeps a `Set` of online user ids and shows a green dot for those users.

3. **Chat messages**
   - Client A calls `SendMessage({ type: \"chat\", receiverId, data })`.
   - Hub:
     - Resolves `senderId` from the connection (ignores client-provided senderId).
     - Normalizes the DTO (`type = \"chat\"`, `senderId = senderUserId.ToString()`).
     - Validates with `MessageValidator`.
     - Persists to SQLite as a `Message` entity.
     - Forwards to the receiver’s group `user_{receiverId}` as `ReceiveMessage` with the same DTO.
   - Both sides also load full history via the REST API when a conversation is opened.

4. **Typing indicator**
   - While typing, client calls `SetTyping({ type: \"typing\", receiverId, data? })` (wrapped by `connection.setTyping`).
   - Hub:
     - Sets `senderId` from connection, normalizes `type = \"typing\"`.
     - Validates (requires `receiverId` and `senderId`).
     - Forwards to receiver’s `user_{receiverId}` group as `ReceiveMessage` with `type: \"typing\"`.
   - Frontend:
     - For the active conversation, when a `typing` message from the other user is received, shows “X is typing…” for a short time, then clears automatically or when a new chat message arrives.

5. **Disconnect**
   - On disconnect, `OnDisconnectedAsync`:
     - Removes the connection from the tracker.
     - If that user has no more active connections, broadcasts `UserOffline(userId)` to group `all`.

---

## Data model (SQLite)

- **User**
  - `Id` (int, PK, identity)
  - `Name` (string, unique, required)
  - Navigation: `MessagesSent`, `MessagesReceived`
- **Message**
  - `Id` (int, PK)
  - `SenderId` (FK → `User.Id`)
  - `ReceiverId` (FK → `User.Id`)
  - `Data` (string, required, max length enforced)
  - `SentAt` (UTC timestamp)
  - Index on `(SenderId, ReceiverId, SentAt)` for fast conversation lookups.

---

## Testing

### Backend (C#)

From the repo root:

```bash
dotnet test
```

`RealChat.Tests/MessageValidatorTests.cs` covers:

- Null message
- Missing / invalid `type`
- Missing / empty `senderId`
- `chat` without `receiverId` or `data`
- `chat` with `data` longer than the allowed max
- Valid `connect`, `chat`, and `typing` messages
- Case‑insensitive `type` (`\"CHAT\"` etc.)

All invalid payloads are caught at the validator layer and converted into `error` messages, never server crashes.

### Frontend (Vue)

From the `client` directory:

```bash
cd client
npm run test        # watch mode
npm run test:run    # single run
```

Uses Vitest and `@vue/test-utils` for component and unit tests.

---

## What I would improve next

- **Authentication & identity**
  - Replace ad‑hoc name‑based users with proper authentication.
  - Prevent name collisions and allow avatars / profiles.
- **Better history & pagination**
  - Paginate message history with “load more” instead of a fixed limit.
  - Add search, pinning, and per‑conversation settings.
- **Frontend polish**
  - Add responsive layout for mobile.

