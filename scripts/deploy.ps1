# 项目部署脚本

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  虹乡胡氏安定堂家谱管理系统 V2.0" -ForegroundColor Green
Write-Host "  一键部署脚本" -ForegroundColor Yellow
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$projectRoot = Split-Path -Parent $MyInvocation.MyCommand.Path

# 1. 检查 .NET SDK
Write-Host "[1/5] 检查 .NET SDK..." -ForegroundColor Yellow
$dotnet = Get-Command dotnet -ErrorAction SilentlyContinue
if (-not $dotnet) {
    Write-Host "错误: 未找到 .NET SDK，请先安装 .NET 8.0" -ForegroundColor Red
    exit 1
}
$dotnetVersion = & dotnet --version
Write-Host "  .NET 版本: $dotnetVersion" -ForegroundColor Green

# 2. 还原所有 NuGet 包
Write-Host ""
Write-Host "[2/5] 还原 NuGet 包..." -ForegroundColor Yellow
dotnet restore "$projectRoot\HuFamilyGenealogy.sln"
if ($LASTEXITCODE -ne 0) {
    Write-Host "NuGet 包还原失败!" -ForegroundColor Red
    exit 1
}
Write-Host "  ✓ NuGet 包还原完成" -ForegroundColor Green

# 3. 编译所有项目
Write-Host ""
Write-Host "[3/5] 编译项目..." -ForegroundColor Yellow
dotnet build "$projectRoot\HuFamilyGenealogy.sln" -c Release
if ($LASTEXITCODE -ne 0) {
    Write-Host "编译失败!" -ForegroundColor Red
    exit 1
}
Write-Host "  ✓ 编译完成" -ForegroundColor Green

# 4. 运行测试
Write-Host ""
Write-Host "[4/5] 运行单元测试..." -ForegroundColor Yellow
dotnet test "$projectRoot\src\Editor\Hu.Editor.Tests\Hu.Editor.Tests.csproj"
Write-Host "  ✓ 测试完成" -ForegroundColor Green

# 5. 发布
Write-Host ""
Write-Host "[5/5] 发布可执行文件..." -ForegroundColor Yellow

# 创建发布目录
$publishDir = "$projectRoot\publish"
if (Test-Path $publishDir) { Remove-Item $publishDir -Recurse -Force }
New-Item -ItemType Directory -Path $publishDir -Force | Out-Null

# 发布 EXE 主编端
Write-Host "  发布 EXE 主编端..." -ForegroundColor Gray
dotnet publish "$projectRoot\src\Editor\Hu.Editor.WPF\Hu.Editor.WPF.csproj" `
    -c Release -r win-x64 --self-contained true `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -o "$publishDir\Hu.Editor.WPF"

# 发布 API
Write-Host "  发布私有云 API..." -ForegroundColor Gray
dotnet publish "$projectRoot\src\Cloud\Hu.Cloud.API\Hu.Cloud.API.csproj" `
    -c Release -o "$publishDir\Hu.Cloud.API"

# 发布 Web 采集端
Write-Host "  发布 Web 采集端..." -ForegroundColor Gray
dotnet publish "$projectRoot\src\Collector\Hu.Collector.Web\Hu.Collector.Web.csproj" `
    -c Release -o "$publishDir\Hu.Collector.Web"

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  ✓ 部署完成!" -ForegroundColor Green
Write-Host "  发布目录: $publishDir" -ForegroundColor Yellow
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "下一步:" -ForegroundColor White
Write-Host "  1. 查看 docs/DEPLOYMENT.md 了解详细部署步骤"
Write-Host "  2. 运行 publish\Hu.Cloud.API 启动 API 服务"
Write-Host "  3. 运行 publish\Hu.Editor.WPF 启动主编端"
Write-Host ""
