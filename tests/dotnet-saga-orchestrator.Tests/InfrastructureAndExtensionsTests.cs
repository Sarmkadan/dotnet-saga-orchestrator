#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using SagaOrchestrator.Core.Domain.Enums;
using SagaOrchestrator.Core.Domain.Models;
using SagaOrchestrator.Core.Extensions;
using SagaOrchestrator.Data.Repositories;
using SagaOrchestrator.Infrastructure.Caching;
using FluentAssertions;
using Moq;
using Xunit;

namespace SagaOrchestrator.Tests;

public class StringExtensionsTests
{
    [Fact]
    public void ToSnakeCase_PascalCaseInput_InsertsUnderscoreBetweenWords()
    {
        "OrderProcessing".ToSnakeCase().Should().Be("order_processing");
    }

    [Fact]
    public void ToSnakeCase_SingleWord_ReturnsLowercase()
    {
        "Saga".ToSnakeCase().Should().Be("saga");
    }

    [Fact]
    public void ToKebabCase_PascalCaseInput_ReturnsHyphenSeparated()
    {
        "SagaOrchestrator".ToKebabCase().Should().Be("saga-orchestrator");
    }

    [Fact]
    public void ToCamelCase_PascalCase_LowercasesFirstCharacter()
    {
        "SagaOrchestrator".ToCamelCase().Should().Be("sagaOrchestrator");
    }

    [Fact]
    public void ToCamelCase_SingleCharacter_ReturnsLowercase()
    {
        "A".ToCamelCase().Should().Be("a");
    }

    [Fact]
    public void Truncate_StringLongerThanMax_AppendsEllipsis()
    {
        var result = "This is a very long description".Truncate(10);

        result.Should().Be("This is...");
        result.Length.Should().Be(10);
    }

    [Fact]
    public void Truncate_StringShorterThanMax_ReturnsOriginalUnchanged()
    {
        "Short".Truncate(20).Should().Be("Short");
    }

    [Fact]
    public void CountOccurrences_SubstringRepeatedMultipleTimes_ReturnsExactCount()
    {
        "saga:step:saga:comp:saga".CountOccurrences("saga").Should().Be(3);
    }

    [Fact]
    public void CountOccurrences_SubstringNotPresent_ReturnsZero()
    {
        "hello world".CountOccurrences("xyz").Should().Be(0);
    }

    [Fact]
    public void ToSlug_StringWithSpacesAndSpecialChars_ReturnsUrlFriendlySlug()
    {
        "Order Processing!".ToSlug().Should().Be("order-processing");
    }

    [Fact]
    public void ToSlug_EmptyString_ReturnsEmptyString()
    {
        string.Empty.ToSlug().Should().BeEmpty();
    }

    [Fact]
    public void RemovePrefix_PrefixPresent_RemovesPrefix()
    {
        "saga_abc123".RemovePrefix("saga_").Should().Be("abc123");
    }

    [Fact]
    public void RemovePrefix_PrefixAbsent_ReturnsOriginalValue()
    {
        "step_abc123".RemovePrefix("saga_").Should().Be("step_abc123");
    }

    [Fact]
    public void RemoveSuffix_SuffixPresent_RemovesSuffix()
    {
        "order-saga".RemoveSuffix("-saga").Should().Be("order");
    }

    [Fact]
    public void NullIfEmpty_EmptyString_ReturnsNull()
    {
        string.Empty.NullIfEmpty().Should().BeNull();
    }

    [Fact]
    public void NullIfEmpty_NonEmptyString_ReturnsSameValue()
    {
        "hello".NullIfEmpty().Should().Be("hello");
    }

    [Fact]
    public void Repeat_PositiveCount_ConcatenatesStringNTimes()
    {
        "ab".Repeat(3).Should().Be("ababab");
    }

    [Fact]
    public void Repeat_ZeroCount_ReturnsEmptyString()
    {
        "ab".Repeat(0).Should().BeEmpty();
    }

