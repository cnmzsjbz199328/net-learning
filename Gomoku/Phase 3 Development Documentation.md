# 多人五子棋（Goku）项目 - 第三阶段（修订版）：生产就绪与云平台部署

## 1. 阶段目标

**核心目标**: 将应用从本地Docker环境**平滑迁移至云端PaaS平台**，实现**自动化部署**和**生产级运维**。此阶段的重点是利用现代化云平台的能力，以最小的运维成本，获得一个安全、可观测且具备基本扩展能力的产品。

**技术焦点**:
- **PaaS平台部署**: 以前端（Vercel/Netlify）和后端（Heroku/Render.com）分离的模式进行部署。
- **CI/CD集成**: 建立从Git仓库到云平台的自动化部署流水线。
- **云原生数据库与缓存**: 使用云平台提供的托管数据库（Managed Database）和缓存服务。
- **集中化配置与密钥管理**: 利用云平台的环境变量和Secrets管理机制。
- **生产级可观测性**: 集成平台自带的或第三方的日志和监控服务。
- **安全性基础**: 确保API的公开端点受到保护，并配置自定义域名和HTTPS。

---

## 2. 范围与边界

### ✅ **本阶段包含 (In-Scope)**:

- **前端部署 (Vercel/Netlify/Cloudflare Pages)**:
    - 将Blazor WASM或React前端项目部署到静态托管平台。
    - 配置自定义域名和自动签发的SSL/TLS证书。
- **后端部署 (Heroku/Render.com)**:
    - 将所有.NET微服务容器化，并部署到PaaS平台。
    - 为每个服务配置独立的域名（如`user-api.gomoku.com`）。
- **自动化部署 (CI/CD)**:
    - 将GitHub仓库与Vercel和Heroku/Render关联，实现`git push`即部署。
- **托管数据服务**:
    - 迁移到Heroku Postgres或Render PostgreSQL等托管数据库服务。
    - 使用Heroku Data for Redis或Render Redis等托管缓存服务。
- **配置与密钥管理**:
    - 使用平台提供的环境变量和Secrets功能来管理数据库连接字符串、JWT密钥等。
- **健康检查与日志**:
    - 实现ASP.NET Core健康检查端点，供平台用于进程健康判断。
    - 将结构化日志（JSON格式）输出到标准输出（stdout），由平台自动收集和展示。
- **API安全**:
    - 为所有后端API配置CORS策略，仅允许前端域名访问。
    - 确保所有公开API都经过JWT身份验证。

### ❌ **本阶段不包含 (Out-of-Scope)**:

- **Kubernetes与容器编排**: 不使用AKS、EKS等复杂的容器编排系统。
- **基础设施即代码 (IaC)**: 不使用Bicep、Terraform等工具，依赖平台UI或CLI进行资源创建。
- **服务网格与高级流量管理**: 不引入Istio/Linkerd，不实现金丝雀发布。
- **自定义监控**: 依赖平台自带的监控，不自行部署Prometheus/Grafana。
- **分布式追踪**: 暂不引入Jaeger或类似的分布式追踪系统。

---

## 3. 任务分解与技术实现

### 3.1. 应用适应性改造任务

#### **任务 1: 前端构建模式调整**

- **如果使用Blazor Server**: 需要将其容器化，并与其他后端服务一起部署到Heroku/Render。
- **如果使用Blazor WASM/React (推荐)**:
    - 配置项目构建命令，使其生成一个纯静态文件目录（如`dist`或`build`）。
    - 在前端代码中，将所有API请求的地址改为指向云端后端的公共URL（通过环境变量注入）。
    - **示例 (`.env.production`)**: `VITE_API_BASE_URL=https://api.gomoku.com`

#### **任务 2: 后端配置与端口绑定**

- **端口**: Heroku/Render等平台通过`PORT`环境变量来告知应用应该监听哪个端口。必须修改`Program.cs`来读取此变量。
    ```csharp
    // Program.cs
    var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
    builder.WebHost.UseUrls($"http://*:{port}");
    ```
- **配置**: 重构所有服务，使其完全通过环境变量读取配置。移除本地`appsettings.json`中的敏感信息。
- **数据库连接**: Heroku/Render通常提供一个`DATABASE_URL`环境变量。需要安装一个库（如`Heroku.EntityFrameworkCore.PostgreSQL`）来解析这个URL。
    ```csharp
    // Program.cs
    var connectionString = builder.Configuration["DATABASE_URL"];
    // 如果平台提供的是标准URL，可能需要手动解析
    builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
    ```

#### **任务 3: 健康检查端点**

- **目的**: 告知PaaS平台你的应用进程是否正常。这是平台进行自动重启和部署判断的关键。
- **实现**: 在所有后端服务中添加一个简单的健康检查端点。
    ```csharp
    // Program.cs
    builder.Services.AddHealthChecks();
    app.MapHealthChecks("/healthz"); // 通常使用 /healthz 或 /health
    ```

### 3.2. 部署任务

