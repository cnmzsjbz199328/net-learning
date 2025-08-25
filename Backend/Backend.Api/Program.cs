// 创建 WebApplicationBuilder 实例
var builder = WebApplication.CreateBuilder(args);

// 注册 OpenAPI 服务（用于开发时的 API 文档）
builder.Services.AddOpenApi();

// 构建应用
var app = builder.Build();

// 开发环境下启用 OpenAPI 路由
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// 启用 HTTPS 重定向（开发环境下可选）
app.UseHttpsRedirection();

// 初始化 pizzas 数组（披萨信息列表）
Pizza[] pizzas = new Pizza[]
{
    new Pizza("Margherita", new[] { "Tomato Sauce", "Mozzarella", "Basil" }, 1),
    new Pizza("Pepperoni", new[] { "Tomato Sauce", "Mozzarella", "Pepperoni" }, 2),
    new Pizza("Hawaiian", new[] { "Tomato Sauce", "Mozzarella", "Ham", "Pineapple" }, 3)
};

// 映射 /pizzas 路由，返回 pizzas 数组的 JSON
app.MapGet("/pizzas", () => Results.Json(pizzas)).WithName("GetPizzaInfo");

// 启动应用
app.Run();

// ========== 所有类型声明放在文件末尾 ==========

// 声明 Pizza 记录类型
record Pizza(string Name, string[] Toppings, int Id);
