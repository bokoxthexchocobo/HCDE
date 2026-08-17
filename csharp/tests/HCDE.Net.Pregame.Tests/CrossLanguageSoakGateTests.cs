namespace HCDE.Net.Pregame.Tests;

[Collection("CrossLanguageSoak")]
public class CrossLanguageSoakGateTests
{
    [Fact]
    public void Evaluate_ReturnsNotRequiredWhenSecretsMissing()
    {
        if (AreSoakSecretsConfigured())
            return;

        var result = CrossLanguageSoakGate.Evaluate();
        Assert.Equal(CrossLanguageSoakGateStatus.NotRequired, result.Status);
    }

    [Fact]
    public void Evaluate_RequiresPassedManifestWhenSecretsConfigured()
    {
        if (!AreSoakSecretsConfigured())
            return;

        if (Environment.GetEnvironmentVariable("HCDE_ENFORCE_SOAK_GATE") != "1")
            return;

        var result = CrossLanguageSoakGate.Evaluate();
        Assert.Equal(CrossLanguageSoakGateStatus.Passed, result.Status);
    }

    [Fact]
    public void Evaluate_FailsWhenManifestContainsSkippedHarness()
    {
        var baseDir = Path.Combine(Path.GetTempPath(), $"hcde-soak-gate-{Guid.NewGuid():N}");
        var soakDir = Path.Combine(baseDir, "csharp", "validation", "soak");
        Directory.CreateDirectory(soakDir);
        File.WriteAllText(Path.Combine(baseDir, "README.md"), "test");
        File.WriteAllText(
            Path.Combine(soakDir, "manifest.json"),
            """
            {
              "Harnesses": [
                { "Harness": "pregame_guest_smoke", "Status": "Skipped" },
                { "Harness": "netcode_step12_invasion", "Status": "Passed" }
              ]
            }
            """);

        try
        {
            var result = CrossLanguageSoakGate.Evaluate(baseDir, requireConfiguredSecrets: false);
            Assert.Equal(CrossLanguageSoakGateStatus.Failed, result.Status);
            Assert.Contains("pregame_guest_smoke", result.Reason);
        }
        finally
        {
            if (Directory.Exists(baseDir))
                Directory.Delete(baseDir, recursive: true);
        }
    }

    [Fact]
    public void ShouldEnforceInCi_ReturnsTrueWhenEnvSet()
    {
        var previous = Environment.GetEnvironmentVariable("HCDE_ENFORCE_SOAK_GATE");
        try
        {
            Environment.SetEnvironmentVariable("HCDE_ENFORCE_SOAK_GATE", "1");
            Assert.True(CrossLanguageSoakGate.ShouldEnforceInCi());
        }
        finally
        {
            Environment.SetEnvironmentVariable("HCDE_ENFORCE_SOAK_GATE", previous);
        }
    }

    [Fact]
    public void Evaluate_FailsWhenManifestIsStale()
    {
        var baseDir = Path.Combine(Path.GetTempPath(), $"hcde-soak-stale-{Guid.NewGuid():N}");
        var soakDir = Path.Combine(baseDir, "csharp", "validation", "soak");
        Directory.CreateDirectory(soakDir);
        File.WriteAllText(Path.Combine(baseDir, "README.md"), "test");
        File.WriteAllText(
            Path.Combine(soakDir, "manifest.json"),
            """
            {
              "RecordedAtUtc": "2020-01-01T00:00:00+00:00",
              "Harnesses": [
                { "Harness": "pregame_guest_smoke", "Status": "Passed" },
                { "Harness": "netcode_step12_invasion", "Status": "Passed" }
              ]
            }
            """);

        try
        {
            var result = CrossLanguageSoakGate.Evaluate(baseDir, requireConfiguredSecrets: false);
            Assert.Equal(CrossLanguageSoakGateStatus.Failed, result.Status);
            Assert.Contains("manifest stale", result.Reason);
        }
        finally
        {
            if (Directory.Exists(baseDir))
                Directory.Delete(baseDir, recursive: true);
        }
    }

    [Fact]
    public void Evaluate_FailsWhenEvidenceFileIsMissing()
    {
        var baseDir = Path.Combine(Path.GetTempPath(), $"hcde-soak-evidence-missing-{Guid.NewGuid():N}");
        var soakDir = Path.Combine(baseDir, "csharp", "validation", "soak");
        var evidenceDir = Path.Combine(soakDir, "evidence");
        Directory.CreateDirectory(evidenceDir);
        File.WriteAllText(Path.Combine(baseDir, "README.md"), "test");
        File.WriteAllText(
            Path.Combine(soakDir, "manifest.json"),
            $$"""
            {
              "RecordedAtUtc": "{{DateTimeOffset.UtcNow:O}}",
              "Harnesses": [
                { "Harness": "pregame_guest_smoke", "Status": "Passed", "EvidenceFile": "pregame_guest_smoke_20260101_Passed.json" }
              ]
            }
            """);

        try
        {
            var result = CrossLanguageSoakGate.Evaluate(baseDir, requireConfiguredSecrets: false);
            Assert.Equal(CrossLanguageSoakGateStatus.Failed, result.Status);
            Assert.Contains("evidence file missing", result.Reason);
        }
        finally
        {
            if (Directory.Exists(baseDir))
                Directory.Delete(baseDir, recursive: true);
        }
    }

