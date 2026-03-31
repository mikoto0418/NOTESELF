# NOTESELF

NOTESELF 是一个面向 Windows 的原生桌面便签应用，当前基于 `WPF + .NET 8` 重建中。

## 项目状态

当前仓库已经完成重构第一阶段：

- 新的 WPF 解决方案骨架
- 旧状态文件兼容模型
- JSON 持久化仓储
- 便签服务基础能力
- 核心单元测试

后续阶段会继续补齐：

- 主界面与便签列表
- 桌面悬浮便签窗口
- 冻结点击穿透
- 托盘、单实例、开机启动
- WebView2 富文本编辑器

## 解决方案结构

```text
NOTESELF.sln
src/
  NOTESELF.Desktop/
  NOTESELF.Core/
  NOTESELF.Infrastructure/
  NOTESELF.Editor.Web/
tests/
  NOTESELF.Core.Tests/
```

## 本地开发

### 环境要求

- Windows
- .NET SDK 8

### 构建

```powershell
dotnet build NOTESELF.sln
```

### 测试

```powershell
dotnet test tests/NOTESELF.Core.Tests/NOTESELF.Core.Tests.csproj
```

### 运行当前桌面壳

```powershell
dotnet run --project src/NOTESELF.Desktop/NOTESELF.Desktop.csproj
```

## 开源协议

本项目采用 [MIT License](LICENSE) 开源。