    [Fact]
    public void SplitAndTrim_StringWithSpacesAroundDelimiters_ReturnsTrimmedParts()
    {
        "  alpha , beta , gamma  ".SplitAndTrim(',').Should().BeEquivalentTo("alpha", "beta", "gamma");
    }
}

public class CollectionExtensionsTests
{
    [Fact]
    public void Batch_CollectionOfTen_ProducesCorrectBatchCount()
    {
        var items = Enumerable.Range(1, 10);

        var batches = items.Batch(3).ToList();

        batches.Should().HaveCount(4);
        batches[0].Should().HaveCount(3);
        batches[3].Should().HaveCount(1);
    }

    [Fact]
    public void Batch_EmptyCollection_ProducesNoBatches()
    {
        var batches = Enumerable.Empty<int>().Batch(5).ToList();

        batches.Should().BeEmpty();
    }

    [Fact]
    public void IsEmpty_NullCollection_ReturnsTrue()
    {
        IEnumerable<int>? items = null;

        items.IsEmpty().Should().BeTrue();
    }

    [Fact]
    public void IsEmpty_EmptyCollection_ReturnsTrue()
    {
        Enumerable.Empty<string>().IsEmpty().Should().BeTrue();
    }

    [Fact]
    public void IsNotEmpty_PopulatedCollection_ReturnsTrue()
    {
        new[] { 1, 2, 3 }.IsNotEmpty().Should().BeTrue();
    }

    [Fact]
    public void Paginate_SecondPageOfFive_ReturnsCorrectElements()
    {
        var items = Enumerable.Range(1, 20);

        var page = items.Paginate(2, 5).ToList();

        page.Should().HaveCount(5);
        page.First().Should().Be(6);
        page.Last().Should().Be(10);
    }

    [Fact]
    public void DistinctBy_CollectionWithDuplicateKeys_ReturnsOnlyFirstOccurrences()
    {
        var items = new[] { "apple", "apricot", "banana", "blueberry" };

        var result = items.DistinctBy(s => s[0]).ToList();

        result.Should().HaveCount(2);
        result.Should().Contain("apple");
        result.Should().Contain("banana");
    }

    [Fact]
    public void Window_CollectionOfFive_ProducesCorrectWindows()
    {
        var items = Enumerable.Range(1, 5);

        var windows = items.Window(3).ToList();

        windows.Should().HaveCount(3);
        windows[0].Should().BeEquivalentTo(new[] { 1, 2, 3 });
        windows[2].Should().BeEquivalentTo(new[] { 3, 4, 5 });
    }

    [Fact]
    public void ToQueryString_DictionaryWithParams_BuildsCorrectQueryString()
    {
        var parameters = new Dictionary<string, string>
        {
            ["status"] = "running",
            ["page"] = "1"
        };

        var queryString = parameters.ToQueryString();

        queryString.Should().Contain("status=running");
        queryString.Should().Contain("page=1");
        queryString.Should().Contain("&");
    }
}

public class CacheKeyBuilderTests
{
    [Fact]
    public void BuildSagaKey_ReturnsCorrectlyFormattedKey()
    {
        CacheKeyBuilder.BuildSagaKey("abc123").Should().Be("saga:abc123");
    }

    [Fact]
    public void BuildDefinitionKey_ReturnsCorrectlyFormattedKey()
    {
        CacheKeyBuilder.BuildDefinitionKey("def456").Should().Be("definition:def456");
    }

    [Fact]
    public void BuildCompensationKey_ReturnsCorrectlyFormattedKey()
    {
        CacheKeyBuilder.BuildCompensationKey("saga_001").Should().Be("compensation:saga_001");
    }

    [Fact]
    public void BuildRateLimitKey_CombinesIdentifierAndResource()
    {
        CacheKeyBuilder.BuildRateLimitKey("user123", "create-saga")
            .Should().Be("ratelimit:user123:create-saga");
    }

    [Fact]
    public void IsSagaKey_WithSagaPrefixedKey_ReturnsTrue()
    {
        CacheKeyBuilder.IsSagaKey("saga:abc123").Should().BeTrue();
    }

