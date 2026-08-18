using System.Text.Json;
using HCDE.Net.Core;

namespace HCDE.Net.Pregame;

public enum CrossLanguageSoakGateStatus
{
    NotRequired,
    Passed,
    Failed,
}

public readonly struct CrossLanguageSoakGateResult
{
    public CrossLanguageSoakGateResult(CrossLanguageSoakGateStatus status, string? reason = null)
    {
        Status = status;
        Reason = reason;
    }

    public CrossLanguageSoakGateStatus Status { get; }
    public string? Reason { get; }
}

public static class CrossLanguageSoakGate
{
    public static bool AreSoakSecretsConfigured()
    {
        return !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("HCDE_HCDESERV_PATH"))
            && !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("HCDE_IWAD_PATH"));
    }

    public static CrossLanguageSoakGateResult Evaluate(string? repositoryRoot = null, bool requireConfiguredSecrets = true)
    {
        if (requireConfiguredSecrets && !AreSoakSecretsConfigured())
            return new CrossLanguageSoakGateResult(CrossLanguageSoakGateStatus.NotRequired, "soak secrets not configured");

        var manifestPath = CrossLanguageSoakManifest.ResolveDefaultManifestPath(repositoryRoot);
        if (!File.Exists(manifestPath))
            return new CrossLanguageSoakGateResult(CrossLanguageSoakGateStatus.Failed, $"manifest missing: {manifestPath}");

        using var document = JsonDocument.Parse(File.ReadAllText(manifestPath));
        if (!document.RootElement.TryGetProperty("Harnesses", out var harnesses)
            || harnesses.ValueKind != JsonValueKind.Array)
        {
            return new CrossLanguageSoakGateResult(CrossLanguageSoakGateStatus.Failed, "manifest missing Harnesses array");
        }

        foreach (var harness in harnesses.EnumerateArray())
        {
            if (!harness.TryGetProperty("Harness", out var harnessName)
                || !harness.TryGetProperty("Status", out var status))
            {
                return new CrossLanguageSoakGateResult(CrossLanguageSoakGateStatus.Failed, "manifest entry missing Harness or Status");
            }

            if (!string.Equals(status.GetString(), CrossLanguageSoakStatus.Passed.ToString(), StringComparison.Ordinal))
            {
                return new CrossLanguageSoakGateResult(
                    CrossLanguageSoakGateStatus.Failed,
                    $"harness {harnessName.GetString()} status is {status.GetString()}");
            }
        }

        var staleness = EvaluateManifestStaleness(
            manifestPath,
            DateTimeOffset.UtcNow,
            ResolveMaxManifestAgeDays());
        if (staleness.Status == CrossLanguageSoakGateStatus.Failed)
            return staleness;

        var evidenceFreshness = EvaluateEvidenceFreshness(repositoryRoot, DateTimeOffset.UtcNow, ResolveMaxEvidenceAgeDays());
        if (evidenceFreshness.Status == CrossLanguageSoakGateStatus.Failed)
            return evidenceFreshness;

        return new CrossLanguageSoakGateResult(CrossLanguageSoakGateStatus.Passed);
    }

    public static CrossLanguageSoakGateResult EvaluateCommittedDualFreshness(
        string? repositoryRoot,
        DateTimeOffset nowUtc,
        int maxManifestAgeDays,
        int maxEvidenceAgeDays)
    {
        var manifestPath = CrossLanguageSoakManifest.ResolveDefaultManifestPath(repositoryRoot);
        var manifestStaleness = EvaluateManifestStaleness(manifestPath, nowUtc, maxManifestAgeDays);
        if (manifestStaleness.Status == CrossLanguageSoakGateStatus.Failed)
            return manifestStaleness;

        return EvaluateEvidenceFreshness(repositoryRoot, nowUtc, maxEvidenceAgeDays);
    }

    public static bool ShouldEnforceInCi() =>
        string.Equals(Environment.GetEnvironmentVariable("HCDE_ENFORCE_SOAK_GATE"), "1", StringComparison.Ordinal);

    public static int ResolveMaxManifestAgeDays()
    {
        var configured = Environment.GetEnvironmentVariable("HCDE_SOAK_MANIFEST_MAX_AGE_DAYS");
        return int.TryParse(configured, out var days) && days > 0
            ? days
            : CrossLanguageSoakManifest.DefaultMaxManifestAgeDays;
    }

    public static int ResolveMaxEvidenceAgeDays()
    {
        var configured = Environment.GetEnvironmentVariable("HCDE_SOAK_EVIDENCE_MAX_AGE_DAYS");
        return int.TryParse(configured, out var days) && days > 0
            ? days
            : CrossLanguageSoakManifest.DefaultMaxEvidenceAgeDays;
    }

    public static CrossLanguageSoakGateResult EvaluateEvidenceFreshness(
        string? repositoryRoot,
        DateTimeOffset nowUtc,
        int maxAgeDays)
    {
        var manifestPath = CrossLanguageSoakManifest.ResolveDefaultManifestPath(repositoryRoot);
        if (!CrossLanguageSoakManifest.TryReadHarnessEvidenceFiles(manifestPath, out var entries))
        {
            return new CrossLanguageSoakGateResult(
                CrossLanguageSoakGateStatus.Failed,
                "manifest missing harness evidence files");
        }

        var evidenceDirectory = CrossLanguageSoakEvidenceArchive.ResolveDefaultEvidenceDirectory(repositoryRoot);
        foreach (var (harness, evidenceFile) in entries)
        {
            var evidencePath = Path.Combine(evidenceDirectory, evidenceFile);
            if (!File.Exists(evidencePath))
            {
                return new CrossLanguageSoakGateResult(
                    CrossLanguageSoakGateStatus.Failed,
                    $"evidence file missing for {harness}: {evidenceFile}");
            }

            var age = nowUtc - File.GetLastWriteTimeUtc(evidencePath);
            if (age > TimeSpan.FromDays(maxAgeDays))
            {
                return new CrossLanguageSoakGateResult(
                    CrossLanguageSoakGateStatus.Failed,
                    $"evidence file stale for {harness}: {evidenceFile}");
            }
        }

        return new CrossLanguageSoakGateResult(CrossLanguageSoakGateStatus.Passed);
    }

    public static CrossLanguageSoakGateResult EvaluateExportBundleDualFreshness(
        string exportDirectory,
        DateTimeOffset nowUtc,
        int maxManifestAgeDays,
        int maxEvidenceAgeDays)
    {
        var manifestPath = Path.Combine(exportDirectory, "manifest.json");
        var manifestStaleness = EvaluateManifestStaleness(manifestPath, nowUtc, maxManifestAgeDays);
        if (manifestStaleness.Status == CrossLanguageSoakGateStatus.Failed)
            return manifestStaleness;

        return EvaluateExportBundleEvidenceFreshness(exportDirectory, nowUtc, maxEvidenceAgeDays);
    }

    public static CrossLanguageSoakGateResult EvaluateExportBundleEvidenceFreshness(
        string exportDirectory,
        DateTimeOffset nowUtc,
        int maxAgeDays)
    {
        var manifestPath = Path.Combine(exportDirectory, "manifest.json");
        if (!CrossLanguageSoakManifest.TryReadHarnessEvidenceFiles(manifestPath, out var entries))
        {
            return new CrossLanguageSoakGateResult(
                CrossLanguageSoakGateStatus.Failed,
                "export bundle missing harness evidence files");
        }

        var evidenceDirectory = Path.Combine(exportDirectory, "evidence");
        foreach (var (harness, evidenceFile) in entries)
        {
            var evidencePath = Path.Combine(evidenceDirectory, evidenceFile);
            if (!File.Exists(evidencePath))
            {
                return new CrossLanguageSoakGateResult(
                    CrossLanguageSoakGateStatus.Failed,
                    $"export bundle evidence missing for {harness}: {evidenceFile}");
            }

            var age = nowUtc - File.GetLastWriteTimeUtc(evidencePath);
            if (age > TimeSpan.FromDays(maxAgeDays))
            {
                return new CrossLanguageSoakGateResult(
                    CrossLanguageSoakGateStatus.Failed,
                    $"export bundle evidence stale for {harness}: {evidenceFile}");
            }
        }

        return new CrossLanguageSoakGateResult(CrossLanguageSoakGateStatus.Passed);
    }

    public static CrossLanguageSoakGateResult EvaluateManifestStaleness(
        string manifestPath,
        DateTimeOffset nowUtc,
        int maxAgeDays)
    {
        if (!CrossLanguageSoakManifest.TryReadRecordedAtUtc(manifestPath, out var recordedAtUtc))
        {
            return new CrossLanguageSoakGateResult(
                CrossLanguageSoakGateStatus.Failed,
                "manifest missing RecordedAtUtc");
        }

        var age = nowUtc - recordedAtUtc;
        if (age > TimeSpan.FromDays(maxAgeDays))
        {
            return new CrossLanguageSoakGateResult(
                CrossLanguageSoakGateStatus.Failed,
                $"manifest stale: recorded {recordedAtUtc:O}, max age {maxAgeDays} days");
        }

        return new CrossLanguageSoakGateResult(CrossLanguageSoakGateStatus.Passed);
    }
}
