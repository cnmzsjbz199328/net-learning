# 多人五子棋（Goku）项目 - 第二阶段（修订版）：功能完备与生产准备

## 1. 阶段目标

**核心目标**: 构建一个功能完备、数据持久、并为**云平台部署做好充分准备**的游戏核心。此阶段的重点是完成核心游戏逻辑，建立稳固的数据持久化层，并采纳云原生开发十二要素（Twelve-Factor App）的关键原则，确保应用能平滑迁移至任何现代PaaS平台。

**技术焦点**:
- **数据持久化**: 全面集成Entity Framework Core，并选用与PaaS平台兼容的数据库（如PostgreSQL）。
- **核心游戏逻辑**: 实现完整的五子连珠胜负判断、得分和棋子消除规则。
- **生产准备 (Production-Readiness)**:
    - **配置**: 采用基于环境变量的配置模式。
    - **日志**: 实现向控制台输出结构化日志。
    - **健康检查**: 添加标准的健康检查端点。
- **本地开发环境**: 使用Docker Compose模拟多服务环境，但保持配置简单，易于向云端映射。

---

## 2. 范围与边界

### ✅ **本阶段包含 (In-Scope)**:

- **数据库集成**:
    - **技术选型**: 推荐使用**PostgreSQL**，因为它在Heroku、Render等平台上有出色且经济高效的托管支持。
    - **实现**: 使用Entity Framework Core + Npgsql驱动进行开发。
    - **数据**: 用户、房间、游戏历史等核心数据将被持久化。
- **完整的游戏逻辑**:
    - 实现五子连珠的**胜负判断**。
    - 实现**得分与消除**机制：获胜方得1分，消除五颗连珠，并随机消除对手五颗棋子。
    - 游戏结束后，房间状态更新为“已结束”。
- **云原生适应性改造**:
    - **配置**: 所有服务重构为优先从**环境变量**读取配置，`appsettings.json`仅用于本地开发的默认值。
    - **日志**: 集成Serilog，将**JSON格式**的日志输出到**标准输出 (stdout)**。
    - **健康检查**: 为所有服务添加`/healthz`端点。
- **本地开发环境 (Docker Compose)**:
    - 提供`docker-compose.yml`文件，其中包含所有微服务和一个PostgreSQL数据库服务。
    - 使用`.env`文件来管理本地开发的环境变量。
- **机器人AI增强**:
    - 机器人AI从“随机落子”升级为“基础攻防逻辑”。
- **密码安全**:
    - 用户密码在注册时使用BCrypt进行哈希处理，并持久化存储。

### ❌ **本阶段不包含 (Out-of-Scope)**:

- **云平台部署**: 本阶段所有工作都在本地完成，不涉及任何Vercel、Heroku账号或部署操作。
- **CI/CD流水线**: 暂不搭建任何自动化构建或部署流水线。
- **高级房间功能**: 依然不支持私人房、密码、观战、踢人等。
- **高级可观测性**: 不涉及APM（如Application Insights）、分布式追踪等。
- **托管云服务**: 本地开发使用Docker容器化的PostgreSQL，不连接任何云数据库。

---

## 3. 任务分解与技术实现

### 3.1. 基础设施与数据层任务 (本地)

#### **任务 1: 配置Docker Compose环境**

- **目标**: 创建一个包含应用服务和一个PostgreSQL数据库的`docker-compose.yml`。
- **`docker-compose.yml` 示例**:
    ```yaml
    version: '3.8'

    services:
      db:
        image: postgres:15
        restart: always
        environment:
          - POSTGRES_USER=gomoku
          - POSTGRES_PASSWORD=localdevpassword
          - POSTGRES_DB=gomoku_db
        ports:
          - "5432:5432"
        volumes:
          - postgres_data:/var/lib/postgresql/data

      user-service:
        build: ./src/UserService
        ports:
          - "8081:8080"
        environment:
          - ASPNETCORE_ENVIRONMENT=Development
          - ConnectionStrings__DefaultConnection=Host=db;Port=5432;Database=gomoku_db;Username=gomoku;Password=localdevpassword
        depends_on:
          - db
      
      # ... room-service 和 api-gateway 类似配置
    
    volumes:
      postgres_data:
    ```