#### **任务 1: 部署前端 (Vercel示例)**

1.  **准备**: 确保前端项目（如React）可以本地构建成功 (`npm run build`)。
2.  **关联仓库**: 在Vercel上创建一个新项目，并关联你的GitHub仓库。
3.  **配置构建**: Vercel会自动识别React/Vue等框架。确认构建命令 (`npm run build`) 和输出目录 (`build`) 正确。
4.  **配置环境变量**: 添加`VITE_API_BASE_URL`，值为后端API网关的公共URL。
5.  **部署**: 触发第一次部署。后续所有到`main`分支的推送都会自动部署。
6.  **配置域名**: 在Vercel项目中添加你的自定义域名。

#### **任务 2: 部署后端 (Heroku示例)**

1.  **准备**:
    - 为每个微服务创建一个`Dockerfile`。
    - 在项目根目录创建一个`heroku.yml`文件，用于声明如何构建和发布你的多个服务。
2.  **创建Heroku应用**:
    - `heroku create gomoku-app --stack=container`
    - 为每个微服务创建数据库、Redis等附加资源（Add-ons）。
3.  **编写 `heroku.yml`**:
    ```yaml
    # heroku.yml
    build:
      docker:
        web: src/ApiGateway/Dockerfile
        user-api: src/UserService/Dockerfile
        room-api: src/RoomService/Dockerfile
    release:
      # 如果需要数据库迁移，在此处执行
      image: room-api
      command:
        - dotnet ef database update
    run:
      web: dotnet ApiGateway.dll # 'web'是唯一一个接收外部流量的进程
      user-api: dotnet UserService.dll
      room-api: dotnet RoomService.dll
    ```
4.  **配置环境变量**:
    - 在Heroku应用设置中，为每个服务配置所需的环境变量（如`DATABASE_URL`, `JWT_SECRET`）。Heroku会自动注入Add-ons的连接字符串。
5.  **部署**: `git push heroku main`。Heroku会根据`heroku.yml`构建所有Docker镜像，并启动`run`部分定义的进程。

#### **任务 3: 配置CORS和API网关**

- **CORS**: 在API网关项目中，严格配置CORS策略，只允许来自Vercel前端域名的跨域请求。
    ```csharp
    // Program.cs in ApiGateway
    var frontendUrl = builder.Configuration["FRONTEND_URL"];
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
            policy.WithOrigins(frontendUrl)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials()); // 如果SignalR需要
    });
    ```
- **API网关**: API网关（如果保留）需要配置为路由到Heroku/Render内部的服务地址。在这些平台上，服务间通常可以通过`service-name.internal`或类似地址进行通信。如果平台不支持，则需要通过它们的公共URL进行通信（效率较低）。

### 3.3. 运维与可观测性任务

#### **任务 1: 日志管理**

- **实现**: 确保所有服务都使用Serilog等库将**JSON格式**的日志输出到**控制台 (stdout)**。
- **查看**: Vercel和Heroku/Render的仪表盘都提供了实时日志流功能。你可以直接在网页上查看和搜索所有服务的日志。

#### **任务 2: 监控与告警**

- **监控**:
    - **平台自带**: 利用Heroku/Render仪表盘监控CPU、内存、响应时间等基础指标。
    - **第三方集成**: 可以集成如New Relic, Datadog等APM工具（通常作为Add-on提供），以获得更深入的代码级性能分析。
- **告警**: 在Heroku/Render或第三方监控工具中，配置基础告警规则，例如：
    - 当5xx错误率超过5%时发送邮件。
    - 当响应时间连续5分钟超过1秒时发送通知。

---

## 4. 阶段验收标准

当以下所有条件均满足时，第三阶段（修订版）开发工作视为完成：

1.  **[✅] 前端成功部署**: 前端应用已部署到Vercel（或同类平台），可以通过自定义HTTPS域名公开访问。
2.  **[✅] 后端成功部署**: 所有后端微服务已部署到Heroku（或同类平台），并且它们的健康检查端点返回正常。
3.  **[✅] 自动化部署流程**: 向GitHub `main`分支推送代码后，Vercel和Heroku能自动拉取最新代码，并完成新版本的构建和部署，无需人工干预。
4.  **[✅] 功能端到端可用**: 用户可以通过前端域名，无障碍地完成注册、登录、创建房间、并与另一位玩家进行一局完整的实时游戏。
5.  **[✅] 托管数据服务**: 应用的数据被正确地存储在云平台提供的托管数据库中，服务重启后数据不会丢失。
6.  **[✅] 配置安全**: 数据库连接字符串、JWT密钥等所有敏感信息都通过平台的环境变量进行管理，代码库中没有任何硬编码的密钥。
7.  **[✅] 日志可查**: 可以在Heroku/Render的日志控制台中，实时查看所有后端服务输出的结构化日志。
8.  **[✅] CORS策略生效**: 尝试从一个未经授权的域名向后端API发送请求时，该请求会被CORS策略阻止。