    [Fact]
    public void IsSagaKey_WithDefinitionKey_ReturnsFalse()
    {
        CacheKeyBuilder.IsSagaKey("definition:abc123").Should().BeFalse();
    }

    [Fact]
    public void IsDefinitionKey_WithDefinitionPrefixedKey_ReturnsTrue()
    {
        CacheKeyBuilder.IsDefinitionKey("definition:abc123").Should().BeTrue();
    }

    [Fact]
    public void ExtractIdFromKey_CompositeKey_ReturnsLastSegment()
    {
        CacheKeyBuilder.ExtractIdFromKey("saga:abc123").Should().Be("abc123");
    }

    [Fact]
    public void ExtractIdFromKey_KeyWithNoDelimiter_ReturnsEntireKey()
    {
        CacheKeyBuilder.ExtractIdFromKey("metrics").Should().Be("metrics");
    }

    [Fact]
    public void CacheExpiration_GetExpiration_ReturnsCorrectDurationPerType()
    {
        CacheExpiration.GetExpiration("definition").Should().Be(TimeSpan.FromHours(1));
        CacheExpiration.GetExpiration("saga").Should().Be(TimeSpan.FromMinutes(15));
        CacheExpiration.GetExpiration("metrics").Should().Be(TimeSpan.FromMinutes(5));
    }

    [Fact]
    public void CacheExpiration_GetExpiration_UnknownType_ReturnsMediumDefault()
    {
        CacheExpiration.GetExpiration("unknown-type").Should().Be(TimeSpan.FromMinutes(15));
    }
}

public class SagaRepositoryMockTests
{
    [Fact]
    public async Task GetByIdAsync_WhenSagaExists_ReturnsThatSaga()
    {
        var repositoryMock = new Mock<ISagaRepository>();
        var expectedSaga = new Saga { Id = "saga_test123", Status = SagaStatus.Running };

        repositoryMock
            .Setup(r => r.GetByIdAsync("saga_test123"))
            .ReturnsAsync(expectedSaga);

        var result = await repositoryMock.Object.GetByIdAsync("saga_test123");

        result.Should().NotBeNull();
        result!.Id.Should().Be("saga_test123");
        result.Status.Should().Be(SagaStatus.Running);
        repositoryMock.Verify(r => r.GetByIdAsync("saga_test123"), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WhenSagaDoesNotExist_ReturnsNull()
    {
        var repositoryMock = new Mock<ISagaRepository>();
        repositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((Saga?)null);

        var result = await repositoryMock.Object.GetByIdAsync("saga_missing");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByStatusAsync_WhenMockedWithRunningSagas_ReturnsFilteredList()
    {
        var repositoryMock = new Mock<ISagaRepository>();
        var runningSagas = new List<Saga>
        {
            new Saga { Status = SagaStatus.Running },
            new Saga { Status = SagaStatus.Running }
        };

        repositoryMock
            .Setup(r => r.GetByStatusAsync(SagaStatus.Running))
            .ReturnsAsync(runningSagas);

        var result = await repositoryMock.Object.GetByStatusAsync(SagaStatus.Running);

        result.Should().HaveCount(2);
        result.Should().AllSatisfy(s => s.Status.Should().Be(SagaStatus.Running));
        repositoryMock.Verify(r => r.GetByStatusAsync(SagaStatus.Running), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_VerifiesRepositoryCalledWithCorrectSaga()
    {
        var repositoryMock = new Mock<ISagaRepository>();
        var saga = new Saga { Id = "saga_new001" };

        repositoryMock
            .Setup(r => r.CreateAsync(It.Is<Saga>(s => s.Id == "saga_new001")))
            .ReturnsAsync(saga);

        var result = await repositoryMock.Object.CreateAsync(saga);

        result.Should().NotBeNull();
        result!.Id.Should().Be("saga_new001");
        repositoryMock.Verify(r => r.CreateAsync(It.Is<Saga>(s => s.Id == "saga_new001")), Times.Once);
    }
}
