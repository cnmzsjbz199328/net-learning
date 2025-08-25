好的，遵照您的指示。我将使用您提供的**成熟度三级（中级）**文档的格式和标准，对您的“多人五子棋游戏”开发文档进行全面的优化和重构。

这份优化后的文档将更加结构化、专业，并提供了具体的代码和配置示例，使其成为一份可以直接指导开发的高质量设计方案。

---

# 多人五子棋（Gomoku）微服务系统设计方案

## 一、项目目标与成熟度等级

本设计方案旨在构建一个可扩展、可维护的多人在线五子棋游戏平台。方案将严格遵循**企业级微服务成熟度三级（Level 3）**标准，确保项目在初期就具备良好的工程实践基础。

### 🟡 Level 3: 中级 (Intermediate)
**目标**: 实现核心业务的**微服务标准化**，建立**CI/CD自动化**流水线，并搭建**基础可观测性**体系，为后续向高可用、高韧性的四级架构演进奠定坚实基础。

---

## 1. 架构设计 (Architecture Design)

**实施要点**:
- **微服务架构拆分**: 遵循领域驱动设计（DDD）的限界上下文原则，将系统拆分为职责单一的微服务。
- **API网关模式**: 引入API网关作为系统的统一流量入口，负责路由、认证、限流和聚合。
- **统一配置管理**: 采用集中式配置中心，实现配置的动态更新和多环境隔离。

**系统架构图**:
```
+----------------+      +----------------+      +------------------------+
|   Web Frontend |      |                |      |  [gRPC/HTTP]           |
| (Blazor/React) |----->|   API Gateway  |----->|      Room Service      |
+----------------+      |    (YARP)      |      +------------------------+
                        |                |                 |
                        +-------+--------+                 | [Database per Service]
                                |                          |
+----------------+              | [WebSocket]              |             +-----------------+
| SignalR Client |<-------------+------------------------->|------------>|   Persistence   |
+----------------+              |                          |             | (SQL Server/PG) |
                                |                          |             +-----------------+
                                | [HTTP]                   |
                                |                          |             +-----------------+
                                +------------------------->|------------>|   User Service  |
                                |                          |             +-----------------+
                                |                          |
                                | [HTTP]                   |             +-----------------+
                                +------------------------->|------------>|    Bot Service  |
                                                           +-----------------+
```

**微服务拆分**:
- **用户服务 (User Service)**: 负责用户注册、登录、身份验证(JWT生成)和用户信息管理。
- **房间服务 (Room Service)**: 核心服务。负责房间的创建、加入、状态管理、游戏逻辑（落子、胜负判断、消除计分）和玩家调度。
- **机器人服务 (Bot Service)**: 作为一个独立的玩家参与者，接收房间服务的下棋请求并返回落子坐标。
- **API网关 (API Gateway)**: 使用 YARP 或 Ocelot 实现，是所有外部HTTP请求的唯一入口。
- **实时通信层 (Real-time Layer)**: 内嵌于房间服务或独立部署，通过SignalR Hub处理WebSocket连接，向客户端广播房间状态。

