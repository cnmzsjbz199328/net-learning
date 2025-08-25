好的，遵照您的指示。

这是基于主设计方案为您的**多人五子棋（Gomoku）项目**量身定制的**第一阶段开发文档**。本文档详细阐述了构建最小可行产品（MVP）所需的所有技术细节、任务分解和验收标准。

---

# 多人五子棋（Gomoku）项目 - 第一阶段开发文档：最小可行产品 (MVP)

## 1. 阶段目标

**核心目标**: 快速构建一个可玩的、端到端的游戏核心循环。此阶段的重点是**验证核心玩法和技术栈的连通性**，而非功能的完备性。所有非核心功能（如持久化存储、计分、高级AI）将在此阶段被省略或简化，以便于快速迭代。

**技术焦点**:
- **服务通信**: 确保API网关、用户服务、房间服务之间的HTTP通信畅通。
- **实时同步**: 验证SignalR在前后端之间的实时消息广播能力。
- **容器化部署**: 实现基于Docker Compose的本地一键启动开发环境。

---

## 2. 范围与边界

### ✅ **本阶段包含 (In-Scope)**:

- **用户系统**: 基于内存的用户注册与登录。
- **房间系统**: 创建公共房间、加入指定房间。
- **游戏核心**: 两人轮流落子，棋盘状态在所有客户端实时同步。
- **机器人**: 一个简单的、能随机下棋的机器人对手。
- **前端界面**: 登录/注册、房间列表、游戏棋盘三大核心页面。
- **存储**: **全部使用内存存储**，服务重启后数据丢失。
- **部署**: 提供`docker-compose.yml`文件，支持本地一键启动所有服务。

### ❌ **本阶段不包含 (Out-of-Scope)**:

- **数据库持久化**: 不涉及任何SQL或NoSQL数据库。
- **胜负判断与计分**: 游戏可以一直进行，不判断输赢，不计分，没有五子消除逻辑。
- **房间管理**: 没有私人房间、密码、踢人、观战等功能。
- **高级功能**: 没有聊天、好友、排行榜等。
- **CI/CD流水线**: 暂不搭建自动化部署流水线。
- **可观测性**: 暂不深入集成日志聚合和APM监控。

---

## 3. 任务分解与技术实现

### 3.1. 后端开发任务

#### **任务 1: 用户服务 (User Service)**

- **技术栈**: ASP.NET Core Minimal API
- **存储**: `private readonly ConcurrentDictionary<string, string> _users = new();`
- **API 端点**:
    - **`POST /register`**
        - **Request Body**: `public record RegisterModel(string Username, string Password);`
        - **逻辑**: 检查用户名是否已存在。若不存在，则将用户名和密码（暂存明文）存入字典。
        - **Response**: `201 Created` 或 `409 Conflict` (用户已存在)。
    - **`POST /login`**
        - **Request Body**: `public record LoginModel(string Username, string Password);`
        - **逻辑**: 验证用户名和密码。成功后，生成一个简单的JWT（可包含`UserId`和`Username` claims）。
        - **Response**: `200 OK` 并返回 `{ "token": "your_jwt_string" }`，或 `401 Unauthorized`。

#### **任务 2: 房间服务 (Room Service)**

- **技术栈**: ASP.NET Core Web API + SignalR Hub
- **存储**:
    - `private readonly ConcurrentDictionary<Guid, Room> _rooms = new();`
    - `private readonly ConcurrentDictionary<string, Guid> _userConnections = new();` (用于追踪SignalR连接)
- **API 端点**:
    - **`POST /rooms`**
        - **Request Header**: `Authorization: Bearer <JWT>`
        - **Request Body**: `public record CreateRoomModel(string RoomName);`
        - **逻辑**: 创建一个新的`Room`对象，生成`RoomId`，将创建者添加为第一个玩家，并存入`_rooms`字典。
        - **Response**: `201 Created` 并返回 `{ "roomId": "guid_string" }`。
    - **`GET /rooms`**
        - **逻辑**: 返回当前所有房间的列表（`RoomId`, `RoomName`, 玩家数量）。
        - **Response**: `200 OK` 并返回房间列表。

