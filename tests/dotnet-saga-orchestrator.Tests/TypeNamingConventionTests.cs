#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Reflection;
using System.Text.RegularExpressions;
using FluentAssertions;
using Xunit;

namespace SagaOrchestrator.Tests;

/// <summary>
/// Guards against a codegen feedback-loop bug where a generator runs twice over its own
/// output and produces types with a doubled suffix, e.g. <c>FooJsonExtensionsJsonExtensions</c>
/// or <c>FooExtensionsExtensions</c>.
/// </summary>
public class TypeNamingConventionTests
{
    /// <summary>
    /// Matches a public type name that ends with the same non-empty word repeated twice
    /// back to back (case-sensitive), such as <c>JsonExtensionsJsonExtensions</c> or
    /// <c>ExtensionsExtensions</c>.
    /// </summary>
    private static readonly Regex RepeatedSuffixPattern = new(@"([A-Z][a-zA-Z0-9]*?)\1$", RegexOptions.Compiled);

    /// <summary>
    /// Verifies that no public type in the production or test assemblies has a name
    /// containing an immediately repeated suffix, which is the signature of a runaway
    /// codegen pass generating extensions on top of already-generated extensions.
    /// </summary>
    [Fact]
    public void PublicTypes_DoNotHaveRepeatedSuffixNames()
    {
        var assemblies = new[]
        {
            typeof(SagaOrchestrator.Core.Extensions.StringExtensions).Assembly,
            typeof(TypeNamingConventionTests).Assembly,
        };

        var offenders = assemblies
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.IsPublic || type.IsNestedPublic)
            .Where(type => RepeatedSuffixPattern.IsMatch(type.Name))
            .Select(type => type.FullName ?? type.Name)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToList();

        offenders.Should().BeEmpty(
            "type names must not contain an immediately repeated suffix (e.g. 'XJsonExtensionsJsonExtensions'), " +
            "which indicates a codegen pass ran twice over its own output");
    }
}
