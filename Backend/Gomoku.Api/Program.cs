using Gomoku.Application;      // 引入 RoomAppService, IRoomRepository
using Gomoku.Infrastructure;   // 引入 InMemoryRoomRepository

// 创建 WebApplicationBuilder 实例
var builder = WebApplication.CreateBuilder(args);

// 注册 OpenAPI 服务（用于开发时的 API 文档）
builder.Services.AddOpenApi();
builder.Services.AddScoped<RoomAppService>();
builder.Services.AddSingleton<IRoomRepository, InMemoryRoomRepository>();
builder.Services.AddControllers(); // 必须有这行

// 构建应用
var app = builder.Build();

// 开发环境下启用 OpenAPI 路由
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// 启用 HTTPS 重定向（开发环境下可选）
app.UseHttpsRedirection();
app.UseRouting();

app.MapControllers(); // 必须有这行，启用 Controller 路由

// 启动应用
app.Run();

// ========== 所有类型声明放在文件末尾 ==========
