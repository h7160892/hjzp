using Hu.Cloud.Models;

namespace Hu.Cloud.Sync.Services;

/// <summary>
/// 增量同步引擎
/// 处理 EXE ↔ 云 ↔ APP/Web 之间的数据同步
/// </summary>
public class IncrementalSyncEngine
{
    private readonly ILogger<IncrementalSyncEngine> _logger;

    public IncrementalSyncEngine(ILogger<IncrementalSyncEngine> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// EXE 端主动拉取待审数据
    /// </summary>
    public async Task<List<PendingSubmission>> PullPendingSubmissionsAsync(
        string editorId, DateTime lastSyncTime)
    {
        _logger.LogInformation("Editor {EditorId} pulling pending submissions since {LastSync}",
            editorId, lastSyncTime);

        // TODO: 查询数据库中待审记录
        return [];
    }

    /// <summary>
    /// 审核通过后写入主谱
    /// </summary>
    public async Task<bool> WriteToMainGenealogyAsync(int submissionId, string editorId)
    {
        _logger.LogInformation("Writing submission {SubmissionId} to main genealogy by editor {Editor}",
            submissionId, editorId);

        // TODO: 1. 解析 submission 的 JSON 数据
        // TODO: 2. 执行事务写入 persons 表
        // TODO: 3. 更新 pending_submissions 状态
        // TODO: 4. 记录审计日志
        return true;
    }

    /// <summary>
    /// 处理离线队列中的待同步数据
    /// </summary>
    public async Task ProcessSyncQueueAsync()
    {
        _logger.LogInformation("Processing sync queue...");

        // TODO: 1. 读取 sync_queue 表中 status=0 的记录
        // TODO: 2. 按 direction 分类处理
        // TODO: 3. 发送 HTTP 请求到私有云 API
        // TODO: 4. 更新 status 和 retry_count
        // TODO: 5. 超过最大重试次数标记为失败
    }

    /// <summary>
    /// 冲突检测与解决
    /// 以 EXE 主编端数据为准
    /// </summary>
    public async Task<ConflictResolution> ResolveConflictAsync(
        int personId, string editorData, string collectorData)
    {
        // 冲突处理策略：
        // 1. 如果主编端数据已封谱 → 以主编端为准，采集端数据转入待审
        // 2. 如果未封谱 → 比较版本号，高者胜出
        // 3. 如果版本号相同 → 标记需要人工确认
        
        return new ConflictResolution
        {
            ResolvedBy = "editor", // 默认以主编端为准
            WinnerData = editorData,
            LoserData = collectorData,
            NeedsManualReview = false
        };
    }
}

public class ConflictResolution
{
    public string ResolvedBy { get; set; } = "";
    public string WinnerData { get; set; } = "";
    public string LoserData { get; set; } = "";
    public bool NeedsManualReview { get; set; }
}
