# 💬 ChatSpot

A modern, real-time chat application built with **.NET** and **React** — supporting group conversations, private messaging, reply threads, and live online presence.

> 🚀 **Live Demo:** chatspot-liart.vercel.app/

---

## ✨ Features

- 🔒 **JWT Authentication** — Secure login, registration with OTP email confirmation, and refresh token support
- 💬 **Group Chats** — Create groups with names, descriptions, and multiple members
- 👥 **Group Management** — Add/remove members, assign roles, leave groups
- 📩 **Private DMs** — One-on-one direct messaging conversations
- ↩️ **Reply Threads** — Reply to specific messages in any conversation
- 🗑️ **Delete Messages** — Remove messages from chats and groups
- 🔍 **Search** — Search users, conversations, and group messages
- 🟢 **Online Status** — See who's active in real time via SignalR
- 🖼️ **Profile Pictures & Bio** — Personalize your account with an avatar and bio
- 📱 **Responsive UI** — Works seamlessly on desktop and mobile

---

## 🖼️ Screenshots

> *Screenshots coming soon 
---

## 🛠️ Tech Stack

### Backend
| Technology | Purpose |
|------------|---------|
| ASP.NET Core | REST API & SignalR hub |
| SignalR | Real-time messaging & online presence |
| Entity Framework Core | ORM & migrations |
| AutoMapper | DTO ↔ model mapping via Profiles |
| PostgreSQL | Users, groups, conversations |
| MongoDB | Message storage |
| JWT Bearer + Refresh Tokens | Authentication & session management |

### Frontend
| Technology | Purpose |
|------------|---------|
| React | UI framework |
| Axios | HTTP client |
| SignalR JS Client | Real-time connection |

### Infrastructure
| Technology | Purpose |
|------------|---------|
| Railway / Render | Cloud deployment |
| PostgreSQL (hosted) | Relational data |
| MongoDB Atlas | Message data |

---

## 🗄️ Architecture Overview

ChatSpot uses a **dual-database** approach optimized for each data type:

- **PostgreSQL** stores structured relational data — users, conversations, groups, and memberships
- **MongoDB** stores messages — offering flexible, high-throughput document storage ideal for chat history with reply references

The backend follows a clean layered architecture with clear separation between **Controllers**, **Services**, **Repositories**, and **Models**, using **Contracts** for interfaces and **Profiles** for AutoMapper mappings.

---

## 📡 API Documentation

> All protected endpoints require the header:
> `Authorization: Bearer <token>`

---

### 🔐 Authentication

#### `POST /api/auth/register`
Register a new user account.

**Request Body:**
```json
{
  "email": "user@example.com",
  "password": "string",
  "userName": "string",
  "bio": "string"
}
```

---

#### `POST /api/auth/confirm-email`
Confirm registration using the OTP sent to the user's email.

**Request Body:**
```json
{
  "otp": "string",
  "email": "string"
}
```

---

#### `POST /api/auth/login`
Authenticate and receive a JWT + refresh token.

**Request Body:**
```json
{
  "email": "string",
  "password": "string"
}
```

---

#### `POST /api/auth/refresh-token`
Get a new access token using a valid refresh token.

**Request Body:**
```json
{
  "token": "string",
  "refreshToken": "string"
}
```

---

#### `POST /api/auth/logout`
Invalidate the current refresh token.

**Request Body:**
```json
{
  "refreshToken": "string"
}
```

---

### 👤 Users

#### `GET /api/user/search`
Search for users by name or username.

**Query Parameters:**
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `SearchQuery` | string | No | Search term |
| `PageNumber` | int | No | Page number (default: 1) |
| `PageSize` | int | No | Results per page (default: 10) |

---

### 💬 Direct Messages (Conversations)

#### `POST /api/chat/create-conversation`
Start a new DM conversation with another user.

**Request Body:**
```json
{
  "otherUserId": "string"
}
```

---

#### `GET /api/chat`
Get all DM conversations for the authenticated user.

**Query Parameters:**
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `SearchQuery` | string | No | Filter conversations |
| `PageNumber` | int | No | Page number |
| `PageSize` | int | No | Results per page |

---

#### `GET /api/chat/{conversationId}`
Get messages inside a specific conversation.

**Path Parameters:**
| Parameter | Type | Required |
|-----------|------|----------|
| `conversationId` | string | Yes |

**Query Parameters:**
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `SearchQuery` | string | No | Search within messages |
| `PageNumber` | int | No | Page number |
| `PageSize` | int | No | Results per page |

---

#### `POST /api/chat/{conversationId}/send-message`
Send a message in a DM conversation. Supports replying to a specific message.

**Request Body:**
```json
{
  "content": "string",
  "replyToId": "string"
}
```
> `replyToId` is optional — omit it for a regular message.

---

#### `DELETE /api/chat/{conversationId}/delete-message`
Delete a message from a conversation.

**Request Body:**
```json
{
  "messageId": "string"
}
```

---

### 👥 Groups

#### `POST /api/group/create-group`
Create a new group chat.

**Request Body:**
```json
{
  "name": "string",
  "description": "string",
  "members": ["userId1", "userId2"]
}
```
> `name` is required. `description` and `members` are optional.

---

#### `GET /api/group`
Get all groups the authenticated user belongs to.

**Query Parameters:**
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `SearchQuery` | string | No | Filter groups |
| `PageNumber` | int | No | Page number |
| `PageSize` | int | No | Results per page |