- **SignalR Hub (`GomokuHub`)**:
    - **连接管理**:
        - `OnConnectedAsync()`: 记录连接ID。
        - `OnDisconnectedAsync()`: 处理玩家断线（从房间中移除，并广播通知）。
    - **服务端方法 (客户端调用)**:
        - `async Task JoinRoom(string roomId)`:
            1.  从JWT中解析`UserId`和`Username`。
            2.  将用户加入房间（如果房间未满两人）。
            3.  将连接加入该房间的SignalR `Group`。
            4.  广播`OnPlayerJoined`事件给房间内所有成员。
            5.  如果房间满两人，则广播`OnGameStart`事件。
        - `async Task PlaceStone(int x, int y)`:
            1.  从JWT解析`UserId`，找到玩家所在房间。
            2.  验证是否轮到该玩家下棋。
            3.  更新内存中的棋盘状态`BoardState`。
            4.  切换下一位玩家。
            5.  广播`OnBoardUpdate`事件给房间内所有成员。
    - **客户端方法 (服务端广播)**:
        - `OnBoardUpdate(int[,] boardState, Guid nextPlayerId)`
        - `OnPlayerJoined(string username)`
        - `OnGameStart(Guid startingPlayerId)`
        - `OnSystemMessage(string message)` (例如: "玩家 xxx 已断开连接")

#### **任务 3: 机器人逻辑 (集成在房间服务内)**

- **触发时机**: 当房间服务检测到当前回合玩家是机器人时。
- **逻辑实现**:
    1.  创建一个`BotPlayer`类。
    2.  当轮到机器人下棋时，调用其`MakeMove(int[,] currentBoard)`方法。
    3.  `MakeMove`方法逻辑：
        - 遍历整个15x15棋盘。
        - 找到所有值为0（空）的格子。
        - 从所有空格子中随机选择一个坐标返回。
    4.  房间服务获取到机器人返回的坐标后，执行`PlaceStone`的后续逻辑（更新棋盘、广播等）。

### 3.2. 前端开发任务 (Blazor Server)

#### **任务 1: 页面与组件**

- **`Login.razor` / `Register.razor`**: 提供输入表单，调用后端的`/login`和`/register` API。成功登录后，将JWT保存到`LocalStorage`或安全的Cookie中，并导航到房间列表页。
- **`RoomList.razor`**:
    - 页面加载时，调用`/rooms` API获取并显示房间列表。
    - 提供“创建房间”按钮和输入框，调用`/rooms` API创建房间。
    - 列表中的每个房间都有一个“加入”按钮。
- **`GamePage.razor` (`@page "/room/{RoomId}"`)**:
    - **初始化 (`OnInitializedAsync`)**:
        1.  从`LocalStorage`读取JWT。
        2.  建立到`GomokuHub`的SignalR连接，并将JWT作为token传递。
        3.  连接成功后，调用Hub的`JoinRoom`方法。
    - **棋盘渲染**: 使用`@for`循环渲染一个15x15的HTML表格或CSS Grid，根据`BoardState`二维数组显示棋子。
    - **交互**:
        - 为棋盘的每个格子添加`@onclick`事件。
        - 点击时，调用Hub的`PlaceStone`方法并传递坐标。
    - **实时更新**:
        - 实现Hub广播的所有客户端方法 (`OnBoardUpdate`, `OnGameStart`等)。
        - 在这些方法中，更新组件的状态变量（如`BoardState`），并调用`StateHasChanged()`来触发UI刷新。

### 3.3. 部署任务

- **任务**: 创建`docker-compose.yml`文件
- **内容**:
    - 定义`user-service`, `room-service`两个服务。
    - 每个服务都通过`build`指令指向其Dockerfile。
    - 映射端口以避免冲突（例如，用户服务`8081:8080`，房间服务`8082:8080`）。
    - 为房间服务设置环境变量，告知其用户服务的地址（`ServiceUrls__UserService=http://user-service:8080`）。
- **运行**: 在项目根目录提供清晰的`README.md`说明，开发者只需执行`docker-compose up --build`即可启动整个系统。

---

## 4. 阶段验收标准

当以下所有条件均满足时，第一阶段开发工作视为完成：

1.  **[✅] 环境启动**: 开发者可以在本地通过单条`docker-compose up`命令成功启动所有后端服务。
2.  **[✅] 用户流程**: 新用户可以成功注册；已注册用户可以成功登录并获取到JWT。
3.  **[✅] 房间创建**: 登录后的用户可以创建一个新的游戏房间。
4.  **[✅] 房间列表**: 用户可以在房间列表页看到所有已创建的公共房间。
5.  **[✅] 玩家加入**: 第二个玩家可以从列表页加入一个已存在的房间。
6.  **[✅] 游戏开始**: 当第二个玩家加入后，两个玩家的界面都应显示游戏开始，并指明谁先手。
7.  **[✅] 实时落子**: 任何一方玩家落子后，双方的棋盘界面都**立即**（无刷新）同步显示最新的棋子位置和回合变更。
8.  **[✅] 机器人对战**: 用户可以创建一个房间并与一个能随机下棋的机器人进行对战。