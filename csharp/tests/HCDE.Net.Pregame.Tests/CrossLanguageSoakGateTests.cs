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

    private static bool AreSoakSecretsConfigured()
    {
        return CrossLanguageSoakGate.AreSoakSecretsConfigured();
    }
}
