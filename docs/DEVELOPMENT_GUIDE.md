# 项目开发规范

## 1. 代码规范

### 命名约定
- **类名/接口名**: PascalCase (如 `GenealogyService`)
- **方法名**: PascalCase (如 `GetPersonAsync`)
- **私有字段**: `_camelCase` (如 `_databaseManager`)
- **局部变量**: `camelCase` (如 `personName`)
- **常量**: `PascalCase` (如 `MaxPersonCount`)
- **文件名**: 与类名一致

### 文件组织
```
src/
├── Editor/           # 主编端
│   ├── Models/       # 数据模型
│   ├── Core/         # 核心业务
│   ├── Services/     # 服务层
│   ├── UI/           # UI 组件
│   └── WPF/          # WPF 界面
├── Collector/        # 采集端
│   ├── Shared/       # 共享逻辑
│   ├── Android/      # MAUI APK
│   └── Web/          # Blazor WASM
└── Cloud/            # 私有云
    ├── API/          # API 服务
    ├── Models/       # 数据模型
    └── Sync/         # 同步引擎
```

## 2. Git 工作流

### 分支策略
```
main          # 主干分支（稳定版）
├── feature/editor-core    # 主编端核心功能
├── feature/collector-app  # APP 采集端
├── feature/web-collector  # Web 采集端
├── feature/cloud-api      # 私有云 API
└── feature/ui-design      # UI 设计
```

### 提交规范
```
feat: 新增人物编辑功能
fix: 修复封谱后仍可修改的问题
docs: 更新部署文档
ui: 调整按钮圆角为8px
test: 添加历法转换测试
refactor: 重构数据库连接管理
```

## 3. UI 开发规范

### 颜色使用
- 主色 `#2C5F5C`（黛青）：仅用于品牌元素、主按钮、导航
- 文字 `#3A3A3A`（浅墨）：正文
- 背景 `#F5F0E8`（米白）：页面底色
- 辅助色仅用于状态标识（成功/警告/错误）

### 间距
- 严格遵循 8px 基准系统
- 禁止使用奇数间距（如 7px, 13px）

### 字体
- 标题：思源宋体 / Microsoft YaHei UI Bold
- 正文：思源黑体 / Microsoft YaHei UI Regular
- 数字：DIN Alternate / Consolas

### 禁止事项
- ❌ 不得使用木纹、复古滤镜等老旧设计元素
- ❌ 不得使用"管理后台"、"控制台"等词汇
- ❌ 采集端不得出现完整世系图
- ❌ 不得在采集端提供导出功能

## 4. 安全规范

### 数据加密
- SQLite 数据库必须使用 SQLCipher AES-256 加密
- 备份文件必须额外 RSA 加密
- 密码必须使用 bcrypt/argon2 哈希存储
- 传输必须使用 HTTPS

### 权限控制
- 采集端提交的数据必须经过审核才能进入主谱
- 封谱后禁止修改已有记录（仅允许追加生/娶/卒/葬）
- 敏感字段（联系方式）必须脱敏存储

### 审计
- 所有写操作必须记录审计日志
- 封谱/解封操作必须记录操作人和时间

## 5. 性能要求

### 目标设备
- Windows: 4GB RAM, Win10/11
- Android: Android 10+, 千元机

### 性能指标
- 人物列表加载 ≤ 2秒（800人）
- UI 动效 ≤ 300ms
- 低配设备自动降级动效
- 离线操作无阻塞

## 6. 测试规范

### 单元测试覆盖率
- 核心业务逻辑 ≥ 80%
- 历法转换 100%
- 封谱机制 100%

### 测试类型
- 单元测试（xUnit + Moq）
- 集成测试（SQLite 内存数据库）
- UI 自动化测试（WPF + MAUI）
