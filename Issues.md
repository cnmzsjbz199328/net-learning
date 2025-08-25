# 项目开发常见问题记录（Issues）

本文件记录了在本项目开发、容器化和部署过程中遇到的典型问题及解决方法，便于后续复盘和学习。

---

## 1. dotnet build 报错：未找到项目文件
- **现象**：`MSBUILD : error MSB1003: Specify a project or solution file.`
- **原因**：当前目录下没有 .csproj 或 .sln 文件。
- **解决**：切换到包含项目文件的目录，或用 `dotnet build ./路径/项目.csproj`。

## 2. 端口被占用，无法启动服务
- **现象**：`Failed to bind to address ... address already in use.`
- **原因**：端口（如 5900）被系统服务（如 macOS VNC）或其他进程占用。
- **解决**：用 `lsof -i :端口号` 查找并释放端口，或更换端口。

## 3. Docker 容器端口映射不一致，前端访问后端失败
- **现象**：前端容器访问 `http://backend:80` 报错 connection refused。
- **原因**：后端容器实际监听 8080 端口，但 Compose 映射为 80。
- **解决**：要么让后端监听 80 端口（推荐），要么前端访问 `http://backend:8080`。

## 4. 前端反序列化 JSON 属性为默认值
- **现象**：页面显示披萨 ID 为 0，名称和配料为空。
- **原因**：System.Text.Json 默认区分大小写，JSON 字段为小写，C# 属性为大写。
- **解决**：反序列化时加 `PropertyNameCaseInsensitive = true`。

## 5. 前端页面报错 string.Join 参数为 null
- **现象**：ArgumentNullException: Value cannot be null. (Parameter 'value')
- **原因**：Toppings 字段为 null。
- **解决**：用 `pizza.Toppings ?? Array.Empty<string>()` 防御性处理。

## 6. Docker Compose 端口冲突
- **现象**：`bind: address already in use`
- **原因**：端口被占用。
- **解决**：修改 docker-compose.yml 端口映射，选用未被占用端口。

## 7. 前端容器环境变量未生效
- **现象**：前端容器内 BackendUrl 变量正确，但代码未读取。
- **解决**：确认代码用 `Environment.GetEnvironmentVariable("BackendUrl")`，并重新 build 镜像。

## 8. 日志查看无输出
- **现象**：`docker compose logs frontend` 无输出或提示 not found。
- **原因**：未在项目根目录运行，或日志级别设置过高。
- **解决**：切换到 docker-compose.yml 所在目录再执行。

---

如有新问题，持续补充。
