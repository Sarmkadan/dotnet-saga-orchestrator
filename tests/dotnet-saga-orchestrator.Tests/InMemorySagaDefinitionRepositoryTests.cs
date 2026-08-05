using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SagaOrchestrator.Core.Domain.Models;
using SagaOrchestrator.Data.Repositories;
using Xunit;

namespace SagaOrchestrator.Tests;

public class InMemorySagaDefinitionRepositoryTests
{
    private readonly InMemorySagaDefinitionRepository _repository;

    public InMemorySagaDefinitionRepositoryTests()
    {
        _repository = new InMemorySagaDefinitionRepository();
    }

    [Fact]
    public async Task CreateAsync_AddsDefinition()
    {
        var def = new SagaDefinition { Id = "1", Name = "Saga1" };
        var result = await _repository.CreateAsync(def);
        Assert.NotNull(result);
        Assert.Equal("1", result.Id);
    }

    [Fact]
    public async Task CreateAsync_ThrowsOnDuplicateId()
    {
        var def = new SagaDefinition { Id = "1" };
        await _repository.CreateAsync(def);
        await Assert.ThrowsAsync<InvalidOperationException>(() => _repository.CreateAsync(def));
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsDefinitionIfExists()
    {
        await _repository.CreateAsync(new SagaDefinition { Id = "1" });
        var result = await _repository.GetByIdAsync("1");
        Assert.NotNull(result);
        Assert.Equal("1", result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNullIfNotExists()
    {
        var result = await _repository.GetByIdAsync("nonexistent");
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByNameAsync_ReturnsDefinitionIfExists()
    {
        await _repository.CreateAsync(new SagaDefinition { Id = "1", Name = "Saga1" });
        var result = await _repository.GetByNameAsync("Saga1");
        Assert.NotNull(result);
        Assert.Equal("Saga1", result.Name);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesDefinition()
    {
        await _repository.CreateAsync(new SagaDefinition { Id = "1", Name = "OldName" });
        var updated = await _repository.UpdateAsync(new SagaDefinition { Id = "1", Name = "NewName" });
        Assert.NotNull(updated);
        Assert.Equal("NewName", updated.Name);
        
        var retrieved = await _repository.GetByIdAsync("1");
        Assert.Equal("NewName", retrieved?.Name);
    }

    [Fact]
    public async Task DeleteAsync_RemovesDefinition()
    {
        await _repository.CreateAsync(new SagaDefinition { Id = "1" });
        var deleted = await _repository.DeleteAsync("1");
        Assert.True(deleted);
        Assert.Null(await _repository.GetByIdAsync("1"));
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllDefinitions()
    {
        await _repository.CreateAsync(new SagaDefinition { Id = "1" });
        await _repository.CreateAsync(new SagaDefinition { Id = "2" });
        var all = await _repository.GetAllAsync();
        Assert.Equal(2, all.Count);
    }

    [Fact]
    public async Task GetActiveAsync_ReturnsOnlyActiveDefinitions()
    {
        await _repository.CreateAsync(new SagaDefinition { Id = "1", IsActive = true });
        await _repository.CreateAsync(new SagaDefinition { Id = "2", IsActive = false });
        var active = await _repository.GetActiveAsync();
        Assert.Single(active);
        Assert.True(active[0].IsActive);
    }

    [Fact]
    public async Task SearchAsync_FiltersByName()
    {
        await _repository.CreateAsync(new SagaDefinition { Id = "1", Name = "ABC" });
        await _repository.CreateAsync(new SagaDefinition { Id = "2", Name = "DEF" });
        var results = await _repository.SearchAsync(new Dictionary<string, object> { { "name", "AB" } });
        Assert.Single(results);
        Assert.Equal("ABC", results[0].Name);
    }
}
