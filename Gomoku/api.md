# 多人五子棋 (Goku) 项目 - 后端API文档 (V1)

**基础URL (Base URL)**: `https://api.gomoku.yourdomain.com`
**认证方式**: JWT (JSON Web Token)。需要认证的接口，必须在HTTP请求头中携带 `Authorization: Bearer <your_jwt_token>`。

---

## 1. 认证 (Authentication)

> 负责访客注册和登录，获取用于后续操作的JWT。

### **`POST /api/auth/register`**

**摘要**: 注册一个临时的访客昵称。

**描述**: 为未登录的用户创建一个临时的身份（访客），该身份在当前游戏会话中有效。此接口返回的JWT权限较低，仅用于加入游戏和发送消息。

**请求体 (Request Body)**: `application/json`
```json
{
  "nickname": "RookiePlayer123"
}
```*   `nickname` (string, required, min: 3, max: 20): 玩家希望在游戏中显示的昵称。

**响应 (Responses)**:

*   **`201 Created`**: 注册成功。
    ```json
    {
      "token": "eyJhbGciOiJI...",
      "userId": "c3a1b2d4-e5f6-7890-1234-567890abcdef",
      "nickname": "RookiePlayer123"
    }
    ```
*   **`400 Bad Request`**: 请求体无效（如昵称太短或太长）。
    ```json
    {
      "error": "Nickname must be between 3 and 20 characters."
    }
    ```
*   **`409 Conflict`**: 昵称已被占用（在某些要求昵称唯一的房间模式下）。
    ```json
    {
      "error": "Nickname is already in use in this room."
    }
    ```

---

## 2. 房间管理 (Rooms)

> 负责游戏房间的创建、查询和加入。

### **`POST /api/rooms/public`**

**摘要**: 快速加入一个公开房间。

**描述**: 为已注册昵称的玩家（访客）快速匹配并加入一个可用的公开房间。如果不存在可加入的房间，系统会自动创建一个新的公开房间。

**认证**: **需要JWT**

**响应 (Responses)**:

*   **`200 OK`**: 成功加入或创建房间。
    ```json
    {
      "roomId": "a1b2c3d4-e5f6-7890-1234-567890abcdef",
      "roomName": "Public Lobby #1",
      "isPrivate": false
    }
    ```
*   **`401 Unauthorized`**: 未提供或提供了无效的JWT。

### **`POST /api/rooms/private`**

**摘要**: 创建一个受密码保护的私人房间。

**描述**: 由房主创建一个新的私人房间，其他玩家需要凭密码才能进入。

**认证**: **需要JWT**

**请求体 (Request Body)**: `application/json`
```json
{
  "roomName": "FriendsOnly",
  "password": "supersecretpassword"
}
```
*   `roomName` (string, required, min: 3, max: 20): 房间的名称。
*   `password` (string, required, min: 4): 进入房间所需的密码。

**响应 (Responses)**:

*   **`201 Created`**: 房间创建成功。
    ```json
    {
      "roomId": "f9e8d7c6-b5a4-3210-fedc-ba9876543210",
      "roomName": "FriendsOnly",
      "isPrivate": true
    }
    ```
*   **`400 Bad Request`**: 请求体无效。
*   **`401 Unauthorized`**: 未提供或提供了无效的JWT。

### **`POST /api/rooms/private/join`**

**摘要**: 加入一个指定的私人房间。

**描述**: 使用房间ID和密码加入一个私人房间。

**认证**: **需要JWT**

**请求体 (Request Body)**: `application/json`
```json
{
  "roomId": "f9e8d7c6-b5a4-3210-fedc-ba9876543210",
  "password": "supersecretpassword"
}
```
*   `roomId` (string, format: uuid, required): 目标房间的唯一ID。
*   `password` (string, required): 房间的密码。

**响应 (Responses)**:

*   **`200 OK`**: 成功加入房间。
    ```json
    {
      "roomId": "f9e8d7c6-b5a4-3210-fedc-ba9876543210",
      "roomName": "FriendsOnly",
      "isPrivate": true
    }
    ```
*   **`401 Unauthorized`**: 未提供或提供了无效的JWT。
*   **`403 Forbidden`**: 密码错误。
    ```json
    {
      "error": "Incorrect password for the room."
    }
    ```
*   **`404 Not Found`**: 房间ID不存在。
    ```json
    {
      "error": "Room not found."
    }
    ```
*   **`410 Gone`**: 房间已满员。
    ```json
    {
      "error": "The room is full."
    }
    ```

