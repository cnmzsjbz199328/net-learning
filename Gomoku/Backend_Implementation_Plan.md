
# 五子棋后端改造实施方案

## 1. 目标与原则

**核心目标**: 将当前的 `Backend.Api` 项目，按照企业级最佳实践，重构为一个功能完整、高度分层、可扩展的五子棋游戏后端，以支持原型文档中定义的MVP（最小可行产品）功能。

**核心原则**:
- **清晰分层 (Layered Architecture)**: 严格遵循关注点分离原则，将项目拆分为独立的、职责单一的层级。
- **领域驱动 (Domain-Driven)**: 业务逻辑的核心（如棋盘规则、房间状态）将封装在领域层，与外部技术实现解耦。
- **依赖倒置 (Dependency Inversion)**: 高层模块不依赖于低层模块的具体实现，而是依赖于抽象（接口），所有具体实现通过依赖注入（DI）在运行时提供。
- **面向接口编程 (Interface-based)**: 关键服务和数据仓储都将通过接口进行定义，便于测试和替换实现。

---

## 2. 最终分层架构设计

我们将采用经典的四层架构模型。这需要我们将解决方案拆分为多个独立的类库项目。

```
.
└── Backend/
    ├── Gomoku.Domain/         # 领域层 (核心业务模型与规则)
    ├── Gomoku.Application/    # 应用层 (业务用例与流程编排)
    ├── Gomoku.Infrastructure/ # 基础设施层 (数据存储、外部服务实现)
    └── Gomoku.Api/            # 表示层 (API端点、SignalR Hub)
```

### 2.1. 各层级职责

- **`Gomoku.Domain` (领域层)**
  - **职责**: 定义项目的核心。包含所有业务实体（Entities）、值对象（Value Objects）和核心领域逻辑。
  - **内容**: `Room`, `Player`, `Board`, `Stone` 等类，以及`GameRules`等纯粹的业务规则。
  - **依赖**: **无**。这是最核心、最纯净的一层，不依赖任何其他层。

- **`Gomoku.Application` (应用层)**
  - **职责**: 编排领域层的对象来执行具体的业务用例（Use Cases）。定义了系统需要提供的功能。
  - **内容**: `RoomService`, `UserService` 等应用服务；`IRoomRepository` 等仓储接口；DTOs（数据传输对象）；CQRS的Commands和Queries。
  - **依赖**: `Gomoku.Domain`。

- **`Gomoku.Infrastructure` (基础设施层)**
  - **职责**: 提供所有技术实现。负责数据的持久化、与第三方服务的交互等。
  - **内容**: `InMemoryRoomRepository` (MVP阶段的内存仓储实现)；`GomokuHub` (SignalR的具体实现)；未来会包含 `EntityFrameworkRoomRepository` (数据库实现)。
  - **依赖**: `Gomoku.Application` (因为它需要实现应用层定义的接口)。

- **`Gomoku.Api` (表示层)**
  - **职责**: 暴露系统的功能给外部世界。处理HTTP请求，承载SignalR连接。
  - **内容**: `RoomsController`, `AuthController`；`Program.cs` (用于配置依赖注入、中间件和路由)。
  - **依赖**: `Gomoku.Application`, `Gomoku.Infrastructure`。

---

## 3. MVP实施步骤 (Step-by-Step)

以下步骤将指导您完成从现有代码到分层架构的完整重构。

### **阶段 0: 项目结构初始化**

1.  **重命名项目**:
    - 在解决方案中，将 `Backend.Api` 项目重命名为 `Gomoku.Api`。
2.  **创建新项目 (类库)**:
    - 在解决方案的 `Backend` 目录下，创建三个新的.NET类库项目：
      - `Gomoku.Domain`
      - `Gomoku.Application`
      - `Gomoku.Infrastructure`
3.  **设置项目引用**:
    - `Gomoku.Api` 添加对 `Gomoku.Application` 和 `Gomoku.Infrastructure` 的引用。
    - `Gomoku.Application` 添加对 `Gomoku.Domain` 的引用。
    - `Gomoku.Infrastructure` 添加对 `Gomoku.Application` 的引用。
4.  **安装NuGet包**:
    - **`Gomoku.Api`**:
      - `Microsoft.AspNetCore.Authentication.JwtBearer` (用于JWT认证)
    - **`Gomoku.Infrastructure`**:
      - 保持对 `Microsoft.AspNetCore.SignalR` 的引用（如果需要的话，但通常Hub的实现放在这里，框架引用在Api层）。

### **阶段 1: 构建领域层 (`Gomoku.Domain`)**

