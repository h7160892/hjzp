using Hu.Editor.Core;
using Hu.Editor.Models;
using Microsoft.Data.Sqlite;
using SQLitePCL;

namespace Hu.Editor.Core.Data;

/// <summary>
/// 数据库连接管理 - 加密 SQLite
/// </summary>
public class DatabaseManager : IDisposable
{
    private readonly string _connectionString;
    private SqliteConnection? _connection;
    private static bool _initialized = false;

    static DatabaseManager()
    {
        // 初始化 SQLitePCLRaw
        raw.SetProvider(new SQLitePCL.E_sqlcipher_provider());
        Batteries_V2.Init();
    }

    public DatabaseManager(string dbPath, string encryptionKey)
    {
        _connectionString = $"Data Source={dbPath};Cache=Shared;";
        EncryptionKey = encryptionKey;
    }

    public string EncryptionKey { get; }

    public async Task EnsureConnectedAsync()
    {
        if (_connection != null && _connection.State == System.Data.ConnectionState.Open)
            return;

        _connection = new SqliteConnection(_connectionString);
        _connection.Open();

        // 设置加密密钥
        _connection.ExecuteCommand($"PRAGMA key = '{EncryptionKey}';");

        if (!_initialized)
        {
            await InitializeDatabaseAsync();
            _initialized = true;
        }
    }

