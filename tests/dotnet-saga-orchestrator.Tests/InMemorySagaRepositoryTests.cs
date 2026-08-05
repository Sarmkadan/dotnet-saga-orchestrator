using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SagaOrchestrator.Core.Domain.Enums;
using SagaOrchestrator.Core.Domain.Models;
using SagaOrchestrator.Data.Repositories;
using Xunit;

namespace SagaOrchestrator.Tests;

public class InMemorySagaRepositoryTests
{
    private static Saga CreateSampleSaga(string id, string correlationId, SagaStatus status, DateTime? startedAt = null)
    {
        return new Saga
        {
            Id = id,
            CorrelationId = correlationId,
            Definition = new SagaDefinition { Id = $"def-{id}" },
            Status = status,
            StartedAt = startedAt ?? DateTime.UtcNow
        };
    }

    [Fact]
    public async Task CreateAsync_ShouldAddAndReturnSaga()
    {
        // Arrange
        var repo = new InMemorySagaRepository();
        var saga = CreateSampleSaga("s1", "c1", SagaStatus.Pending);

        // Act
        var created = await repo.CreateAsync(saga);
        var fetched = await repo.GetByIdAsync("s1");

        // Assert
        Assert.NotNull(created);
        Assert.Equal(saga, created);
        Assert.Equal(saga, fetched);
    }

    [Fact]
    public async Task CreateAsync_DuplicateId_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var repo = new InMemorySagaRepository();
        var saga1 = CreateSampleSaga("dup", "c1", SagaStatus.Pending);
        var saga2 = CreateSampleSaga("dup", "c2", SagaStatus.Pending);
        await repo.CreateAsync(saga1);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => repo.CreateAsync(saga2));
    }

    [Fact]
    public async Task UpdateAsync_NonExisting_ShouldReturnNull()
    {
        // Arrange
        var repo = new InMemorySagaRepository();
        var saga = CreateSampleSaga("nonexistent", "cX", SagaStatus.Pending);

        // Act
        var result = await repo.UpdateAsync(saga);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_EmptyOrNullId_ShouldReturnFalse()
    {
        // Arrange
        var repo = new InMemorySagaRepository();

        // Act
        var resultNull = await repo.DeleteAsync(null!);
        var resultEmpty = await repo.DeleteAsync(string.Empty);

        // Assert
        Assert.False(resultNull);
        Assert.False(resultEmpty);
    }

    [Fact]
    public async Task GetByStatusAsync_ShouldReturnMatchingSagas()
    {
        // Arrange
        var repo = new InMemorySagaRepository();
        var sagaA = CreateSampleSaga("a", "cA", SagaStatus.Completed);
        var sagaB = CreateSampleSaga("b", "cB", SagaStatus.Failed);
        var sagaC = CreateSampleSaga("c", "cC", SagaStatus.Completed);
        await repo.CreateAsync(sagaA);
        await repo.CreateAsync(sagaB);
        await repo.CreateAsync(sagaC);

        // Act
        var completed = await repo.GetByStatusAsync(SagaStatus.Completed);

        // Assert
        Assert.Equal(2, completed.Count);
        Assert.Contains(sagaA, completed);
        Assert.Contains(sagaC, completed);
    }

    [Fact]
    public async Task SearchAsync_ShouldFilterByMultipleCriteria()
    {
        // Arrange
        var repo = new InMemorySagaRepository();
        var now = DateTime.UtcNow;
        var saga1 = CreateSampleSaga("1", "c1", SagaStatus.Pending, now.AddHours(-2));
        var saga2 = CreateSampleSaga("2", "c2", SagaStatus.Completed, now.AddHours(-1));
        var saga3 = CreateSampleSaga("3", "c3", SagaStatus.Pending, now);
        saga1.Definition = new SagaDefinition { Id = "def-1" };
        saga2.Definition = new SagaDefinition { Id = "def-2" };
        saga3.Definition = new SagaDefinition { Id = "def-1" };
        await repo.CreateAsync(saga1);
        await repo.CreateAsync(saga2);
        await repo.CreateAsync(saga3);

        var criteria = new Dictionary<string, object>
        {
            { "status", SagaStatus.Pending },
            { "definitionId", "def-1" },
            { "startDateFrom", now.AddHours(-3) },
            { "startDateTo", now.AddHours(-1) }
        };

        // Act
        var results = await repo.SearchAsync(criteria);

        // Assert
        Assert.Single(results);
        Assert.Equal(saga1, results[0]);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllStoredSagas()
    {
        // Arrange
        var repo = new InMemorySagaRepository();
        var sagaX = CreateSampleSaga("x", "cX", SagaStatus.Pending);
        var sagaY = CreateSampleSaga("y", "cY", SagaStatus.Failed);
        await repo.CreateAsync(sagaX);
        await repo.CreateAsync(sagaY);

        // Act
        var all = await repo.GetAllAsync();

        // Assert
        Assert.Equal(2, all.Count);
        Assert.Contains(sagaX, all);
        Assert.Contains(sagaY, all);
    }
}
