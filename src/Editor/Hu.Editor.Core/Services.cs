using Hu.Editor.Models;

namespace Hu.Editor.Core.Services;

/// <summary>
/// 家谱数据服务 - 核心编纂逻辑
/// </summary>
public interface IGenealogyService
{
    Task<Person> GetPersonAsync(int id);
    Task<List<Person>> SearchPersonsAsync(string keyword);
    Task<List<Person>> GetAllPersonsAsync();
    Task<int> AddPersonAsync(Person person);
    Task UpdatePersonAsync(Person person);
    Task DeletePersonAsync(int id);
    Task<List<Person>> GetDescendantsAsync(int ancestorId);
    Task<List<Person>> GetAncestorsAsync(int personId);
    Task<List<Person>> GetSiblingsAsync(int personId);
    Task<List<Person>> GetSpousesAsync(int personId);
    Task<bool> IsSealedAsync();
    Task SealGenealogyAsync(string resetCode);
    Task UnsealGenealogyAsync(string resetCode);
    Task<List<Generation>> GetGenerationsAsync(int familyId);
    Task<int> GetGenerationAsync(int personId);
    Task<int> CalculateAgeAsync(string birthDate, string? deathDate);
}

/// <summary>
/// 世系树服务
/// </summary>
public interface ITreeService
{
    Task<List<TreeNode>> BuildTreeAsync(int familyId);
    Task<List<TreeNode>> GetSubtreeAsync(int personId, int levelsUp, int levelsDown);
    Task<string> ExportTreeAsync(int familyId, TreeFormat format);
}

public enum TreeFormat { Png, Svg, PdfVertical }

public class TreeNode
{
    public int PersonId { get; set; }
    public string Name { get; set; } = "";
    public int Generation { get; set; }
    public int? ParentId { get; set; }
    public List<int> ChildIds { get; set; } = [];
    public List<int> SpouseIds { get; set; } = [];
    public bool IsSelected { get; set; }
    public double Scale { get; set; } = 1.0;
}

/// <summary>
/// 封谱管理服务
/// </summary>
public interface ISealService
{
    Task<bool> IsSealedAsync();
    Task SealAsync(string resetCode);
    Task UnsealAsync(string resetCode);
    Task<string> GenerateResetCodeAsync();
    Task<string?> GetResetCodeHashAsync();
}

/// <summary>
/// 备份服务
/// </summary>
public interface IBackupService
{
    Task<string> CreateBackupAsync();
    Task RestoreFromBackupAsync(string backupPath);
    Task<List<string>> ListBackupsAsync();
    Task DeleteBackupAsync(string backupPath);
    Task<bool> AutoBackupIfNeededAsync();
}

/// <summary>
/// 历法转换服务
/// </summary>
public interface ICalefactionService
{
    /// <summary>
    /// 公历转农历
    /// </summary>
    string ToLunar(string gregorianDate);
    
    /// <summary>
    /// 公历转干支
    /// </summary>
    string ToGanZhi(string gregorianDate);
    
    /// <summary>
    /// 计算享年
    /// </summary>
    int? CalculateAge(string birthDate, string? deathDate);
    
    /// <summary>
    /// 格式化日期展示
    /// </summary>
    string FormatDate(string gregorianDate, DateFormat displayFormat);
}

/// <summary>
/// 导出服务
/// </summary>
public interface IExportService
{
    Task ExportPdfVerticalAsync(List<int> personIds, string outputPath);
    Task ExportTreeImageAsync(List<TreeNode> tree, string outputPath, ImageFormat format);
    Task ExportExcelAsync(List<Person> persons, string outputPath);
    Task ExportCsvAsync(List<Person> persons, string outputPath);
}

public enum ImageFormat { Png, Svg, Jpeg }

/// <summary>
/// 扫描服务
/// </summary>
public interface IScanService
{
    Task<byte[]> ScanDocumentAsync(int dpi);
    Task<List<byte[]>> ScanMultiPageAsync(int dpi);
    Task<bool> IsScannerAvailableAsync();
}

/// <summary>
/// 查重服务
/// </summary>
public interface IDuplicateDetectionService
{
    Task<List<DuplicateMatch>> FindDuplicatesAsync(Person person);
}

public class DuplicateMatch
{
    public Person Candidate { get; set; } = new();
    public double SimilarityScore { get; set; }
    public List<string> MatchFields { get; set; } = [];
}
