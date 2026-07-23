using System.Text.Json;
using Hu.Editor.Core;
using Hu.Editor.Models;

namespace Hu.Editor.Services;

/// <summary>
/// 备份服务实现
/// 操作前自动快照，SQLite 加密备份，支持一键回滚
/// </summary>
public class BackupService : IBackupService
{
    private readonly string _dbPath;
    private readonly string _backupDir;
    private readonly string _encryptionKey;
    private readonly ILogger<BackupService> _logger;

    public BackupService(string dbPath, string backupDir, string encryptionKey,
                         ILogger<BackupService> logger)
    {
        _dbPath = dbPath;
        _backupDir = backupDir;
        _encryptionKey = encryptionKey;
        _logger = logger;
        Directory.CreateDirectory(_backupDir);
    }

    /// <summary>
    /// 创建加密备份
    /// </summary>
    public async Task<string> CreateBackupAsync()
    {
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var backupFileName = $"genealogy_backup_{timestamp}.db.enc";
        var backupPath = Path.Combine(_backupDir, backupFileName);

        try
        {
            // 1. 执行 SQLite VACUUM 确保数据库完整性
            await VacuumDatabaseAsync();

            // 2. 复制加密数据库文件
            File.Copy(_dbPath, backupPath, true);

            // 3. 额外 RSA 加密备份文件
            EncryptBackupFile(backupPath);

            _logger.LogInformation("Backup created successfully: {BackupPath}", backupPath);
            return backupPath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create backup");
            throw;
        }
    }

    /// <summary>
    /// 从备份恢复
    /// </summary>
    public async Task RestoreFromBackupAsync(string backupPath)
    {
        if (!File.Exists(backupPath))
            throw new FileNotFoundException("备份文件不存在", backupPath);

        try
        {
            // 1. 先创建当前状态的备份（防止恢复失败）
            var emergencyBackup = await CreateBackupAsync();
            _logger.LogInformation("Emergency backup created: {Path}", emergencyBackup);

            // 2. 解密备份文件
            var decryptedPath = backupPath + ".dec";
            DecryptBackupFile(backupPath, decryptedPath);

            // 3. 恢复数据库
            File.Copy(decryptedPath, _dbPath, true);

            // 4. 清理临时文件
            if (File.Exists(decryptedPath))
                File.Delete(decryptedPath);

            _logger.LogInformation("Database restored from: {BackupPath}", backupPath);
        }
        catch
        {
            _logger.LogError("Restore failed - emergency backup preserved");
            throw;
        }
    }

    /// <summary>
    /// 列出所有备份
    /// </summary>
    public async Task<List<string>> ListBackupsAsync()
    {
        var backups = Directory.GetFiles(_backupDir, "*.enc")
                              .OrderByDescending(f => f)
                              .ToList();
        return backups;
    }

    /// <summary>
    /// 删除备份
    /// </summary>
    public async Task DeleteBackupAsync(string backupPath)
    {
        if (File.Exists(backupPath))
            File.Delete(backupPath);
        _logger.LogInformation("Backup deleted: {Path}", backupPath);
    }

    /// <summary>
    /// 检查是否需要自动备份
    /// </summary>
    public async Task<bool> AutoBackupIfNeededAsync()
    {
        var lastBackup = GetLastBackupTime();
        var intervalHours = 24;

        // 读取配置
        try
        {
            var configPath = Path.Combine(_backupDir, "config.json");
            if (File.Exists(configPath))
            {
                var config = JsonSerializer.Deserialize<Dictionary<string, string>>(
                    File.ReadAllText(configPath));
                if (config?.ContainsKey("backup_interval_hours") == true)
                    intervalHours = int.Parse(config["backup_interval_hours"]);
            }
        }
        catch { }

        if (lastBackup.HasValue && (DateTime.Now - lastBackup.Value).TotalHours < intervalHours)
            return false;

        await CreateBackupAsync();
        return true;
    }

    private async Task VacuumDatabaseAsync()
    {
        // TODO: 执行 SQLite VACUUM 命令
        await Task.CompletedTask;
    }

    private void EncryptBackupFile(string sourcePath)
    {
        // TODO: 使用 RSA 加密备份文件
        // 实际实现应使用 AesCryptoServiceProvider
    }

    private void DecryptBackupFile(string sourcePath, string destPath)
    {
        // TODO: 解密备份文件
    }

    private DateTime? GetLastBackupTime()
    {
        var backups = ListBackupsAsync().Result;
        if (backups.Count == 0) return null;

        var latest = backups.First();
        var fileName = Path.GetFileName(latest);
        // 从文件名提取时间: genealogy_backup_20260716_120000.db.enc
        var dateStr = fileName.Replace("genealogy_backup_", "")
                             .Replace(".db.enc", "");
        
        if (DateTime.TryParseExact(dateStr, "yyyyMMdd_HHmmss", 
            null, System.Globalization.DateTimeStyles.None, out var dt))
            return dt;
        
        return null;
    }
}
