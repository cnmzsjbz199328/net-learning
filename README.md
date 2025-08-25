# .NET 微服务项目示例

本项目演示了如何用 .NET、Docker 和 Docker Compose 构建前后端分离的微服务应用。

## 项目结构

- `Backend/Backend.Api`：后端 Web API 服务，提供披萨信息接口。
- `Frontend/Frontend.Web`：前端 Razor Pages 网站，展示披萨列表。
- `docker-compose.yml`：一键编排前后端服务。

## 快速开始

### 1. 本地运行（需安装 .NET 9）

```bash
# 后端
cd Backend/Backend.Api
 dotnet run --urls "http://localhost:5910"
# 前端（需修改 API 地址为 http://localhost:5910）
cd ../../Frontend/Frontend.Web
 dotnet run --urls "http://localhost:5902"
```

### 2. Docker Compose 一键启动

```bash
cd /Users/tangjiang/net learning
# 构建并启动
 docker compose build
 docker compose up
```
- 前端访问：http://localhost:5902
- 后端 API：http://localhost:5910/pizzas

## 常见问题

详见 [Issues.md](Issues.md)

## 主要技术栈
- .NET 9
- ASP.NET Core Web API
- ASP.NET Core Razor Pages
- Docker & Docker Compose

## 进阶建议
- 可扩展为多服务、加数据库、API 认证、K8s 部署等。
- 适合微服务入门、.NET 容器化实践。

---

如需更多帮助，详见 guidance.md 或联系作者。
