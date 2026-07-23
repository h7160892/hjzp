using Hu.Editor.Core.Data;
using Hu.Editor.Models;

namespace Hu.Editor.Core.Services;

/// <summary>
/// 历法转换服务实现
/// 底层统一存储公历，展示层支持公历/农历/干支自由切换
/// </summary>
public class CalefactionService : ICalefactionService
{
    /// <summary>
    /// 公历转农历（简化实现，实际使用 ChineseLunisolarCalendar）
    /// </summary>
    public string ToLunar(string gregorianDate)
    {
        if (string.IsNullOrWhiteSpace(gregorianDate)) return "";

        try
        {
            var date = DateTime.Parse(gregorianDate);
            var chineseCal = new System.Globalization.ChineseLunisolarCalendar();
            
            var year = chineseCal.GetYear(date);
            var month = chineseCal.GetMonth(date);
            var day = chineseCal.GetDayOfMonth(date);
            
            // 闰月处理
            var isLeapMonth = chineseCal.GetLeapMonth(date.Year) == month;
            var leapPrefix = isLeapMonth ? "闰" : "";
            
            var lunarMonths = new[] { "", "正", "二", "三", "四", "五", "六", 
                                       "七", "八", "九", "十", "冬", "腊" };
            var lunarDays = new[] { "", "初一", "初二", "初三", "初四", "初五",
                                     "初六", "初七", "初八", "初九", "初十",
                                     "十一", "十二", "十三", "十四", "十五",
                                     "十六", "十七", "十八", "十九", "二十",
                                     "廿一", "廿二", "廿三", "廿四", "廿五",
                                     "廿六", "廿七", "廿八", "廿九", "三十" };
            
            return $"{year}年{leapPrefix}{lunarMonths[month]}{lunarDays[day]}";
        }
        catch
        {
            return gregorianDate;
        }
    }

    /// <summary>
    /// 公历转干支
    /// </summary>
    public string ToGanZhi(string gregorianDate)
    {
        if (string.IsNullOrWhiteSpace(gregorianDate)) return "";

        try
        {
            var date = DateTime.Parse(gregorianDate);
            var yearGanZhi = GetYearGanZhi(date.Year);
            
            // 月份干支（简化）
            var monthGanZhi = GetMonthGanZhi(date.Month, date.Year);
            // 日干支（简化）
            var dayGanZhi = GetDayGanZhi(date);
            
            return $"[{yearGanZhi}年 {monthGanZhi}月 {dayGanZhi}日]";
        }
        catch
        {
            return "";
        }
    }

    private string GetYearGanZhi(int year)
    {
        var gan = new[] { "庚", "辛", "壬", "癸", "甲", "乙", "丙", "丁", "戊", "己" };
        var zhi = new[] { "申", "酉", "戌", "亥", "子", "丑", "寅", "卯", "辰", "巳", "午", "未" };
        
        int ganIndex = (year - 4) % 10;
        int zhiIndex = (year - 4) % 12;
        
        if (ganIndex < 0) ganIndex += 10;
        if (zhiIndex < 0) zhiIndex += 12;
        
        return $"{gan[ganIndex]}{zhi[zhiIndex]}";
    }

    private string GetMonthGanZhi(int month, int year)
    {
        var monthZhi = new[] { "", "寅", "卯", "辰", "巳", "午", "未", 
                                "申", "酉", "戌", "亥", "子", "丑" };
        var ganBase = new[] { "甲", "丙", "戊", "庚", "壬" };
        
        int yearGanIndex = (year - 4) % 10;
        if (yearGanIndex < 0) yearGanIndex += 10;
        int ganOffset = Array.IndexOf(ganBase, ganBase[yearGanIndex % 5]);
        
        int monthGanIndex = (ganOffset * 2 + month - 1) % 10;
        
        return $"{gan[monthGanIndex]}{monthZhi[month]}";
    }

    private string GetDayGanZhi(DateTime date)
    {
        var gan = new[] { "甲", "乙", "丙", "丁", "戊", "己", "庚", "辛", "壬", "癸" };
        var zhi = new[] { "子", "丑", "寅", "卯", "辰", "巳", "午", "未", "申", "酉", "戌", "亥" };
        
        // 基准日：1900-01-31 = 甲子日
        var baseDate = new DateTime(1900, 1, 31);
        var daysSinceBase = (date - baseDate).Days;
        
        return $"{gan[(daysSinceBase % 10 + 10) % 10]}{zhi[(daysSinceBase % 12 + 12) % 12]}";
    }

    /// <summary>
    /// 计算享年
    /// </summary>
    public int? CalculateAge(string birthDate, string? deathDate)
    {
        if (string.IsNullOrWhiteSpace(birthDate) || string.IsNullOrWhiteSpace(deathDate ?? ""))
            return null;

        try
        {
            var birth = DateTime.Parse(birthDate);
            var death = DateTime.Parse(deathDate);
            var age = death.Year - birth.Year;
            if (death.Month < birth.Month || 
                (death.Month == birth.Month && death.Day < birth.Day))
                age--;
            return Math.Max(0, age);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 格式化日期展示
    /// </summary>
    public string FormatDate(string gregorianDate, DateFormat displayFormat)
    {
        if (string.IsNullOrWhiteSpace(gregorianDate)) return "";

        return displayFormat switch
        {
            DateFormat.Lunar => ToLunar(gregorianDate),
            DateFormat.GanZhi => ToGanZhi(gregorianDate),
            _ => gregorianDate
        };
    }
}
