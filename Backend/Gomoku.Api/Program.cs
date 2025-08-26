// 引入应用程序和基础设施层的命名空间
using Gomoku.Application;      // 引入 RoomAppService, IRoomRepository
using Gomoku.Infrastructure;   // 引入 InMemoryRoomRepository
using Gomoku.Api;        // 引入 GomokuHub

// 创建一个 WebApplicationBuilder 实例，用于配置应用程序的依赖注入和服务
var builder = WebApplication.CreateBuilder(args);

// 在服务容器中注册服务
// AddOpenApi 用于生成 OpenAPI 规范
builder.Services.AddOpenApi();
// 将 RoomAppService 注册为作用域服务
builder.Services.AddScoped<RoomAppService>();
// 将 IRoomRepository 的实现 InMemoryRoomRepository 注册为单例服务
builder.Services.AddSingleton<IRoomRepository, InMemoryRoomRepository>();
// 注册控制器服务
builder.Services.AddControllers(); // 必须有这行
builder.Services.AddSignalR(); // 注册 SignalR

// 注册CORS策略
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .WithOrigins("http://localhost:5500", "http://127.0.0.1:5500") // 明确允许你的前端端口
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials(); // 允许带凭据
    });
});

// 构建 WebApplication 实例
var app = builder.Build();

// 启用CORS
app.UseCors();

// 配置 HTTP 请求处理管道
// 在开发环境中，启用 OpenAPI UI
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// 启用 HTTPS 重定向
//app.UseHttpsRedirection();
// 启用路由
app.UseRouting();
app.UseAuthorization();

// 将控制器端点映射到请求管道
app.MapControllers(); // 必须有这行，启用 Controller 路由
app.MapHub<GomokuHub>("/gamehub"); // 注册 Hub 路由

// 启动应用程序
app.Run();

// ========== 所有类型声明放在文件末尾 ==========
