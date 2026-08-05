using FluentAssertions;
using MehokBrowser.Services;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace MehokBrowser.Tests.Services;

/// <summary>
/// Тесты для ConfigurationService
/// </summary>
public class ConfigurationServiceTests
{
    [Fact]
    public void Constructor_WithNullConfiguration_ShouldThrow_ArgumentNullException()
    {
        // Act
        Action act = () => new ConfigurationService(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("configuration");
    }

    [Fact]
    public void ApplicationName_WithValidConfig_ShouldReturn_Name()
    {
        // Arrange
        const string expectedName = "TestApp";
        var configMock = new Mock<IConfiguration>();
        configMock.Setup(c => c["Application:Name"]).Returns(expectedName);

        var service = new ConfigurationService(configMock.Object);

        // Act
        var result = service.ApplicationName;

        // Assert
        result.Should().Be(expectedName);
    }

    [Fact]
    public void ApplicationName_WithMissingConfig_ShouldReturn_DefaultName()
    {
        // Arrange
        var configMock = new Mock<IConfiguration>();
        configMock.Setup(c => c["Application:Name"]).Returns((string?)null);

        var service = new ConfigurationService(configMock.Object);

        // Act
        var result = service.ApplicationName;

        // Assert
        result.Should().Be("MehokBrowser");
    }

    [Fact]
    public void ApplicationVersion_WithValidConfig_ShouldReturn_Version()
    {
        // Arrange
        const string expectedVersion = "2.0.0";
        var configMock = new Mock<IConfiguration>();
        configMock.Setup(c => c["Application:Version"]).Returns(expectedVersion);

        var service = new ConfigurationService(configMock.Object);

        // Act
        var result = service.ApplicationVersion;

        // Assert
        result.Should().Be(expectedVersion);
    }

    [Fact]
    public void ApplicationVersion_WithMissingConfig_ShouldReturn_DefaultVersion()
    {
        // Arrange
        var configMock = new Mock<IConfiguration>();
        configMock.Setup(c => c["Application:Version"]).Returns((string?)null);

        var service = new ConfigurationService(configMock.Object);

        // Act
        var result = service.ApplicationVersion;

        // Assert
        result.Should().Be("1.0.0");
    }

    [Fact]
    public void GetValue_WithExistingKey_ShouldReturn_Value()
    {
        // Arrange
        const string key = "TestKey";
        const int expectedValue = 42;
        var configMock = new Mock<IConfiguration>();
        configMock.Setup(c => c.GetValue<int>(key, It.IsAny<int>())).Returns(expectedValue);

        var service = new ConfigurationService(configMock.Object);

        // Act
        var result = service.GetValue(key, 0);

        // Assert
        result.Should().Be(expectedValue);
    }

    [Fact]
    public void GetValue_WithMissingKey_ShouldReturn_DefaultValue()
    {
        // Arrange
        const string key = "MissingKey";
        const int defaultValue = 99;
        var configMock = new Mock<IConfiguration>();
        configMock.Setup(c => c.GetValue<int>(key, defaultValue)).Returns(defaultValue);

        var service = new ConfigurationService(configMock.Object);

        // Act
        var result = service.GetValue(key, defaultValue);

        // Assert
        result.Should().Be(defaultValue);
    }
}
