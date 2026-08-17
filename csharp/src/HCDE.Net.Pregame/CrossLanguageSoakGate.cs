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

        return new CrossLanguageSoakGateResult(CrossLanguageSoakGateStatus.Passed);
    }

    public static bool ShouldEnforceInCi() =>
        string.Equals(Environment.GetEnvironmentVariable("HCDE_ENFORCE_SOAK_GATE"), "1", StringComparison.Ordinal);
}
