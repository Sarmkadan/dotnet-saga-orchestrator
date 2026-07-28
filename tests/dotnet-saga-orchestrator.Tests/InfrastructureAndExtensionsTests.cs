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

/// <summary>
/// Unit tests for the <see cref="SagaOrchestrator.Core.Extensions.StringExtensions"/> class.
/// Tests string extension methods for common string transformations and manipulations.
/// </summary>
public class StringExtensionsTests
{
    /// <summary>
    /// Tests that <see cref="string.ToSnakeCase()"/> correctly converts PascalCase strings to snake_case by inserting underscores between words.
    /// </summary>
    [Fact]
    public void ToSnakeCase_PascalCaseInput_InsertsUnderscoreBetweenWords()
    {
        "OrderProcessing".ToSnakeCase().Should().Be("order_processing");
    }

    /// <summary>
    /// Tests that <see cref="string.ToSnakeCase()"/> correctly converts single-word strings to lowercase snake_case.
    /// </summary>
    [Fact]
    public void ToSnakeCase_SingleWord_ReturnsLowercase()
    {
        "Saga".ToSnakeCase().Should().Be("saga");
    }

    /// <summary>
    /// Tests that <see cref="string.ToKebabCase()"/> correctly converts PascalCase strings to kebab-case by inserting hyphens between words.
    /// </summary>
    [Fact]
    public void ToKebabCase_PascalCaseInput_ReturnsHyphenSeparated()
    {
        "SagaOrchestrator".ToKebabCase().Should().Be("saga-orchestrator");
    }

    /// <summary>
    /// Tests that <see cref="string.ToCamelCase()"/> correctly converts PascalCase strings to camelCase by lowercasing the first character.
    /// </summary>
    [Fact]
    public void ToCamelCase_PascalCase_LowercasesFirstCharacter()
    {
        "SagaOrchestrator".ToCamelCase().Should().Be("sagaOrchestrator");
    }

    /// <summary>
    /// Tests that <see cref="string.ToCamelCase()"/> correctly converts single character strings to lowercase.
    /// </summary>
    [Fact]
    public void ToCamelCase_SingleCharacter_ReturnsLowercase()
    {
        "A".ToCamelCase().Should().Be("a");
    }

    /// <summary>
    /// Tests that <see cref="string.Truncate(int)"/> correctly truncates strings longer than the specified maximum length and appends an ellipsis.
    /// </summary>
    [Fact]
    public void Truncate_StringLongerThanMax_AppendsEllipsis()
    {
        var result = "This is a very long description".Truncate(10);

        result.Should().Be("This is...");
        result.Length.Should().Be(10);
    }

    /// <summary>
    /// Tests that <see cref="string.Truncate(int)"/> returns the original string unchanged when it is shorter than the specified maximum length.
    /// </summary>
    [Fact]
    public void Truncate_StringShorterThanMax_ReturnsOriginalUnchanged()
    {
        "Short".Truncate(20).Should().Be("Short");
    }

    /// <summary>
    /// Tests that <see cref="string.CountOccurrences(string)"/> correctly counts the number of times a substring appears in a string.
    /// </summary>
    [Fact]
    public void CountOccurrences_SubstringRepeatedMultipleTimes_ReturnsExactCount()
    {
        "saga:step:saga:comp:saga".CountOccurrences("saga").Should().Be(3);
    }

    /// <summary>
    /// Tests that <see cref="string.CountOccurrences(string)"/> returns zero when the substring is not present in the string.
    /// </summary>
    [Fact]
    public void CountOccurrences_SubstringNotPresent_ReturnsZero()
    {
        "hello world".CountOccurrences("xyz").Should().Be(0);
    }

    /// <summary>
    /// Tests that <see cref="string.ToSlug()"/> converts strings with spaces and special characters to URL-friendly slug format.
    /// </summary>
    [Fact]
    public void ToSlug_StringWithSpacesAndSpecialChars_ReturnsUrlFriendlySlug()
    {
        "Order Processing!".ToSlug().Should().Be("order-processing");
    }

    /// <summary>
    /// Tests that <see cref="string.ToSlug()"/> returns an empty string when the input is empty.
    /// </summary>
    [Fact]
    public void ToSlug_EmptyString_ReturnsEmptyString()
    {
        string.Empty.ToSlug().Should().BeEmpty();
    }

    /// <summary>
    /// Tests that <see cref="string.RemovePrefix(string)"/> removes the specified prefix from the string when present.
    /// </summary>
    [Fact]
    public void RemovePrefix_PrefixPresent_RemovesPrefix()
    {
        "saga_abc123".RemovePrefix("saga_").Should().Be("abc123");
    }

    /// <summary>
    /// Tests that <see cref="string.RemovePrefix(string)"/> returns the original string unchanged when the prefix is not present.
    /// </summary>
    [Fact]
    public void RemovePrefix_PrefixAbsent_ReturnsOriginalValue()
    {
        "step_abc123".RemovePrefix("saga_").Should().Be("step_abc123");
    }

    /// <summary>
    /// Tests that <see cref="string.RemoveSuffix(string)"/> removes the specified suffix from the string when present.
    /// </summary>
    [Fact]
    public void RemoveSuffix_SuffixPresent_RemovesSuffix()
    {
        "order-saga".RemoveSuffix("-saga").Should().Be("order");
    }

    /// <summary>
    /// Tests that <see cref="string.NullIfEmpty()"/> returns null when the string is empty.
    /// </summary>
    [Fact]
    public void NullIfEmpty_EmptyString_ReturnsNull()
    {
        string.Empty.NullIfEmpty().Should().BeNull();
    }

    /// <summary>
    /// Tests that <see cref="string.NullIfEmpty()"/> returns the original string unchanged when it is not empty.
    /// </summary>
    [Fact]
    public void NullIfEmpty_NonEmptyString_ReturnsSameValue()
    {
        "hello".NullIfEmpty().Should().Be("hello");
    }

    /// <summary>
    /// Tests that <see cref="string.Repeat(int)"/> concatenates the string the specified number of times.
    /// </summary>
    [Fact]
    public void Repeat_PositiveCount_ConcatenatesStringNTimes()
    {
        "ab".Repeat(3).Should().Be("ababab");
    }

    /// <summary>
    /// Tests that <see cref="string.Repeat(int)"/> returns an empty string when the count is zero.
    /// </summary>
    [Fact]
    public void Repeat_ZeroCount_ReturnsEmptyString()
    {
        "ab".Repeat(0).Should().BeEmpty();
    }

    /// <summary>
    /// Tests that <see cref="string.SplitAndTrim(char)"/> splits a string by the specified delimiter and trims whitespace from each resulting part.
    /// </summary>
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

        var result = SagaOrchestrator.Core.Extensions.CollectionExtensions.DistinctBy(items, s => s[0]).ToList();

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
