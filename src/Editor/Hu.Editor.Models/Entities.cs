using System.Text.Json.Serialization;

namespace Hu.Editor.Models;

/// <summary>
/// 人物模型 - 家谱核心数据实体
/// </summary>
public class Person
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? OriginalName { get; set; }        // 原名/字/号
    public Gender Gender { get; set; }
    public string BirthDate { get; set; } = "";       // 公历存储
    public string DeathDate { get; set; } = "";
    public string? BurialPlace { get; set; }
    public List<int> SpouseIds { get; set; } = [];
    public int? FatherId { get; set; }
    public int? MotherId { get; set; }
    public int? BirthOrder { get; set; }              // 排行
    public string? Education { get; set; }
    public string? Occupation { get; set; }
    public string? Honors { get; set; }               // 功名/荣誉
    public string? Residence { get; set; }
    public string? ContactPhone { get; set; }          // 脱敏存储
    public string? ContactWechat { get; set; }
    public string? PhotoPath { get; set; }
    public List<string> ScanPaths { get; set; } = [];
    public List<string> AudioPaths { get; set; } = [];
    public List<RelationshipTag> RelationshipTags { get; set; } = [];
    
    // 历法展示层
    public string? LunarBirth { get; set; }
    public string? GanZhiBirth { get; set; }
    public int? AgeAtDeath { get; set; }              // 享年（自动计算）
    
    public PersonStatus Status { get; set; } = PersonStatus.Normal;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
    public int Version { get; set; } = 1;
    
    // 导航属性
    public Person? Father { get; set; }
    public Person? Mother { get; set; }
    public List<Person> Children { get; set; } = [];
    public List<Person> Spouses { get; set; } = [];
    public Family? Family { get; set; }
}

/// <summary>
/// 家族/房头模型
/// </summary>
public class Family
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int? AncestorId { get; set; }
    public int GenerationCount { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
    
    public Person? Ancestor { get; set; }
    public List<Person> Members { get; set; } = [];
}

/// <summary>
/// 世代模型
/// </summary>
public class Generation
{
    public int Id { get; set; }
    public int FamilyId { get; set; }
    public int GenerationNumber { get; set; }
    public string? GenerationName { get; set; }
    public string? CharacterPattern { get; set; }     // 字辈谱
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

/// <summary>
/// 用户模型
/// </summary>
public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public UserRole Role { get; set; }
    public int? FamilyId { get; set; }
    public string? Phone { get; set; }
    public string? WechatId { get; set; }
    public string? DeviceId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
}

/// <summary>
/// 待审提交模型
/// </summary>
public class PendingSubmission
{
    public int Id { get; set; }
    public int SubmitterId { get; set; }
    public string PersonData { get; set; } = "";       // JSON
    public int? FamilyId { get; set; }
    public SubmissionType SubmissionType { get; set; }
    public SubmissionStatus Status { get; set; }
    public string? SupplementReason { get; set; }
    public int? ReviewerId { get; set; }
    public string? ReviewComment { get; set; }
    public bool SyncedToMain { get; set; }
    public DateTime SubmittedAt { get; set; } = DateTime.Now;
    public DateTime? ReviewedAt { get; set; }
}

/// <summary>
/// 素材模型
/// </summary>
public class Material
{
    public int Id { get; set; }
    public int PersonId { get; set; }
    public MaterialType MaterialType { get; set; }
    public string FilePath { get; set; } = "";
    public string? CloudHash { get; set; }
    public UploadStatus UploadStatus { get; set; }
    public string? OcrResult { get; set; }
    public bool OcrConfirmed { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

/// <summary>
/// 审计日志模型
/// </summary>
public class AuditLog
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Action { get; set; } = "";
    public string? TargetType { get; set; }
    public int? TargetId { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string? IpAddress { get; set; }
    public string? DeviceInfo { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

/// <summary>
/// 同步队列模型
/// </summary>
public class SyncQueueItem
{
    public int Id { get; set; }
    public SyncDirection Direction { get; set; }
    public string EntityType { get; set; } = "";
    public int? EntityId { get; set; }
    public string Payload { get; set; } = "";
    public SyncStatus Status { get; set; }
    public int RetryCount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? SyncedAt { get; set; }
}

// 补充枚举
public enum SubmissionType { Add = 1, Modify = 2, MaterialUpload = 3 }
public enum UploadStatus { NotUploaded = 0, Uploaded = 1 }
public enum SyncStatus { Pending = 0, Synced = 1, Failed = 2 }