### **`GET /api/rooms/public`**

**摘要**: 获取所有公开房间的列表。

**描述**: 返回一个当前可加入的公开房间列表，用于在大厅中展示。

**认证**: **可选JWT** (访客和登录用户都可以查看)

**响应 (Responses)**:

*   **`200 OK`**: 成功获取列表。
    ```json
    [
      {
        "roomId": "a1b2c3d4-e5f6-7890-1234-567890abcdef",
        "roomName": "Public Lobby #1",
        "playerCount": 5,
        "maxPlayers": 10
      },
      {
        "roomId": "b2c3d4e5-f6a7-8901-2345-67890abcdef1",
        "roomName": "Casual Game",
        "playerCount": 8,
        "maxPlayers": 10
      }
    ]
    ```

---

## 3. 实时游戏 (Real-time Gameplay via SignalR)

> 所有核心游戏交互都通过WebSocket (SignalR)进行，以实现低延迟的实时通信。客户端需要先通过HTTP API加入一个房间，然后使用获取到的JWT连接到SignalR Hub。

**Hub URL**: `wss://api.gomoku.yourdomain.com/gamehub`

### **客户端 -> 服务端 事件 (Client-to-Server Events)**

#### `JoinRoom`

**描述**: 当客户端成功加载游戏页面后，调用此方法以正式进入房间的实时通信频道。
**参数**:
*   `roomId` (string): 从HTTP API获取到的房间ID。
**逻辑**: 服务端会将当前连接加入到指定房间的SignalR组，并向该连接推送当前的完整游戏状态。

#### `PlaceStone`

**描述**: 玩家在轮到自己时，调用此方法进行落子。
**参数**:
*   `x` (int): 棋盘的X坐标 (0-14)。
*   `y` (int): 棋盘的Y坐标 (0-14)。
**逻辑**: 服务端会验证该操作是否合法（是否轮到该玩家、位置是否为空等）。如果合法，则更新游戏状态，并向房间内的所有客户端广播后续事件。

### **服务端 -> 客户端 事件 (Server-to-Client Events)**

#### `GameStateUpdate`

**描述**: 当有新玩家加入、游戏开始或网络重连时，服务端会向**单个或所有**客户端推送完整的游戏状态。
**数据负载 (Payload)**:
```json
{
  "boardState": [[0, 1, 0, ...], [2, 0, 1, ...], ...], // 15x15二维数组，0:空, 1:玩家1, 2:玩家2...
  "players": [
    { "userId": "...", "nickname": "PlayerA", "score": 2, "color": 1 },
    { "userId": "...", "nickname": "BotEasy", "score": 0, "color": 2 },
    { "userId": "...", "nickname": "PlayerC", "score": 5, "color": 3 }
  ],
  "currentPlayerId": "...", // 当前轮到谁下棋
  "gameStatus": "InProgress" // "WaitingForPlayers", "InProgress", "Finished"
}
```

#### `MovePlayed`

**描述**: 当一位玩家成功落子后，服务端向房间内**所有**客户端广播此事件。
**数据负载 (Payload)**:
```json
{
  "playerId": "...",
  "x": 5,
  "y": 7,
  "color": 1,
  "nextPlayerId": "..."
}
```

#### `ScoredAndBalanced`

**描述**: 当有玩家达成N子连珠得分后，服务端广播此事件。
**数据负载 (Payload)**:
```json
{
  "scoringPlayer": {
    "userId": "...",
    "newScore": 6,
    "pointsGained": 1
  },
  "removedStones": {
    "winningLine": [{"x":1,"y":1}, {"x":2,"y":2}, ...], // 获胜连珠的棋子
    "balancedStones": [ // 因平衡机制被移除的棋子
      { "playerId": "...", "x": 8, "y": 8 },
      { "playerId": "...", "x": 3, "y": 9 }
    ]
  },
  "nextPlayerId": "..."
}
```

#### `PlayerJoined` / `PlayerLeft`

**描述**: 当有玩家加入或离开房间时，广播此事件，以便客户端更新玩家列表。
**数据负载 (Payload)**:
```json
{
  "player": { "userId": "...", "nickname": "NewPlayer", "score": 0, "color": 4 },
  "message": "NewPlayer has joined the game." // e.g., "PlayerA has left the game."
}
```

#### `ErrorOccurred`

**描述**: 当玩家执行非法操作时（如不是你的回合），服务端向**该名**玩家发送此错误通知。
**数据负载 (Payload)**:
```json
{
  "message": "It's not your turn."
}
```