    private async Task InitializeDatabaseAsync()
    {
        if (_connection == null) return;

        // 人物表
        await _connection.ExecuteAsync(@"
            CREATE TABLE IF NOT EXISTS persons (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                name TEXT NOT NULL,
                original_name TEXT,
                gender INTEGER NOT NULL DEFAULT 1,
                birth_date TEXT,
                death_date TEXT,
                burial_place TEXT,
                spouse_ids TEXT DEFAULT '[]',
                father_id INTEGER REFERENCES persons(id),
                mother_id INTEGER REFERENCES persons(id),
                birth_order INTEGER,
                education TEXT,
                occupation TEXT,
                honors TEXT,
                residence TEXT,
                contact_phone TEXT,
                contact_wechat TEXT,
                photo_path TEXT,
                scan_paths TEXT DEFAULT '[]',
                audio_paths TEXT DEFAULT '[]',
                relationship_tags TEXT DEFAULT '[]',
                lunar_birth TEXT,
                gan_zhi_birth TEXT,
                age_at_death INTEGER,
                status INTEGER DEFAULT 0,
                created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
                updated_at DATETIME DEFAULT CURRENT_TIMESTAMP,
                version INTEGER DEFAULT 1
            );
        ");

        // 家族表
        await _connection.ExecuteAsync(@"
            CREATE TABLE IF NOT EXISTS families (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                name TEXT NOT NULL,
                ancestor_id INTEGER REFERENCES persons(id),
                generation_count INTEGER DEFAULT 0,
                description TEXT,
                created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
                updated_at DATETIME DEFAULT CURRENT_TIMESTAMP
            );
        ");

        // 世代表
        await _connection.ExecuteAsync(@"
            CREATE TABLE IF NOT EXISTS generations (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                family_id INTEGER NOT NULL REFERENCES families(id),
                generation_number INTEGER NOT NULL,
                generation_name TEXT,
                character_pattern TEXT,
                created_at DATETIME DEFAULT CURRENT_TIMESTAMP
            );
        ");

        // 待审提交表
        await _connection.ExecuteAsync(@"
            CREATE TABLE IF NOT EXISTS pending_submissions (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                submitter_id INTEGER NOT NULL,
                person_data TEXT NOT NULL,
                family_id INTEGER REFERENCES families(id),
                submission_type INTEGER NOT NULL DEFAULT 1,
                status INTEGER DEFAULT 0,
                supplement_reason TEXT,
                reviewer_id INTEGER,
                review_comment TEXT,
                submitted_at DATETIME DEFAULT CURRENT_TIMESTAMP,
                reviewed_at DATETIME,
                synced_to_main BOOLEAN DEFAULT 0
            );
        ");

        // 素材表
        await _connection.ExecuteAsync(@"
            CREATE TABLE IF NOT EXISTS materials (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                person_id INTEGER NOT NULL REFERENCES persons(id),
                material_type INTEGER NOT NULL,
                file_path TEXT NOT NULL,
                cloud_hash TEXT,
                upload_status INTEGER DEFAULT 0,
                ocr_result TEXT,
                ocr_confirmed BOOLEAN DEFAULT 0,
                created_at DATETIME DEFAULT CURRENT_TIMESTAMP
            );
        ");

        // 用户表
        await _connection.ExecuteAsync(@"
            CREATE TABLE IF NOT EXISTS users (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                username TEXT NOT NULL UNIQUE,
                password_hash TEXT NOT NULL,
                display_name TEXT NOT NULL,
                role INTEGER NOT NULL DEFAULT 3,
                family_id INTEGER REFERENCES families(id),
                phone TEXT,
                wechat_id TEXT,
                device_id TEXT,
                is_active BOOLEAN DEFAULT 1,
                created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
                last_login_at DATETIME
            );
        ");

        // 审计日志表
        await _connection.ExecuteAsync(@"
            CREATE TABLE IF NOT EXISTS audit_log (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                user_id INTEGER NOT NULL,
                action TEXT NOT NULL,
                target_type TEXT,
                target_id INTEGER,
                old_value TEXT,
                new_value TEXT,
                ip_address TEXT,
                device_info TEXT,
                created_at DATETIME DEFAULT CURRENT_TIMESTAMP
            );
        ");

        // 系统配置表
        await _connection.ExecuteAsync(@"
            CREATE TABLE IF NOT EXISTS system_config (
                key TEXT PRIMARY KEY,
                value TEXT NOT NULL,
                description TEXT,
                updated_at DATETIME DEFAULT CURRENT_TIMESTAMP
            );
        ");

        // 同步队列表
        await _connection.ExecuteAsync(@"
            CREATE TABLE IF NOT EXISTS sync_queue (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                direction INTEGER NOT NULL,
                entity_type TEXT NOT NULL,
                entity_id INTEGER,
                payload TEXT NOT NULL,
                status INTEGER DEFAULT 0,
                retry_count INTEGER DEFAULT 0,
                created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
                synced_at DATETIME
            );
        ");

        // 创建索引
        await _connection.ExecuteAsync(@"
            CREATE INDEX IF NOT EXISTS idx_persons_father ON persons(father_id);
            CREATE INDEX IF NOT EXISTS idx_persons_name ON persons(name);
            CREATE INDEX IF NOT EXISTS idx_persons_status ON persons(status);
            CREATE INDEX IF NOT EXISTS idx_pending_submitter ON pending_submissions(submitter_id);
            CREATE INDEX IF NOT EXISTS idx_pending_status ON pending_submissions(status);
            CREATE INDEX IF NOT EXISTS idx_materials_person ON materials(person_id);
            CREATE INDEX IF NOT EXISTS idx_audit_user ON audit_log(user_id);
            CREATE INDEX IF NOT EXISTS idx_sync_direction ON sync_queue(direction, status);
        ");

        // 插入默认配置
        await _connection.ExecuteAsync(@"
            INSERT OR IGNORE INTO system_config (key, value, description) VALUES
            ('sealed', 'false', '是否已封谱'),
            ('reset_code', '', '解封码哈希'),
            ('generation_character', '', '字辈谱'),
            ('family_description', '虹乡安定堂胡氏', '家族简介'),
            ('backup_enabled', 'true', '自动备份开关'),
            ('backup_interval_hours', '24', '备份间隔(小时)'),
            ('ocr_enabled', 'true', 'OCR开关'),
            ('theme_mode', 'light', '主题: light/dark'),
            ('date_format', 'gregorian', '历法展示: gregorian/lunar/ganzhi');
        ");
    }

    public SqliteConnection? GetConnection() => _connection;

    public void Dispose()
    {
        _connection?.Dispose();
        _connection = null;
    }
}

// 扩展方法
internal static class SqliteExtensions
{
    public static void ExecuteCommand(this SqliteConnection conn, string sql)
    {
        using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        cmd.ExecuteNonQuery();
    }

    public static Task ExecuteAsync(this SqliteConnection conn, string sql)
    {
        using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        return Task.Run(() => cmd.ExecuteNonQuery());
    }
}
