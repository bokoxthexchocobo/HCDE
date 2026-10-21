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
    public void LocTallyHeaderTotalsMatchChunkSums()
    {
        var repo = FindRepoRoot();
        var tallyPath = Path.Combine(repo, "csharp", "docs", "data", "loc_tally.json");
        using var doc = JsonDocument.Parse(File.ReadAllText(tallyPath));
        var root = doc.RootElement;
        var chunks = root.GetProperty("chunks");

        var convertedSum = 0;
        var recognitionSum = 0;
        var managedSum = 0;
        foreach (var chunk in chunks.EnumerateArray())
        {
            var bucket = ChunkBucket(chunk);
            var nativeLoc = chunk.GetProperty("nativeLoc").GetInt32();
            var managedLoc = chunk.GetProperty("managedLoc").GetInt32();
            if (bucket == "conversion")
            {
                convertedSum += nativeLoc;
                managedSum += managedLoc;
            }
            else if (bucket == "recognition")
            {
                recognitionSum += nativeLoc;
            }
        }

        var convertedHeader = root.GetProperty("convertedBehaviorNative").GetInt32();
        var recognitionHeader = root.GetProperty("recognitionBehaviorNative").GetInt32();
        var managedHeader = root.GetProperty("convertedBehaviorManaged").GetInt32();
        var combinedHeader = root.GetProperty("combinedBehaviorNative").GetInt32();
        var remainingHeader = root.GetProperty("remainingNative").GetInt32();
        var baseline = root.GetProperty("baselineNative").GetInt32();

        Assert.Equal(convertedSum, convertedHeader);
        Assert.Equal(recognitionSum, recognitionHeader);
        Assert.Equal(managedSum, managedHeader);
        Assert.Equal(convertedSum + recognitionSum, combinedHeader);
        Assert.Equal(baseline - convertedSum - recognitionSum, remainingHeader);
    }

    [Fact]
    public void LocTallyConversionChunksDeclareExplicitBucket()
    {
        var repo = FindRepoRoot();
        var tallyPath = Path.Combine(repo, "csharp", "docs", "data", "loc_tally.json");
        using var doc = JsonDocument.Parse(File.ReadAllText(tallyPath));
        foreach (var chunk in doc.RootElement.GetProperty("chunks").EnumerateArray())
        {
            var bucket = ChunkBucket(chunk);
            if (bucket != "conversion")
                continue;
            var nativeLoc = chunk.GetProperty("nativeLoc").GetInt32();
            if (nativeLoc <= 0)
                continue;
            var name = chunk.GetProperty("name").GetString() ?? "";
            Assert.True(
                chunk.TryGetProperty("bucket", out var bucketEl) && bucketEl.ValueKind == JsonValueKind.String,
                $"Conversion chunk '{name}' must set explicit \"bucket\": \"conversion\" so header sums match auditor spreadsheets.");
        }
    }

    [Fact]
    public void LocTallyManagedLocMatchesSurvivingSourceAtHead()
    {
        var repo = FindRepoRoot();
        var tallyPath = Path.Combine(repo, "csharp", "docs", "data", "loc_tally.json");
        using var doc = JsonDocument.Parse(File.ReadAllText(tallyPath));
        foreach (var chunk in doc.RootElement.GetProperty("chunks").EnumerateArray())
        {
            if (ChunkBucket(chunk) != "conversion")
                continue;
            var nativeLoc = chunk.GetProperty("nativeLoc").GetInt32();
            if (nativeLoc <= 0)
                continue;
            var name = chunk.GetProperty("name").GetString() ?? "";
            var managedLoc = chunk.GetProperty("managedLoc").GetInt32();
            var files = ManagedSourceFilesForChunk(repo, name, chunk);
            if (files.Count == 0)
                continue;
            var surviving = files.Sum(CountCountableManagedSourceLines);
            Assert.True(
                surviving == managedLoc,
                $"Chunk '{name}' managedLoc must equal countable csharp/src HEAD lines (tally={managedLoc}, head={surviving}).");
        }
    }

    [Fact]
    public void ManagedSourceFilesAreExclusiveAcrossChunkDefinitions()
    {
        var repo = FindRepoRoot();
        var toolsDir = Path.Combine(repo, "csharp", "tools");
        var owners = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var jsonPath in Directory.EnumerateFiles(toolsDir, "chunk*.json"))
        {
            using var doc = JsonDocument.Parse(File.ReadAllText(jsonPath));
            var root = doc.RootElement;
            if (root.ValueKind == JsonValueKind.Array)
                root = root[0];
            if (!root.TryGetProperty("managedSourceFiles", out var files) || files.ValueKind != JsonValueKind.Array)
                continue;
            var chunkName = root.TryGetProperty("name", out var nameEl) ? nameEl.GetString() ?? jsonPath : jsonPath;
            foreach (var file in files.EnumerateArray())
            {
                var path = file.GetString() ?? "";
                if (path.Length == 0)
                    continue;
                Assert.StartsWith("csharp/src/", path);
                Assert.DoesNotContain(".Tests/", path);
                Assert.True(
                    owners.TryAdd(path, chunkName),
                    $"csharp/src file '{path}' is attributed to both '{owners[path]}' and '{chunkName}'.");
            }
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

    private static List<string> ManagedSourceFilesForChunk(string repo, string chunkName, JsonElement tallyChunk)
    {
        var m = Regex.Match(chunkName, @"chunk(\d+)");
        if (!m.Success)
            return [];
        var jsonPath = Path.Combine(repo, "csharp", "tools", $"chunk{m.Groups[1].Value}.json");
        if (File.Exists(jsonPath))
        {
            using var doc = JsonDocument.Parse(File.ReadAllText(jsonPath));
            var root = doc.RootElement;
            if (root.ValueKind == JsonValueKind.Array)
                root = root[0];
            if (root.TryGetProperty("managedSourceFiles", out var files) && files.ValueKind == JsonValueKind.Array)
            {
                var fromJson = files.EnumerateArray().Select(e => e.GetString() ?? "").Where(s => s.Length > 0).ToList();
                if (fromJson.Count > 0)
                    return fromJson;
            }
        }

        if (tallyChunk.TryGetProperty("managedSourceFiles", out var tallyFiles) && tallyFiles.ValueKind == JsonValueKind.Array)
            return tallyFiles.EnumerateArray().Select(e => e.GetString() ?? "").Where(s => s.Length > 0).ToList();
        return [];
    }

    private static int CountCountableManagedSourceLines(string repoRelativePath)
    {
        var repo = FindRepoRoot();
        var path = Path.Combine(repo, repoRelativePath.Replace('/', Path.DirectorySeparatorChar));
        if (!File.Exists(path))
            return 0;
        return File.ReadAllLines(path).Count(IsCountableManagedCsharpLine);
    }

    private static bool IsCountableManagedCsharpLine(string line) => true;

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
