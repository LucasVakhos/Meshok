using FluentAssertions;
using MehokBrowser.Services;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace MehokBrowser.Tests.Services;

/// <summary>
/// Тесты для ConnectionStringProvider
/// </summary>
public class ConnectionStringProviderTests
{
    [Fact]
    public void Constructor_WithNullConfiguration_ShouldThrow_ArgumentNullException()
    {
        // Act
        Action act = () => new ConnectionStringProvider(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("configuration");
    }

    [Fact]
    public void GetFirebirdConnectionString_WithValidConfig_ShouldReturn_ConnectionString()
    {
        // Arrange
        const string expectedConnectionString = "DataSource=localhost;Database=test.fdb;User=SYSDBA;";
        var configMock = new Mock<IConfiguration>();
        configMock.Setup(c => c.GetSection("ConnectionStrings")["Firebird"])
            .Returns(expectedConnectionString);

        var provider = new ConnectionStringProvider(configMock.Object);

        // Act
        var result = provider.GetFirebirdConnectionString();

        // Assert
        result.Should().Be(expectedConnectionString);
    }

    [Fact]
    public void GetFirebirdConnectionString_WithMissingConfig_ShouldThrow_InvalidOperationException()
    {
        // Arrange
        var configMock = new Mock<IConfiguration>();
        configMock.Setup(c => c.GetSection("ConnectionStrings")["Firebird"])
            .Returns((string?)null);

        var provider = new ConnectionStringProvider(configMock.Object);

        // Act
        Action act = () => provider.GetFirebirdConnectionString();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Firebird connection string is not configured in appsettings.json");
    }

    [Fact]
    public void GetMySqlConnectionString_WithValidConfig_ShouldReturn_ConnectionString()
    {
        // Arrange
        const string expectedConnectionString = "Server=localhost;Database=test;Uid=root;";
        var configMock = new Mock<IConfiguration>();
        configMock.Setup(c => c.GetSection("ConnectionStrings")["MySQL"])
            .Returns(expectedConnectionString);

        var provider = new ConnectionStringProvider(configMock.Object);

        // Act
        var result = provider.GetMySqlConnectionString();

        // Assert
        result.Should().Be(expectedConnectionString);
    }

    [Fact]
    public void GetMySqlConnectionString_WithMissingConfig_ShouldThrow_InvalidOperationException()
    {
        // Arrange
        var configMock = new Mock<IConfiguration>();
        configMock.Setup(c => c.GetSection("ConnectionStrings")["MySQL"])
            .Returns((string?)null);

        var provider = new ConnectionStringProvider(configMock.Object);

        // Act
        Action act = () => provider.GetMySqlConnectionString();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("MySQL connection string is not configured in appsettings.json");
    }
}
