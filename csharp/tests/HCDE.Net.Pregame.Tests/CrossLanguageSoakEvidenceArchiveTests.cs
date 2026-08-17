namespace HCDE.Net.Pregame.Tests;

[Collection("CrossLanguageSoak")]
public class CrossLanguageSoakEvidenceArchiveTests
{
    [Fact]
    public void RecordEvidence_WritesHarnessJsonFiles()
    {
        var evidenceDir = Path.Combine(Path.GetTempPath(), $"hcde-soak-archive-{Guid.NewGuid():N}");
        try
        {
            var files = CrossLanguageSoakEvidenceArchive.RecordEvidence(evidenceDir);
            Assert.Equal(2, files.Count);
            Assert.All(files, path => Assert.EndsWith(".json", path));
        }
        finally
        {
            if (Directory.Exists(evidenceDir))
                Directory.Delete(evidenceDir, recursive: true);
        }
    }

    [Fact]
    public void RecordEvidence_SkipsWhenHcdeservOrIwadMissing()
    {
        var serverPath = Environment.GetEnvironmentVariable("HCDE_HCDESERV_PATH");
        var iwadPath = Environment.GetEnvironmentVariable("HCDE_IWAD_PATH");
        if (!string.IsNullOrWhiteSpace(serverPath) && !string.IsNullOrWhiteSpace(iwadPath))
            return;

        var evidenceDir = Path.Combine(Path.GetTempPath(), $"hcde-soak-archive-{Guid.NewGuid():N}");
        try
        {
            var files = CrossLanguageSoakEvidenceArchive.RecordEvidence(evidenceDir);
            Assert.Equal(2, files.Count);
            foreach (var file in files)
            {
                Assert.Contains("_Skipped.json", file);
                var json = File.ReadAllText(file);
                Assert.Contains("\"SkipReason\"", json);
            }
        }
        finally
        {
            if (Directory.Exists(evidenceDir))
                Directory.Delete(evidenceDir, recursive: true);
        }
    }

    [Fact]
    public void RecordValidationSkippedEvidence_WhenRequested()
    {
        if (Environment.GetEnvironmentVariable("HCDE_RECORD_VALIDATION_EVIDENCE") != "1")
            return;

        var evidenceDir = CrossLanguageSoakEvidenceArchive.ResolveDefaultEvidenceDirectory();
        if (Directory.Exists(evidenceDir))
        {
            foreach (var file in Directory.GetFiles(evidenceDir, "*.json"))
                File.Delete(file);
        }

        var files = CrossLanguageSoakEvidenceArchive.RecordDefaultEvidence();
        Assert.Equal(2, files.Count);
        Assert.All(files, path => Assert.Contains("_Skipped.json", path));
    }
}
