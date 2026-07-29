using Xunit;

namespace Game.Tests;

/// <summary>
/// Gate A: Domain and Application must not reference Unity assemblies.
/// </summary>
public sealed class DependencyAuditTests
{
    private static readonly string[] ForbiddenPrefixes =
    [
        "UnityEngine",
        "UnityEditor",
        "TMPro",
    ];

    [Theory]
    [InlineData(typeof(Game.Domain.Inventory.Inventory))]
    [InlineData(typeof(Game.Application.AssemblyMarker))]
    public void Assembly_HasNoUnityReferences(Type typeFromAssembly)
    {
        var assembly = typeFromAssembly.Assembly;
        var referenced = assembly.GetReferencedAssemblies().Select(a => a.Name ?? string.Empty);

        var forbidden = referenced
            .Where(name => ForbiddenPrefixes.Any(prefix =>
                name.Equals(prefix, StringComparison.OrdinalIgnoreCase)
                || name.StartsWith(prefix + ".", StringComparison.OrdinalIgnoreCase)))
            .ToArray();

        Assert.True(
            forbidden.Length == 0,
            $"Assembly '{assembly.GetName().Name}' references forbidden Unity assemblies: {string.Join(", ", forbidden)}");
    }

    [Fact]
    public void DomainSource_HasNoUnityUsingDirectives()
    {
        var domainRoot = FindSiblingPath("src", "Game.Domain");
        var offenders = Directory
            .EnumerateFiles(domainRoot, "*.cs", SearchOption.AllDirectories)
            .SelectMany(path => File.ReadAllLines(path)
                .Select((line, index) => (path, line, index: index + 1)))
            .Where(x => LooksLikeUnityUsing(x.line))
            .Select(x => $"{x.path}:{x.index}: {x.line.Trim()}")
            .ToArray();

        Assert.True(offenders.Length == 0, string.Join(Environment.NewLine, offenders));
    }

    private static bool LooksLikeUnityUsing(string line)
    {
        var trimmed = line.TrimStart();
        return trimmed.StartsWith("using UnityEngine", StringComparison.Ordinal)
               || trimmed.StartsWith("using UnityEditor", StringComparison.Ordinal)
               || trimmed.StartsWith("using TMPro", StringComparison.Ordinal);
    }

    private static string FindSiblingPath(params string[] parts)
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            var candidate = Path.Combine(new[] { dir.FullName }.Concat(parts).ToArray());
            if (Directory.Exists(candidate))
            {
                return candidate;
            }

            dir = dir.Parent;
        }

        throw new DirectoryNotFoundException($"Could not find path: {string.Join("/", parts)}");
    }
}
