using HCDE.Net.Core;

namespace HCDE.Net.Pregame.Tests;

[Collection("CrossLanguageSoak")]
public class CrossLanguageSoakSuiteTests
{
    [Fact]
    public void RunAll_RecordsEvidenceForEachHarnessWhenConfigured()
    {
        var evidenceDir = Path.Combine(Path.GetTempPath(), $"hcde-soak-suite-{Guid.NewGuid():N}");
        var priorEvidenceDir = Environment.GetEnvironmentVariable("HCDE_SOAK_EVIDENCE_DIR");
        Environment.SetEnvironmentVariable("HCDE_SOAK_EVIDENCE_DIR", evidenceDir);
        try
        {
            var results = CrossLanguageSoakSuite.RunAll();
            Assert.Equal(2, results.Count);
            Assert.All(results, result => Assert.NotEqual(CrossLanguageSoakStatus.Failed, result.Status));

            var files = Directory.GetFiles(evidenceDir, "*.json");
            Assert.Equal(2, files.Length);
        }
        finally
        {
            Environment.SetEnvironmentVariable("HCDE_SOAK_EVIDENCE_DIR", priorEvidenceDir);
            if (Directory.Exists(evidenceDir))
                Directory.Delete(evidenceDir, recursive: true);
        }
    }

    [Fact]
    public void RunAll_SkipsWhenHcdeservOrIwadMissing()
    {
        var serverPath = Environment.GetEnvironmentVariable("HCDE_HCDESERV_PATH");
        var iwadPath = Environment.GetEnvironmentVariable("HCDE_IWAD_PATH");
        if (!string.IsNullOrWhiteSpace(serverPath) && !string.IsNullOrWhiteSpace(iwadPath))
            return;

        var results = CrossLanguageSoakSuite.RunAll();
        Assert.Equal(2, results.Count);
        Assert.All(results, result => Assert.Equal(CrossLanguageSoakStatus.Skipped, result.Status));
    }
}
