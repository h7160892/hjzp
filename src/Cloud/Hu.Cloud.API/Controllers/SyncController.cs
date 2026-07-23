using Microsoft.AspNetCore.Mvc;
using Hu.Cloud.Models;

namespace Hu.Cloud.API.Controllers;

/// <summary>
/// 私有云 API - 数据中继服务
/// 仅提供接口，无网页管理后台
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SyncController : ControllerBase
{
    private readonly ILogger<SyncController> _logger;

    public SyncController(ILogger<SyncController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 用户登录 - 返回 Token
    /// </summary>
    [HttpPost("auth/login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        // TODO: 验证用户名密码，签发 JWT Token
        _logger.LogInformation("Login attempt for user: {Username}", request.Username);
        
        return Ok(new 
        { 
            Success = true, 
            Token = "jwt_token_placeholder",
            User = new { Id = 1, DisplayName = "胡某某", Role = "editor" }
        });
    }

    /// <summary>
    /// 提交采集数据
    /// </summary>
    [Authorize]
    [HttpPost("submission")]
    public IActionResult Submit([FromBody] SubmitRequest request)
    {
        // 数据进入待审表，不直接写入主谱
        _logger.LogInformation("Submission received from user {UserId}", 
            HttpContext.User.FindFirst("userId")?.Value);
        
        return Ok(new { Success = true, Message = "信息已提交，等待主编审核" });
    }

    /// <summary>
    /// 上传素材
    /// </summary>
    [Authorize]
    [HttpPost("material/upload")]
    public IActionResult UploadMaterial(IFormFile file)
    {
        // TODO: 存储到私有云文件服务，返回云路径
        var fileName = Path.GetFileName(file.FileName);
        _logger.LogInformation("Material uploaded: {FileName}", fileName);
        
        return Ok(new { Success = true, FileUrl = $"/materials/{fileName}" });
    }

    /// <summary>
    /// OCR 识别
    /// </summary>
    [Authorize]
    [HttpPost("ocr/recognize")]
    public IActionResult RecognizeText([FromBody] OcrRequest request)
    {
        // TODO: 调用 OCR 引擎识别
        return Ok(new 
        { 
            Success = true, 
            Text = "OCR识别结果占位", 
            Confidence = 0.95f 
        });
    }

    /// <summary>
    /// 获取可查阅的世系信息（仅自身及上下两代，脱敏）
    /// </summary>
    [Authorize]
    [HttpGet("tree/{personId}")]
    public IActionResult GetAccessibleTree(int personId, [FromQuery] int levels = 2)
    {
        // TODO: 查询世系树，限制范围，脱敏处理
        return Ok(new { Nodes = new List<object>() });
    }

    /// <summary>
    /// 获取我的提交状态
    /// </summary>
    [Authorize]
    [HttpGet("submissions/mine")]
    public IActionResult GetMySubmissions()
    {
        // TODO: 查询当前用户的提交记录
        return Ok(new List<SubmissionRecord>());
    }

    /// <summary>
    /// 主编端拉取待审数据
    /// </summary>
    [Authorize(Roles = "admin,room_admin")]
    [HttpGet("pending/pull")]
    public IActionResult PullPendingSubmissions()
    {
        // TODO: 返回所有待审提交
        return Ok(new { Submissions = new List<PendingSubmission>() });
    }

    /// <summary>
    /// 主编端审核通过
    /// </summary>
    [Authorize(Roles = "admin,room_admin")]
    [HttpPost("pending/{id}/approve")]
    public IActionResult ApproveSubmission(int id)
    {
        // TODO: 审核通过，数据写入主谱
        _logger.LogInformation("Submission {Id} approved", id);
        return Ok(new { Success = true, Message = "已采纳" });
    }

    /// <summary>
    /// 主编端驳回
    /// </summary>
    [Authorize(Roles = "admin,room_admin")]
    [HttpPost("pending/{id}/reject")]
    public IActionResult RejectSubmission(int id, [FromBody] RejectRequest request)
    {
        // TODO: 审核驳回
        return Ok(new { Success = true, Message = "已驳回" });
    }
}

// 请求模型
public class LoginRequest { public string Username { get; set; } = ""; public string Password { get; set; } = ""; }
public class SubmitRequest { public string PersonData { get; set; } = ""; public int SubmissionType { get; set; } }
public class OcrRequest { public byte[] Image { get; set; } = Array.Empty<byte>(); }
public class RejectRequest { public string Reason { get; set; } = ""; }
public class SubmissionRecord { public int Id { get; set; } public string Status { get; set; } = ""; }
public class PendingSubmission { public int Id { get; set; } public string Data { get; set; } = ""; }
