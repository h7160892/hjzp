# 虹乡胡氏安定堂家谱管理系统 - 部署指南

## 环境要求

### Windows EXE 主编端
- 操作系统：Windows 10/11 (x64)
- 运行库：.NET 8.0 Desktop Runtime
- 内存：≥ 4GB RAM
- 磁盘：≥ 500MB 可用空间
- 可选硬件：扫描仪（TWAIN/WIA 驱动）

### Android APK 采集端
- 操作系统：Android 10+
- 内存：≥ 2GB RAM
- 屏幕：自适应

### Web 采集端
- 浏览器：Chrome 120+ / Edge 120+ / Firefox 120+
- 无需安装，打开网页即可使用

### 私有云 API
- 操作系统：Windows Server 2019+ / Ubuntu 22.04+
- 运行库：.NET 8.0 ASP.NET Core Runtime
- 数据库：SQLite（本地）
- 存储：文件存储（照片/录音/扫描件）
- 网络：内网 IP 可达

---

## 一键部署脚本

### 1. 安装 .NET 8 SDK/Runtime

```powershell
# 检查 .NET 版本
dotnet --version

# 如未安装，下载 .NET 8 Desktop Runtime
# https://dotnet.microsoft.com/download/dotnet/8.0
```

### 2. 部署私有云 API

```powershell
# 进入 API 目录
cd C:\zp\HuFamilyGenealogy\src\Cloud\Hu.Cloud.API

# 还原 NuGet 包
dotnet restore

# 发布
dotnet publish -c Release -o ..\..\..\publish\Hu.Cloud.API

# 配置 HTTPS 证书（开发环境）
dotnet dev-certs https --trust

# 启动 API 服务
cd ..\..\..\publish\Hu.Cloud.API
dotnet Hu.Cloud.API.dll --urls "https://0.0.0.0:5001"
```

### 3. 部署 Web 采集端

```powershell
# 进入 Web 项目目录
cd C:\zp\HuFamilyGenealogy\src\Collector\Hu.Collector.Web

# 还原并发布
dotnet restore
dotnet publish -c Release -o ..\..\..\publish\Hu.Collector.Web

# 将发布目录部署到 IIS / Nginx
# IIS: 创建网站指向 publish\Hu.Collector.Web
# Nginx: 反向代理到 Blazor WASM
```

### 4. 构建 EXE 主编端

```powershell
# 进入编辑器项目目录
cd C:\zp\HuFamilyGenealogy\src\Editor\Hu.Editor.WPF

# 还原并发布（单文件自包含）
dotnet restore
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o ..\..\..\publish\Hu.Editor.WPF

# 生成安装包
# 可使用 WiX Toolset 或 Inno Setup 制作 .exe 安装程序
```

### 5. 构建 Android APK

```powershell
# 进入 MAUI 项目目录
cd C:\zp\HuFamilyGenealogy\src\Collector\Hu.Collector.Android

# 还原并发布 APK
dotnet restore
dotnet publish -c Release -p:ArchiveOnBuild=true -p:AndroidKeyStore=true -p:AndroidSigningKeyStore=mykeystore.jks -p:AndroidSigningKeyAlias=myalias -p:AndroidSigningKeyPass=xxx -p:AndroidSigningStorePass=xxx

# APK 输出在 bin/Release/net8.0-android/android/ 目录
```

---

## 初始化数据库

首次运行 EXE 主编端时，程序会自动创建 SQLite 数据库并初始化表结构。

数据库位置：`C:\Users\<用户名>\AppData\Local\HuFamilyGenealogy\genealogy.db`

如需手动初始化：
```powershell
# 运行 EXE 主编端，首次启动自动建库
.\安定堂家谱编纂端.exe
```

---

## 配置说明

### API 配置文件 (appsettings.json)

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Database": {
    "ConnectionString": "Data Source=genealogy.db;Cache=Shared;",
    "EncryptionKey": "CHANGE_ME_TO_SECURE_KEY"
  },
  "Sync": {
    "PollIntervalSeconds": 300,
    "MaxRetries": 3
  },
  "Storage": {
    "PhotosPath": "C:\\zp\\HuFamilyGenealogy\\storage\\photos",
    "AudiosPath": "C:\\zp\\HuFamilyGenealogy\\storage\\audios",
    "ScansPath": "C:\\zp\\HuFamilyGenealogy\\storage\\scans"
  }
}
```

---

## 防火墙配置

如需内网访问，开放端口：

| 服务 | 端口 | 协议 |
|------|------|------|
| 私有云 API | 5001 | HTTPS |
| Web 采集端 | 80/443 | HTTP/HTTPS |

```powershell
# PowerShell 添加防火墙规则
New-NetFirewallRule -DisplayName "HuCloud API" -Direction Inbound -Protocol TCP -LocalPort 5001 -Action Allow
New-NetFirewallRule -DisplayName "HuWeb Collector" -Direction Inbound -Protocol TCP -LocalPort 443 -Action Allow
```

---

## 备份策略

- 自动备份：每 24 小时（可配置）
- 备份位置：`C:\zp\HuFamilyGenealogy\backups\`
- 备份格式：`genealogy_backup_YYYYMMDD_HHmmss.db.enc`
- 保留策略：默认保留最近 30 个备份

---

## 常见问题

### Q: 数据库加密失败？
A: 确认已安装 SQLCipher，检查 `EncryptionKey` 配置。

### Q: Android 编译失败？
A: 确认安装了 Android SDK Build-Tools 和 Xamarin workload。

```powershell
dotnet workload install android
```

### Q: Web 端无法连接 API？
A: 确认 CORS 配置正确，API 服务已启动。

---
*文档版本：V1.0 | 更新日期：2026-07-16*
