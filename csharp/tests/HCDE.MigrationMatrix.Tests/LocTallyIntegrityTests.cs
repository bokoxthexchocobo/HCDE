using System.Text.Json;
using System.Text.RegularExpressions;

namespace HCDE.MigrationMatrix.Tests;

public class LocTallyIntegrityTests
{
    private static readonly Regex CommitSha = new("^[0-9a-f]{40}$", RegexOptions.Compiled);

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
            var managedLoc = chunk.GetProperty("managedLoc").GetInt32();
            var bucket = ChunkBucket(chunk);
            var hasRanges = chunk.TryGetProperty("nativeRanges", out var rangesEl);
            var hasCommit = chunk.TryGetProperty("conversionManagedCommit", out var commitEl);

            if (nativeLoc > 0)
            {
                Assert.True(hasRanges, $"Chunk '{name}' credits {nativeLoc} native LOC without nativeRanges.");
            }

            if (bucket == "conversion" && nativeLoc > 0)
            {
                Assert.True(hasCommit, $"Conversion chunk '{name}' must include conversionManagedCommit.");
                var sha = commitEl.GetString() ?? "";
                Assert.True(CommitSha.IsMatch(sha), $"Chunk '{name}' conversionManagedCommit is not a full git SHA.");
                Assert.True(managedLoc > 0, $"Conversion chunk '{name}' must credit managedLoc > 0.");
            }

            if (bucket == "recognition" && nativeLoc > 0)
            {
                Assert.False(hasCommit, $"Recognition chunk '{name}' must not include conversionManagedCommit.");
                Assert.Equal(0, managedLoc);
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

    [Fact]
    public void LocTallyNativeRangesDoNotOverlapAcrossChunks()
    {
        var repo = FindRepoRoot();
        var tallyPath = Path.Combine(repo, "csharp", "docs", "data", "loc_tally.json");
        using var doc = JsonDocument.Parse(File.ReadAllText(tallyPath));
        var chunks = doc.RootElement.GetProperty("chunks");

        var credited = new Dictionary<(string File, int Line), string>();
        foreach (var chunk in chunks.EnumerateArray())
        {
            if (!chunk.TryGetProperty("nativeRanges", out var rangesEl))
                continue;

            var chunkName = chunk.GetProperty("name").GetString() ?? "";
            foreach (var range in rangesEl.EnumerateArray())
            {
                var file = range.GetProperty("file").GetString() ?? "";
                var start = range.GetProperty("start").GetInt32();
                var end = range.GetProperty("end").GetInt32();
                var symbol = range.TryGetProperty("symbol", out var sym) ? sym.GetString() : "";
                for (var line = start; line <= end; line++)
                {
                    var key = (file, line);
                    if (credited.TryGetValue(key, out var prior))
                    {
                        Assert.Fail(
                            $"Native line {file}:{line} is credited in both '{prior}' and '{chunkName}' (range {symbol}).");
                    }

                    credited[key] = chunkName;
                }
            }
        }
    }

    private static string ChunkBucket(JsonElement chunk)
    {
        if (chunk.TryGetProperty("bucket", out var bucket) && bucket.ValueKind == JsonValueKind.String)
            return bucket.GetString() ?? "conversion";
        var name = chunk.GetProperty("name").GetString() ?? "";
        return name.Contains("recognition", StringComparison.Ordinal) ? "recognition" : "conversion";
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
