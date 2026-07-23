using FluentAssertions;
using Hu.Editor.Core;
using Hu.Editor.Models;

namespace Hu.Editor.Tests;

public class CalefactionServiceTests
{
    private readonly CalefactionService _service = new();

    [Fact]
    public void ToLunar_GivenGregorianDate_ReturnsLunarDateString()
    {
        // Arrange
        var gregorianDate = "2024-02-10"; // 2024年正月初一

        // Act
        var result = _service.ToLunar(gregorianDate);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("年");
    }

    [Fact]
    public void ToLunar_EmptyDate_ReturnsEmptyString()
    {
        // Act
        var result = _service.ToLunar("");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ToGanZhi_GivenValidDate_ReturnsGanZhiString()
    {
        // Arrange
        var gregorianDate = "2024-02-10";

        // Act
        var result = _service.ToGanZhi(gregorianDate);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().StartWith("[");
        result.Should().EndWith("]");
    }

    [Fact]
    public void CalculateAge_ValidDates_ReturnsCorrectAge()
    {
        // Arrange
        var birthDate = "1950-01-01";
        var deathDate = "2020-06-15";

        // Act
        var result = _service.CalculateAge(birthDate, deathDate);

        // Assert
        result.Should().Be(70);
    }

    [Fact]
    public void CalculateAge_DeathBeforeBirth_ReturnsZero()
    {
        // Act
        var result = _service.CalculateAge("2020-06-15", "1950-01-01");

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void CalculateAge_MissingDeathDate_ReturnsNull()
    {
        // Act
        var result = _service.CalculateAge("1950-01-01", "");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void FormatDate_GregorianFormat_ReturnsOriginalDate()
    {
        // Arrange
        var date = "2024-02-10";

        // Act
        var result = _service.FormatDate(date, DateFormat.Gregorian);

        // Assert
        result.Should().Be(date);
    }

    [Fact]
    public void FormatDate_LunarFormat_ReturnsLunarDate()
    {
        // Arrange
        var date = "2024-02-10";

        // Act
        var result = _service.FormatDate(date, DateFormat.Lunar);

        // Assert
        result.Should().NotBe(date);
        result.Should().Contain("年");
    }
}