1.  **创建实体 (Entities)**:
    - `Player.cs`: `public record Player(Guid Id, string Nickname, bool IsBot);`
    - `Room.cs`: 一个包含状态（等待、进行中）、玩家列表、棋盘和当前轮到谁的复杂实体。
      ```csharp
      public class Room
      {
          public Guid Id { get; private set; }
          public string Name { get; private set; }
          public List<Player> Players { get; private set; } = new();
          public Board GameBoard { get; private set; }
          // ... 其他属性和方法
          public bool AddPlayer(Player player) { /* ... */ }
          public void StartGame() { /* ... */ }
      }
      ```
2.  **创建值对象 (Value Objects)**:
    - `Board.cs`: `public class Board { private readonly int[,] _stones; ... }`，包含落子、检查胜利等逻辑。
    - `Stone.cs`: `public record Stone(int X, int Y, Guid PlayerId);`

### **阶段 2: 定义应用层 (`Gomoku.Application`)**

1.  **定义仓储接口**:
    - `IRoomRepository.cs`:
      ```csharp
      public interface IRoomRepository
      {
          Task<Room?> GetByIdAsync(Guid id);
          Task<IEnumerable<Room>> GetAllPublicAsync();
          Task AddAsync(Room room);
          Task UpdateAsync(Room room);
      }
      ```
    - `IUserRepository.cs` (MVP阶段可以简化，直接管理Player对象)。
2.  **定义DTOs**:
    - `CreateRoomDto.cs`: `public record CreateRoomDto(string RoomName);`
    - `RoomDto.cs`: `public record RoomDto(Guid Id, string Name, int PlayerCount);`
    - `UserDto.cs`: `public record UserDto(string Token, Guid UserId, string Nickname);`
3.  **创建应用服务**:
    - `RoomAppService.cs`:
      ```csharp
      public class RoomAppService
      {
          private readonly IRoomRepository _roomRepository;
          public RoomAppService(IRoomRepository roomRepository) { /* ... */ }
          public async Task<Guid> CreateRoomAsync(CreateRoomDto dto, Guid ownerId) { /* ... */ }
          public async Task JoinRoomAsync(Guid roomId, Guid playerId) { /* ... */ }
      }
      ```

### **阶段 3: 实现基础设施层 (`Gomoku.Infrastructure`)**

1.  **实现内存仓储 (MVP)**:
    - `InMemoryRoomRepository.cs`:
      ```csharp
      using System.Collections.Concurrent;
      public class InMemoryRoomRepository : IRoomRepository
      {
          private static readonly ConcurrentDictionary<Guid, Room> _rooms = new();
          // ... 实现 IRoomRepository 的所有方法
      }
      ```
2.  **实现SignalR Hub**:
    - `GomokuHub.cs`:
      ```csharp
      public class GomokuHub : Hub
      {
          private readonly RoomAppService _roomService; // 通过DI注入
          public GomokuHub(RoomAppService roomService) { /* ... */ }
          public async Task JoinRoom(string roomId) { /* ... */ }
          public async Task PlaceStone(int x, int y) { /* ... */ }
      }
      ```

### **阶段 4: 配置表示层 (`Gomoku.Api`)**

1.  **清理 `Program.cs`**:
    - 删除所有旧的pizza示例代码。
2.  **配置依赖注入 (DI)**:
    - 在 `Program.cs` 中注册所有服务和仓储：
      ```csharp
      // 注册应用服务
      builder.Services.AddScoped<RoomAppService>();
      builder.Services.AddScoped<UserAppService>();

      // 注册仓储 (MVP阶段使用单例内存实现)
      builder.Services.AddSingleton<IRoomRepository, InMemoryRoomRepository>();
      builder.Services.AddSingleton<IUserRepository, InMemoryUserRepository>();
      ```
3.  **配置认证与SignalR**:
    - 添加JWT认证服务和配置。
    - `builder.Services.AddSignalR();`
    - 在 `app` 构建后，`app.MapHub<GomokuHub>("/gamehub");`
4.  **创建API控制器**:
    - `RoomsController.cs`:
      ```csharp
      [ApiController]
      [Route("api/rooms")]
      public class RoomsController : ControllerBase
      {
          private readonly RoomAppService _roomService;
          public RoomsController(RoomAppService roomService) { /* ... */ }

          [HttpPost("public")]
          [Authorize] // 需要认证
          public async Task<IActionResult> JoinPublicRoom() { /* ... */ }
      }
      ```
    - `AuthController.cs`: 负责调用 `UserAppService` 进行注册/登录并返回JWT。

---

## 4. 下一步

完成以上所有步骤后，您将拥有一个结构清晰、符合MVP要求的后端应用。此时，您可以：
1.  使用 `docker-compose` (如原型文档所述) 启动整个后端。
2.  开始与前端进行对接测试。
3.  在MVP验证通过后，进入第二阶段开发：将 `InMemory` 仓储替换为基于 **Entity Framework Core** 的数据库持久化实现。
