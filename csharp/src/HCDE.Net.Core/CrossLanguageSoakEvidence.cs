using System.Text.Json;

namespace HCDE.Net.Core;

public readonly struct CrossLanguageSoakEvidenceRecord
{
    public CrossLanguageSoakEvidenceRecord(
        string harness,
        CrossLanguageSoakStatus status,
        DateTimeOffset recordedAtUtc,
        string? skipReason = null,
        string? outputTail = null)
    {
        Harness = harness;
        Status = status;
        RecordedAtUtc = recordedAtUtc;
        SkipReason = skipReason;
        OutputTail = outputTail;
    }

    public string Harness { get; }
    public CrossLanguageSoakStatus Status { get; }
    public DateTimeOffset RecordedAtUtc { get; }
    public string? SkipReason { get; }
    public string? OutputTail { get; }
}

public static class CrossLanguageSoakEvidence
{
    private const int OutputTailLimit = 4096;

    public static string? TryWrite(string harness, CrossLanguageSoakResult result)
    {
        var evidenceDir = Environment.GetEnvironmentVariable("HCDE_SOAK_EVIDENCE_DIR");
        if (string.IsNullOrWhiteSpace(evidenceDir))
            return null;

        Directory.CreateDirectory(evidenceDir);
        var timestamp = DateTimeOffset.UtcNow.ToString("yyyyMMdd_HHmmss");
        var fileName = $"{harness}_{timestamp}_{result.Status}.json";
        var path = Path.Combine(evidenceDir, fileName);
        var outputTail = string.IsNullOrWhiteSpace(result.Output)
            ? null
            : result.Output.Length <= OutputTailLimit
                ? result.Output
                : result.Output[^OutputTailLimit..];

        var record = new CrossLanguageSoakEvidenceRecord(
            harness,
            result.Status,
            DateTimeOffset.UtcNow,
            result.SkipReason,
            outputTail);

        var json = JsonSerializer.Serialize(record, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(path, json);
        return path;
    }

    public static CrossLanguageSoakResult Finalize(string harness, CrossLanguageSoakResult result)
    {
        TryWrite(harness, result);
        return result;
    }
}
