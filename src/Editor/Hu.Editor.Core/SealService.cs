using Hu.Editor.Core;
using Hu.Editor.Models;
using Microsoft.Data.Sqlite;

namespace Hu.Editor.Core.Services;

/// <summary>
/// 封谱管理服务实现
/// 封谱后数据只读，仅允许追加「生/娶/卒/葬」
/// </summary>
public class SealService : ISealService
{
    private readonly DatabaseManager _dbManager;
    private readonly ILogger<SealService> _logger;

    public SealService(DatabaseManager dbManager, ILogger<SealService> logger)
    {
        _dbManager = dbManager;
        _logger = logger;
    }

    public async Task<bool> IsSealedAsync()
    {
        var conn = _dbManager.GetConnection();
        if (conn == null) return false;

        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT value FROM system_config WHERE key = 'sealed'";
        var result = await cmd.ExecuteScalarAsync();
        return result?.ToString() == "true";
    }

    public async Task SealAsync(string resetCode)
    {
        var conn = _dbManager.GetConnection();
        if (conn == null) throw new InvalidOperationException("数据库未连接");

        // 存储解封码的哈希值
        var hash = ComputeResetCodeHash(resetCode);
        
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            BEGIN TRANSACTION;
            UPDATE system_config SET value = 'true', updated_at = CURRENT_TIMESTAMP 
            WHERE key = 'sealed';
            UPDATE system_config SET value = @hash, updated_at = CURRENT_TIMESTAMP 
            WHERE key = 'reset_code';
            COMMIT;
        ";
        cmd.Parameters.AddWithValue("@hash", hash);
        await cmd.ExecuteNonQueryAsync();

        _logger.LogInformation("Genealogy sealed successfully. Reset code hash stored.");
    }

    public async Task UnsealAsync(string resetCode)
    {
        var conn = _dbManager.GetConnection();
        if (conn == null) throw new InvalidOperationException("数据库未连接");

        var storedHash = await GetResetCodeHashAsync();
        if (storedHash == null)
            throw new InvalidOperationException("未找到解封码，请联系系统管理员");

        var inputHash = ComputeResetCodeHash(resetCode);
        if (inputHash != storedHash)
            throw new UnauthorizedAccessException("解封码错误");

        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            UPDATE system_config SET value = 'false', updated_at = CURRENT_TIMESTAMP 
            WHERE key = 'sealed';
        ";
        await cmd.ExecuteNonQueryAsync();

        _logger.LogWarning("Genealogy UNSEALED by admin with reset code.");
    }

    public async Task<string> GenerateResetCodeAsync()
    {
        // 生成 32 位随机物理重置码
        var bytes = new byte[16];
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        var code = Convert.ToHexString(bytes).ToLower();
        return code;
    }

    public async Task<string?> GetResetCodeHashAsync()
    {
        var conn = _dbManager.GetConnection();
        if (conn == null) return null;

        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT value FROM system_config WHERE key = 'reset_code'";
        var result = await cmd.ExecuteScalarAsync();
        return result?.ToString();
    }

    private string ComputeResetCodeHash(string code)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var hash = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(code));
        return Convert.ToHexString(hash);
    }
}