- **数据库连接**: 注意`user-service`的连接字符串`Host`指向的是Docker网络中的服务名`db`。

#### **任务 2: 集成Entity Framework Core with Npgsql**

- **操作**:
    1.  在用户服务和房间服务中，引入`Npgsql.EntityFrameworkCore.PostgreSQL`和`Microsoft.EntityFrameworkCore.Tools` NuGet包。
    2.  定义EF Core实体（Entities）和数据库上下文（DbContext），模型与原第二阶段文档相同。
    3.  在`Program.cs`中配置DbContext：
        ```csharp
        // Program.cs
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
        ```
    4.  使用`dotnet ef migrations add`和`dotnet ef database update`命令生成和应用数据库迁移。

### 3.2. 后端应用开发任务

#### **任务 1: 完成核心游戏逻辑**

- **胜负判断、得分、消除**: 实现逻辑与原第二阶段文档完全相同。
    - 每次落子后，调用`CheckWinCondition`。
    - 如果获胜，更新数据库中的玩家分数，修改棋盘状态，并通过SignalR广播更新。
- **持久化**:
    - 所有对房间和用户状态的修改，都必须通过DbContext写入PostgreSQL数据库。
    - 每次落子都记录一条`GameMove`历史。

#### **任务 2: 实现云原生适应性改造**

- **环境变量配置**:
    - 确保所有配置，特别是连接字符串和JWT密钥，都可以通过环境变量覆盖。
    - **`Program.cs` 示例**:
      ```csharp
      var jwtSecret = builder.Configuration["Jwt:SecretKey"] ?? "default-super-secret-key-for-dev";
      ```
- **结构化日志 (Serilog)**:
    - **`Program.cs` 配置**:
        ```csharp
        builder.Host.UseSerilog((context, config) =>
        {
            config.ReadFrom.Configuration(context.Configuration)
                .Enrich.FromLogContext()
                .WriteTo.Console(new Serilog.Formatting.Json.JsonFormatter()); // 输出JSON到控制台
        });
        ```
    - 确保日志中包含关键业务信息，如`RoomId`, `UserId`。
- **健康检查**:
    - **`Program.cs` 配置**:
        ```csharp
        builder.Services.AddHealthChecks()
            // 添加对数据库的健康检查，确保能连接成功
            .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection"));
            
        app.MapHealthChecks("/healthz");
        ```

#### **任务 3: 机器人AI增强**

- **实现逻辑**: 与原第二阶段文档相同。
    1.  优先进攻（形成活四）。
    2.  其次防守（阻止对手的活四）。
    3.  再次尝试形成活三。
    4.  最后随机落子。

### 3.3. 前端开发任务

- **任务与原第二阶段文档相同**:
    - 在UI上显示游戏结果（获胜/失败通知）。
    - 在界面上显示双方玩家的实时分数。
    - 正确处理棋盘上棋子被消除后的UI刷新。

---

## 4. 阶段验收标准

当以下所有条件均满足时，第二阶段（修订版）开发工作视为完成：

1.  **[✅] 本地环境稳定**: 能够通过单条`docker-compose up --build`命令，在本地成功启动所有微服务和一个PostgreSQL数据库。
2.  **[✅] 数据持久性**: 停止并重启Docker Compose环境后（`docker-compose down && docker-compose up`），已注册的用户、历史房间和分数数据依然存在于PostgreSQL数据库中。
3.  **[✅] 完整游戏流程**: 游戏能够正确判断胜负，获胜方分数正确增加，并且棋盘按规则消除棋子。
4.  **[✅] 配置灵活性**: 可以在`docker-compose.yml`的环境变量中修改JWT密钥或数据库密码，服务能够使用新的配置正常启动。
5.  **[✅] 日志标准化**: 运行`docker-compose logs`时，所有服务都以清晰的JSON格式输出日志到控制台。
6.  **[✅] 健康状态可查**: 在本地运行时，可以通过浏览器或curl访问`http://localhost:8081/healthz`（或其他服务的端口），并看到`Healthy`的状态报告。
7.  **[✅] 密码安全**: 在数据库中检查`Users`表，确认用户的密码字段存储的是哈希值，而不是明文。
8.  **[✅] 机器人智能**: 机器人能够对明显的四子连珠威胁做出正确的防守。