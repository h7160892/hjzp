using Hu.Collector.Shared.Models;
using Newtonsoft.Json;

namespace Hu.Collector.Shared.Services;

/// <summary>
/// 采集端统一数据服务
/// 仅用于信息收集，无编纂权
/// </summary>
public class CollectorDataService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private string? _token;

    public CollectorDataService(string baseUrl)
    {
        _baseUrl = baseUrl;
        _httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
    }

    /// <summary>
    /// 用户登录
    /// </summary>
    public async Task<AuthResult> LoginAsync(string username, string password)
    {
        var request = new LoginRequest { Username = username, Password = password };
        var content = new StringContent(JsonConvert.SerializeObject(request), 
                                         Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PostAsync("/api/auth/login", content);
        var json = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<AuthResult>(json) ?? new AuthResult { Success = false };
    }

    /// <summary>
    /// 提交采集数据
    /// </summary>
    public async Task<SubmitResult> SubmitPersonAsync(PersonFormData formData)
    {
        EnsureToken();
        var request = new SubmitRequest
        {
            PersonData = JsonConvert.SerializeObject(formData),
            SubmissionType = 1 // 新增
        };
        
        var content = new StringContent(JsonConvert.SerializeObject(request), 
                                         Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("/api/submission", content);
        
        var json = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<SubmitResult>(json) ?? 
               new SubmitResult { Success = false, Message = "提交失败" };
    }

    /// <summary>
    /// 上传素材（照片/录音）
    /// </summary>
    public async Task<UploadResult> UploadMaterialAsync(Stream fileStream, 
                                                       string fileName, 
                                                       MaterialType type)
    {
        EnsureToken();
        var multipart = new MultipartFormDataContent();
        var streamContent = new StreamContent(fileStream);
        streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(
            type == MaterialType.Audio ? "audio/mp3" : "image/jpeg");
        multipart.Add(streamContent, "file", fileName);
        
        var response = await _httpClient.PostAsync("/api/material/upload", multipart);
        var json = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<UploadResult>(json) ?? 
               new UploadResult { Success = false };
    }

    /// <summary>
    /// 查看我的提交状态
    /// </summary>
    public async Task<List<SubmissionRecord>> GetMySubmissionsAsync()
    {
        EnsureToken();
        var response = await _httpClient.GetAsync("/api/submissions/mine");
        var json = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<List<SubmissionRecord>>(json) ?? [];
    }

    /// <summary>
    /// 查阅公开世系信息（仅自身及上下两代）
    /// </summary>
    public async Task<TreeViewData> GetAccessibleTreeAsync(int personId, int levels)
    {
        EnsureToken();
        var response = await _httpClient.GetAsync($"/api/tree/{personId}?levels={levels}");
        var json = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<TreeViewData>(json) ?? new TreeViewData();
    }

    /// <summary>
    /// OCR 辅助识别图片文字
    /// </summary>
    public async Task<OcrResult> RecognizeTextAsync(byte[] imageBytes)
    {
        EnsureToken();
        var content = new ByteArrayContent(imageBytes);
        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
        
        var response = await _httpClient.PostAsync("/api/ocr/recognize", content);
        var json = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<OcrResult>(json) ?? new OcrResult();
    }

    private void EnsureToken()
    {
        if (_token == null)
            throw new UnauthorizedAccessException("请先登录");
        _httpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);
    }
}

// ===== 数据传输模型 =====

public class AuthResult
{
    public bool Success { get; set; }
    public string? Token { get; set; }
    public string? Message { get; set; }
    public UserInfo? User { get; set; }
}

public class UserInfo
{
    public int Id { get; set; }
    public string DisplayName { get; set; } = "";
    public string Role { get; set; } = "";
}

public class LoginRequest
{
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
}

public class SubmitRequest
{
    public string PersonData { get; set; } = "";
    public int SubmissionType { get; set; }
}

public class SubmitResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public int? SubmissionId { get; set; }
}

public class UploadResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? FileUrl { get; set; }
}

public class SubmissionRecord
{
    public int Id { get; set; }
    public string Status { get; set; } = "";  // 待审核/已采纳/需补充
    public string? SupplementReason { get; set; }
    public DateTime SubmittedAt { get; set; }
}

public class TreeViewData
{
    public List<TreeNodeData> Nodes { get; set; } = [];
}

public class TreeNodeData
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? BirthDate { get; set; }
    public string? DeathDate { get; set; }
    public int? ParentId { get; set; }
    public List<int> ChildIds { get; set; } = [];
    public bool IsPublic { get; set; }  // 是否脱敏展示
}

public class OcrResult
{
    public bool Success { get; set; }
    public string? Text { get; set; }
    public float Confidence { get; set; }
}

public enum MaterialType { Photo, Audio }
