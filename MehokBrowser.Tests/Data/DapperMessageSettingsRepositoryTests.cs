using FluentAssertions;
using MeshokBrowser.Data;
using MeshokBrowser.Models;
using Xunit;

namespace MehokBrowser.Tests.Data;

/// <summary>
/// Тесты для DapperMessageSettingsRepository
/// </summary>
public class DapperMessageSettingsRepositoryTests
{
    [Fact]
    public void ConcreteType_ShouldReturn_MessagesSetType()
    {
        // Arrange
        var repository = new DapperMessageSettingsRepository();

        // Act
        var type = repository.ConcreteType;

        // Assert
        type.Should().Be(typeof(MessagesSet));
    }

    [Fact]
    public void KeyIntLookupList_ShouldReturn_EmptyArray()
    {
        // Arrange
        var repository = new DapperMessageSettingsRepository();

        // Act
        var result = repository.KeyIntLookupList();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public void KeyEntityLookupList_ShouldReturn_EmptyArray()
    {
        // Arrange
        var repository = new DapperMessageSettingsRepository();

        // Act
        var result = repository.KeyEntityLookupList();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public void ExequteQuery_ShouldThrow_NotSupportedException()
    {
        // Arrange
        var repository = new DapperMessageSettingsRepository();
        var sql = new[] { "SELECT * FROM test" };

        // Act
        Action act = () => repository.ExequteQuery(sql);

        // Assert
        act.Should().Throw<NotSupportedException>()
            .WithMessage("Raw SQL execution is not supported by the Dapper message repository.");
    }

    // TODO: Добавить интеграционные тесты с тестовой БД
    // TODO: Добавить тесты для async методов
    // TODO: Добавить тесты для SaveAsync, DeleteAsync
}
