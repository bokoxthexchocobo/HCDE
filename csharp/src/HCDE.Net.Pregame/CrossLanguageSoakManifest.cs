using System.Text.Json;
using HCDE.Net.Core;

namespace HCDE.Net.Pregame;

public sealed class CrossLanguageSoakManifestEntry
{
    public CrossLanguageSoakManifestEntry(
        string harness,
        string status,
        DateTimeOffset recordedAtUtc,
        string? skipReason = null,
        string? evidenceFile = null)
    {
        Harness = harness;
        Status = status;
        RecordedAtUtc = recordedAtUtc;
        SkipReason = skipReason;
        EvidenceFile = evidenceFile;
    }

    public string Harness { get; }
    public string Status { get; }
    public DateTimeOffset RecordedAtUtc { get; }
    public string? SkipReason { get; }
    public string? EvidenceFile { get; }
}

public static class CrossLanguageSoakManifest
{
    public const int DefaultMaxManifestAgeDays = 8;

    private static readonly string[] HarnessOrder =
    [
        "pregame_guest_smoke",
        "netcode_step12_invasion",
    ];

    public static string ResolveDefaultManifestPath(string? repositoryRoot = null)
    {
        repositoryRoot ??= FindRepositoryRoot();
        return Path.Combine(repositoryRoot, "csharp", "validation", "soak", "manifest.json");
    }

    public static void Write(
        string manifestPath,
        IReadOnlyList<CrossLanguageSoakResult> results,
        IReadOnlyList<string> evidenceFiles)
    {
        var entries = new List<CrossLanguageSoakManifestEntry>();
        for (var i = 0; i < HarnessOrder.Length; i++)
        {
            var harness = HarnessOrder[i];
            var result = i < results.Count ? results[i] : default;
            var evidenceFile = evidenceFiles.LastOrDefault(
                path => Path.GetFileName(path).StartsWith(harness + "_", StringComparison.Ordinal));
            entries.Add(new CrossLanguageSoakManifestEntry(
                harness,
                result.Status.ToString(),
                DateTimeOffset.UtcNow,
                result.SkipReason,
                evidenceFile is null ? null : Path.GetFileName(evidenceFile)));
        }

        var manifest = new
        {
            RecordedAtUtc = DateTimeOffset.UtcNow,
            Harnesses = entries.Select(entry => new
            {
                entry.Harness,
                entry.Status,
                entry.RecordedAtUtc,
                entry.SkipReason,
                entry.EvidenceFile,
            }),
        };

        Directory.CreateDirectory(Path.GetDirectoryName(manifestPath)!);
        var json = JsonSerializer.Serialize(manifest, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(manifestPath, json);
    }

    public static bool TryReadRecordedAtUtc(string manifestPath, out DateTimeOffset recordedAtUtc)
    {
        recordedAtUtc = default;
        if (!File.Exists(manifestPath))
            return false;

        using var document = JsonDocument.Parse(File.ReadAllText(manifestPath));
        if (!document.RootElement.TryGetProperty("RecordedAtUtc", out var recordedAt)
            || recordedAt.ValueKind != JsonValueKind.String)
        {
            return false;
        }

        return DateTimeOffset.TryParse(recordedAt.GetString(), out recordedAtUtc);
    }

    private static string FindRepositoryRoot()
    {
        var cursor = new DirectoryInfo(AppContext.BaseDirectory);
        while (cursor != null)
        {
            if (Directory.Exists(Path.Combine(cursor.FullName, "csharp"))
                && File.Exists(Path.Combine(cursor.FullName, "README.md")))
            {
                return cursor.FullName;
            }

            cursor = cursor.Parent;
        }

        return Directory.GetCurrentDirectory();
    }
}
