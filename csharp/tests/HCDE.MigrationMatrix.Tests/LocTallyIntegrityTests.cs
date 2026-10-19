using System.Text.Json;

namespace HCDE.MigrationMatrix.Tests;

public class LocTallyIntegrityTests
{
    /// <summary>Pre-range headline credits; migrate to <c>nativeRanges</c> instead of extending this set.</summary>
    private static readonly HashSet<string> LegacyHeadlineChunksWithoutRanges = new(StringComparer.Ordinal)
    {
        "acs-random-getcvar-polywait",
        "combat-trace-ordering",
        "acs-map-mutations",
        "line-specials-plat-generic-elevator",
        "line-specials-gap-closure",
        "acs-callfunc-wire-dispatch-udmf",
        "action-combat-port",
        "acs-hot-callfunc-behavior",
        "sector-changeflags-gap-audit",
        "line-special-feasible-gaps",
        "action-dispatch-damage-wiring",
        "action-combat-dispatch-batch2",
        "action-explode-radius-give",
        "thing-projectile-175-178",
        "acs-air-userarray-spawnspot",
        "p-enemy-chunk1",
        "p-map-radius-chunk1",
    };

    [Fact]
    public void LocTallyChunksHonorNativeRanges()
    {
        var repo = FindRepoRoot();
        var tallyPath = Path.Combine(repo, "csharp", "docs", "data", "loc_tally.json");
        using var doc = JsonDocument.Parse(File.ReadAllText(tallyPath));
        var chunks = doc.RootElement.GetProperty("chunks");

        foreach (var chunk in chunks.EnumerateArray())
        {
            var name = chunk.GetProperty("name").GetString() ?? "";
            var nativeLoc = chunk.GetProperty("nativeLoc").GetInt32();
            var hasRanges = chunk.TryGetProperty("nativeRanges", out var rangesEl);

            if (nativeLoc > 0 && !hasRanges)
            {
                Assert.True(
                    LegacyHeadlineChunksWithoutRanges.Contains(name),
                    $"Chunk '{name}' credits {nativeLoc} native LOC without nativeRanges.");
                continue;
            }

            if (!hasRanges)
                continue;

            var sum = 0;
            foreach (var range in rangesEl.EnumerateArray())
            {
                var start = range.GetProperty("start").GetInt32();
                var end = range.GetProperty("end").GetInt32();
                sum += end - start + 1;

                Assert.True(
                    range.TryGetProperty("tests", out var tests) && tests.GetArrayLength() > 0,
                    $"Chunk '{name}' range {range.GetProperty("symbol").GetString()} lacks named tests.");
            }

            Assert.True(
                sum == nativeLoc,
                $"Chunk '{name}' nativeLoc {nativeLoc} must equal nativeRanges sum {sum}.");
        }
    }

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "csharp", "HCDE.sln"))) return dir.FullName;
            dir = dir.Parent;
        }
        throw new InvalidOperationException("Repository root not found.");
    }
}