**集中式配置示例 (appsettings.json + C#)**:
```csharp
// Program.cs - 从Azure App Configuration加载配置
builder.Configuration.AddAzureAppConfiguration(options =>
{
    options.Connect(builder.Configuration["ConnectionStrings:AppConfig"])
           // 加载通用配置和本服务特定配置
           .Select("Gomoku:Common:*") 
           .Select("Gomoku:RoomService:*", builder.Environment.EnvironmentName)
           .ConfigureRefresh(refresh =>
           {
               // 监听此Key的变更以刷新所有配置
               refresh.Register("Gomoku:Common:Sentinel", refreshAll: true)
                      .SetCacheExpiration(TimeSpan.FromMinutes(5));
           });
});
```

**API网关配置示例 (YARP in appsettings.json)**:
```json
{
  "ReverseProxy": {
    "Routes": {
      "user-route": {
        "ClusterId": "user-cluster",
        "Match": { "Path": "/api/users/{**catch-all}" },
        "AuthorizationPolicy": "Authenticated"
      },
      "room-route": {
        "ClusterId": "room-cluster",
        "Match": { "Path": "/api/rooms/{**catch-all}" }
      }
    },
    "Clusters": {
      "user-cluster": {
        "Destinations": {
          "destination1": { "Address": "http://user-service/" }
        }
      },
      "room-cluster": {
        "Destinations": {
          "destination1": { "Address": "http://room-service/" }
        }
      }
    }
  }
}
```

**关键能力**:
- **服务解耦**: 用户、房间、机器人功能独立，可单独部署、更新和扩展。
- **统一入口**: 所有客户端（Web/Mobile）通过API网关访问后端服务。
- **动态配置**: 无需重启服务即可更新配置，如游戏参数、功能开关。
- **环境隔离**: 开发、测试、生产环境使用不同的配置源，确保安全与稳定。

**验收标准**:
- [ ] 至少3个核心服务（用户、房间、机器人）已实现微服务化并能独立运行。
- [ ] 所有HTTP API调用均通过API网关进行路由和身份验证。
- [ ] 所有服务的数据库连接字符串、密钥等敏感信息均由配置中心或Secret管理。
- [ ] CI/CD流水线已建立，代码提交后可自动部署到测试环境。
- [ ] 具备集中式日志查询能力。

## 2. 容器化与部署 (Containerization & Deployment)

**实施要点**:
- **标准化Dockerfile**: 为每个.NET微服务提供统一的多阶段构建Dockerfile。
- **容器编排入门**: 使用Docker Compose进行本地开发和测试环境的一键部署。

**标准化Dockerfile**:
```dockerfile
# Stage 1: Build Environment
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["RoomService.csproj", "./"]
RUN dotnet restore "RoomService.csproj"
COPY . .
RUN dotnet build "RoomService.csproj" -c Release -o /app/build

# Stage 2: Publish Environment
FROM build AS publish
RUN dotnet publish "RoomService.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Final Runtime Image
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=publish /app/publish .
# 创建一个非root用户来运行应用，增强安全性
RUN useradd -m appuser
USER appuser
ENTRYPOINT ["dotnet", "RoomService.dll"]
```

**Docker Compose配置示例**:
```yaml
# docker-compose.yml
version: '3.8'

services:
  user-service:
    build:
      context: ./src/UserService
      dockerfile: Dockerfile
    ports:
      - "8081:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development

  room-service:
    build:
      context: ./src/RoomService
      dockerfile: Dockerfile
    ports:
      - "8082:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ServiceUrls__UserService=http://user-service:8080

  api-gateway:
    build:
      context: ./src/ApiGateway
      dockerfile: Dockerfile
    ports:
      - "5000:8080"
    depends_on:
      - user-service
      - room-service
```

## 3. CI/CD与自动化 (CI/CD & Automation)

**实施要点**:
- **自动化流水线**: 使用GitHub Actions或Azure Pipelines，实现从代码提交到容器镜像推送的自动化。
- **基础设施即代码 (IaC) 入门**: 数据库、缓存等基础资源建议使用Bicep/ARM模板或Terraform进行声明式管理。

**GitHub Actions流水线示例**:
```yaml
# .github/workflows/ci-cd.yml
name: Build and Push Docker Image

on:
  push:
    branches: [ "main" ]

jobs:
  build_and_push:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v3

    - name: Set up Docker Buildx
      uses: docker/setup-buildx-action@v2

    - name: Log in to Azure Container Registry
      uses: docker/login-action@v2
      with:
        registry: myregistry.azurecr.io
        username: ${{ secrets.ACR_USERNAME }}
        password: ${{ secrets.ACR_PASSWORD }}

    - name: Build and push Room Service
      uses: docker/build-push-action@v4
      with:
        context: ./src/RoomService
        file: ./src/RoomService/Dockerfile
        push: true
        tags: myregistry.azurecr.io/gomoku/room-service:latest
```

## 4. 数据管理 (Data Management)

**实施要点**:
- **服务专属数据库**: 每个微服务拥有独立的数据库，避免跨服务数据耦合。
- **定义清晰的数据模型**: 使用C#的`record`类型定义不可变的数据传输对象（DTOs）。

**核心数据结构 (C# Records)**:
```csharp
// User.cs
public record User(Guid UserId, string UserName, int Score);

// Room.cs
public enum RoomStatus { Waiting, Playing, Finished }
public record Room(
    Guid RoomId,
    string RoomName,
    bool IsPrivate,
    Guid OwnerId,
    List<Player> Players,
    int[,] BoardState, // 15x15 array: 0=empty, 1=player1, 2=player2
    RoomStatus Status
);

// Player.cs
public enum PlayerType { Human, Bot }
public record Player(Guid UserId, string UserName, PlayerType Type);

// GameMessage.cs
public enum MessageType { PlayerJoined, StonePlaced, Scored, System }
public record GameMessage(MessageType Type, string Content, DateTime Timestamp);
```

## 5. 可观测性 (Observability)

**实施要点**:
- **结构化日志**: 所有微服务日志采用JSON格式，并包含关联ID（Correlation ID）以便于链路追踪。
- **基础APM**: 集成Azure Application Insights或OpenTelemetry，监控请求延迟、失败率和依赖调用。

**结构化日志配置 (Serilog)**:
```csharp
// Program.cs - 配置Serilog
builder.Host.UseSerilog((context, services, config) =>
{
    config.ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "RoomService")
        // 将日志以JSON格式输出到控制台，便于Docker/K8s日志收集器处理
        .WriteTo.Console(new Serilog.Formatting.Json.JsonFormatter()); 
});

// 在游戏逻辑中使用
_logger.LogInformation("Player {PlayerId} placed a stone at ({X}, {Y}) in Room {RoomId}", 
    playerId, x, y, roomId);
```

**日志查询示例 (KQL in Azure Log Analytics)**:
```kusto
// 查询特定房间ID的所有游戏活动日志
AppTraces
| where Properties.Application == "RoomService"
| where Message has "Room" // 快速过滤
| extend RoomId = tostring(parse_json(Message).RoomId)
| where RoomId == "your-specific-room-id"
| order by TimeGenerated desc
| project TimeGenerated, Message, SeverityLevel
```

## 6. API与事件定义

**REST API (由API网关暴露)**:
- `POST /api/users/register` - 注册
- `POST /api/users/login` - 登录，返回JWT
- `POST /api/rooms` - 创建房间 (需认证)
- `POST /api/rooms/{roomId}/join` - 加入房间 (需认证)
- `GET /api/rooms` - 获取公共房间列表

**SignalR Hub (`GomokuHub`)**:
- **客户端调用服务端**:
  - `Task JoinRoom(string roomId)`: 加入指定房间的广播组。
  - `Task PlaceStone(int x, int y)`: 在当前房间落子。
- **服务端广播给客户端**:
  - `OnBoardUpdate(int[,] boardState, Guid nextPlayerId)`: 广播最新的棋盘状态和轮到谁。
  - `OnScoreUpdate(Guid playerId, int newScore)`: 广播分数变化。
  - `OnGameEvent(string message, string eventType)`: 广播系统消息（如“玩家xxx加入”、“xxx获胜”）。
  - `OnOpponentStonesRemoved(List<Point> removedStones)`: 广播对手被消除的棋子坐标。

## 7. 开发路线图

**第一阶段: 最小可行产品 (MVP)**
1.  **后端**: 实现用户注册登录、房间创建加入、基础SignalR广播。房间和棋盘状态使用内存存储。
2.  **前端**: 实现登录/注册页面、房间列表、棋盘页面，能通过SignalR实时同步棋盘。
3.  **机器人**: 实现一个简单的随机落子机器人。
4.  **部署**: 使用Docker Compose一键启动本地开发环境。

**第二阶段: 功能增强与完善**
1.  **持久化**: 将用户、房间、对局历史数据存入数据库。
2.  **计分与消除**: 实现五子连珠后的消除和计分逻辑。
3.  **可观测性**: 全面集成Serilog结构化日志和Application Insights。
4.  **CI/CD**: 搭建完整的GitHub Actions流水线，自动化部署到测试环境。
5.  **机器人**: 提升机器人AI难度。
6.  **扩展功能**: 实现观战、聊天、积分排行榜等。