---

#### `GET /api/group/{groupId}`
Get messages inside a specific group.

**Query Parameters:**
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `SearchQuery` | string | No | Search within messages |
| `PageNumber` | int | No | Page number |
| `PageSize` | int | No | Results per page |

---

#### `POST /api/group/{groupId}/send-message`
Send a message in a group. Supports replying to a specific message.

**Request Body:**
```json
{
  "content": "string",
  "replyToId": "string"
}
```

---

#### `DELETE /api/group/{groupId}/delete-message`
Delete a message from a group.

**Request Body:**
```json
{
  "messageId": "string"
}
```

---

#### `POST /api/group/{groupId}/members`
Add new members to a group.

**Request Body:**
```json
{
  "userIds": ["userId1", "userId2"]
}
```

---

#### `GET /api/group/{groupId}/members`
Get the member list of a group.

**Query Parameters:**
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `SearchQuery` | string | No | Filter members |
| `PageNumber` | int | No | Page number |
| `PageSize` | int | No | Results per page |

---

#### `DELETE /api/group/{groupId}/remove/{targetUserId}`
Remove a member from a group *(admin only)*.

---

#### `DELETE /api/group/{groupId}/leave`
Leave a group as the authenticated user.

---

#### `POST /api/group/{groupId}/members/{userId}/change-role`
Change the role of a group member *(admin only)*.

---

### 🔌 SignalR Hub — `/hubs/chat`

Connect using the SignalR JS client with a Bearer token.

| Event (Client → Server) | Payload | Description |
|--------------------------|---------|-------------|
| `SendMessage` | `{ conversationId, content, replyToId? }` | Send a DM |
| `SendGroupMessage` | `{ groupId, content, replyToId? }` | Send a group message |
| `JoinGroup` | `{ groupId }` | Subscribe to a group room |
| `LeaveGroup` | `{ groupId }` | Unsubscribe from a group room |

| Event (Server → Client) | Payload | Description |
|--------------------------|---------|-------------|
| `ReceiveMessage` | `{ messageId, senderId, content, sentAt, replyToId? }` | New DM received |
| `ReceiveGroupMessage` | `{ messageId, senderId, content, sentAt, replyToId? }` | New group message received |
| `UserOnline` | `{ userId }` | A user came online |
| `UserOffline` | `{ userId }` | A user went offline |
| `MessageDeleted` | `{ messageId }` | A message was deleted |

---

## 🚀 Local Development

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Node.js 18+](https://nodejs.org/)
- [PostgreSQL](https://www.postgresql.org/)
- [MongoDB](https://www.mongodb.com/) or a [MongoDB Atlas](https://www.mongodb.com/atlas) free cluster

### 1. Clone the repo
```bash
git clone https://github.com/your-username/chatspot.git
cd chatspot
```

### 2. Backend setup
```bash
cd ChatSpot
cp appsettings.json appsettings.Development.json
```

Edit `appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "PostgresConnection": "Host=localhost;Database=chatspot;Username=postgres;Password=yourpassword",
    "MongoConnection": "mongodb://localhost:27017"
  },
  "MongoSettings": {
    "DatabaseName": "chatspot_messages"
  },
  "Jwt": {
    "Key": "your-super-secret-key-at-least-32-chars",
    "Issuer": "ChatSpot",
    "Audience": "ChatSpotUsers"
  },
  "Email": {
    "SmtpHost": "smtp.example.com",
    "FromAddress": "no-reply@chatspot.com"
  }
}
```

```bash
dotnet ef database update
dotnet run
```

Backend runs at `http://localhost:5000`

### 3. Frontend setup
```bash
cd web
npm install
```

Create a `.env` file:
```env
VITE_API_URL=http://localhost:5000
VITE_SIGNALR_HUB_URL=http://localhost:5000/hubs/chat
```

```bash
npm run dev
```

Frontend runs at `http://localhost:5173`

---

## 📁 Project Structure

```
ChatSpot/
├── Configurations/         # App & service configuration classes
├── Contracts/              # Interfaces for services & repositories
├── Controllers/            # API endpoint controllers
├── Dtos/                   # Request & response data transfer objects
├── Hubs/                   # SignalR real-time hub
├── Infrastrcutre/          # Cross-cutting concerns (middleware, extensions)
├── Migrations/             # EF Core PostgreSQL migrations
├── Models/                 # Entity models (PostgreSQL + MongoDB)
├── Profiles/               # AutoMapper mapping profiles
├── Repositories/           # Data access layer
├── ResourceParameters/     # Pagination & search query parameters
├── Services/               # Business logic layer
├── web/                    # React frontend
├── appsettings.json
├── appsettings.Development.json
└── Program.cs
```

---
👨‍💻 Contributors
<table>
  <tr>
    <td align="center">
      <a href="https://github.com/Adhamelsaady">
        <img src="https://github.com/Adhamelsaady.png" width="80px" style="border-radius:50%" alt="Adham ElSaady"/><br />
        <strong>Adham ElSaady</strong>
      </a><br />
      🔧 Backend
    </td>
    <td align="center">
      <a href="https://github.com/Tarik-Lotfy">
        <img src="https://github.com/Tarik-Lotfy.png" width="80px" style="border-radius:50%" alt="Tarik Lotfy"/><br />
        <strong>Tarik Lotfy</strong>
      </a><br />
      🎨 Frontend & Mobile
    </td>
  </tr>
</table>