    [Fact]
    public void Evaluate_FailsWhenEvidenceFileIsStale()
    {
        var baseDir = Path.Combine(Path.GetTempPath(), $"hcde-soak-evidence-stale-{Guid.NewGuid():N}");
        var soakDir = Path.Combine(baseDir, "csharp", "validation", "soak");
        var evidenceDir = Path.Combine(soakDir, "evidence");
        Directory.CreateDirectory(evidenceDir);
        File.WriteAllText(Path.Combine(baseDir, "README.md"), "test");
        var evidenceFile = "pregame_guest_smoke_20200101_Passed.json";
        File.WriteAllText(Path.Combine(evidenceDir, evidenceFile), "{}");
        File.SetLastWriteTimeUtc(Path.Combine(evidenceDir, evidenceFile), DateTime.UtcNow.AddDays(-30));
        File.WriteAllText(
            Path.Combine(soakDir, "manifest.json"),
            $$"""
            {
              "RecordedAtUtc": "{{DateTimeOffset.UtcNow:O}}",
              "Harnesses": [
                { "Harness": "pregame_guest_smoke", "Status": "Passed", "EvidenceFile": "{{evidenceFile}}" }
              ]
            }
            """);

        try
        {
            var result = CrossLanguageSoakGate.Evaluate(baseDir, requireConfiguredSecrets: false);
            Assert.Equal(CrossLanguageSoakGateStatus.Failed, result.Status);
            Assert.Contains("evidence file stale", result.Reason);
        }
        finally
        {
            if (Directory.Exists(baseDir))
                Directory.Delete(baseDir, recursive: true);
        }
    }

    [Fact]
    public void EvaluateEvidenceFreshness_PassesWhenWithinMaxAge()
    {
        var baseDir = Path.Combine(Path.GetTempPath(), $"hcde-soak-evidence-fresh-{Guid.NewGuid():N}");
        var soakDir = Path.Combine(baseDir, "csharp", "validation", "soak");
        var evidenceDir = Path.Combine(soakDir, "evidence");
        Directory.CreateDirectory(evidenceDir);
        File.WriteAllText(Path.Combine(baseDir, "README.md"), "test");
        var evidenceFile = "pregame_guest_smoke_20260101_Passed.json";
        File.WriteAllText(Path.Combine(evidenceDir, evidenceFile), "{}");
        File.WriteAllText(
            Path.Combine(soakDir, "manifest.json"),
            $$"""
            {
              "RecordedAtUtc": "{{DateTimeOffset.UtcNow:O}}",
              "Harnesses": [
                { "Harness": "pregame_guest_smoke", "Status": "Passed", "EvidenceFile": "{{evidenceFile}}" }
              ]
            }
            """);

        try
        {
            var result = CrossLanguageSoakGate.EvaluateEvidenceFreshness(
                baseDir,
                DateTimeOffset.UtcNow,
                maxAgeDays: 8);
            Assert.Equal(CrossLanguageSoakGateStatus.Passed, result.Status);
        }
        finally
        {
            if (Directory.Exists(baseDir))
                Directory.Delete(baseDir, recursive: true);
        }
    }

    [Fact]
    public void EvaluateManifestStaleness_PassesWhenWithinMaxAge()
    {
        var manifestPath = Path.Combine(Path.GetTempPath(), $"hcde-soak-fresh-{Guid.NewGuid():N}.json");
        File.WriteAllText(
            manifestPath,
            $$"""{"RecordedAtUtc":"{{DateTimeOffset.UtcNow:O}}","Harnesses":[]}""");

        try
        {
            var result = CrossLanguageSoakGate.EvaluateManifestStaleness(
                manifestPath,
                DateTimeOffset.UtcNow,
                maxAgeDays: 8);
            Assert.Equal(CrossLanguageSoakGateStatus.Passed, result.Status);
        }
        finally
        {
            if (File.Exists(manifestPath))
                File.Delete(manifestPath);
        }
    }

    private static bool AreSoakSecretsConfigured()
    {
        return CrossLanguageSoakGate.AreSoakSecretsConfigured();
    }
}
