namespace Hu.Editor.Models;

/// <summary>
/// 人物角色枚举
/// </summary>
public enum Gender
{
    Female = 0,
    Male = 1
}

/// <summary>
/// 人物状态
/// </summary>
public enum PersonStatus
{
    Normal = 0,       // 正常
    Deceased = 1,     // 已故
    PendingReview = 2 // 待审核
}

/// <summary>
/// 关系标签类型
/// </summary>
public enum RelationshipTag
{
    None = 0,
    Adoption = 1,         // 过继
    Matrilocal = 2,       // 入赘
    ConcurrentHeir = 3,   // 兼祧
    FosterChild = 4,      // 收养
    NonHuMarried = 5,     // 外姓入谱
    Divorced = 6          // 离异
}

/// <summary>
/// 历法类型
/// </summary>
public enum DateFormat
{
    Gregorian = 0,  // 公历
    Lunar = 1,       // 农历
    GanZhi = 2       // 干支
}

/// <summary>
/// 用户角色
/// </summary>
public enum UserRole
{
    Admin = 0,           // 总管理员
    RoomAdmin = 1,       // 房头管理员
    Editor = 2,          // 编辑员
    ReadOnly = 3         // 只读用户
}

/// <summary>
/// 素材类型
/// </summary>
public enum MaterialType
{
    TombstonePhoto = 1,   // 墓碑照片
    OldPhoto = 2,          // 老照片
    OralHistory = 3,       // 口述录音
    ScanDocument = 4       // 扫描件
}

/// <summary>
/// 同步方向
/// </summary>
public enum SyncDirection
{
    Upload = 0,  // APP/Web → 云
    Download = 1 // 云 → EXE
}

/// <summary>
/// 待审提交状态
/// </summary>
public enum SubmissionStatus
{
    Pending = 0,    // 待审核
    Accepted = 1,   // 已采纳
    Rejected = 2,   // 已驳回
    NeedsSupplement = 3 // 需补充
}
