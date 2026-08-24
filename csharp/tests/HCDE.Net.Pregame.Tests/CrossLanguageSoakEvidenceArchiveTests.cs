namespace HCDE.Net.Pregame.Tests;

[Collection("CrossLanguageSoak")]
public class CrossLanguageSoakEvidenceArchiveTests
{
    [Fact]
    public void RecordEvidence_CopiesManifestAndEvidence()
    {
        var baseDir = Path.Combine(Path.GetTempPath(), $"hcde-soak-record-{Guid.NewGuid():N}");
        var repositoryRoot = Path.Combine(baseDir, "repo");
        var evidenceDir = CrossLanguageSoakEvidenceArchive.ResolveDefaultEvidenceDirectory(repositoryRoot);
        Directory.CreateDirectory(evidenceDir);
        File.WriteAllText(Path.Combine(repositoryRoot, "README.md"), "test");

        try
        {
            var files = CrossLanguageSoakEvidenceArchive.RecordEvidence(evidenceDir, repositoryRoot);
            Assert.Equal(2, files.Count);
            Assert.True(File.Exists(CrossLanguageSoakManifest.ResolveDefaultManifestPath(repositoryRoot)));
            Assert.All(files, path => Assert.StartsWith(evidenceDir, Path.GetDirectoryName(path)!));
        }
        finally
        {
            if (Directory.Exists(baseDir))
                Directory.Delete(baseDir, recursive: true);
        }
    }

    [Fact]
    public void RecordEvidence_WritesManifestWithHarnessStatuses()
    {
        var baseDir = Path.Combine(Path.GetTempPath(), $"hcde-soak-manifest-{Guid.NewGuid():N}");
        var evidenceDir = Path.Combine(baseDir, "evidence");
        try
        {
            var files = CrossLanguageSoakEvidenceArchive.RecordEvidence(evidenceDir);
            Assert.Equal(2, files.Count);
            var manifestPath = Path.Combine(baseDir, "manifest.json");
            Assert.True(File.Exists(manifestPath));
            var json = File.ReadAllText(manifestPath);
            Assert.Contains("pregame_guest_smoke", json);
            Assert.Contains("netcode_step12_invasion", json);
        }
        finally
        {
            if (Directory.Exists(baseDir))
                Directory.Delete(baseDir, recursive: true);
        }
    }

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
    public void RecordEvidence_CopiesHarnessJsonFiles()
    {
        var baseDir = Path.Combine(Path.GetTempPath(), $"hcde-soak-harness-{Guid.NewGuid():N}");
        var repositoryRoot = Path.Combine(baseDir, "repo");
        var evidenceDir = CrossLanguageSoakEvidenceArchive.ResolveDefaultEvidenceDirectory(repositoryRoot);
        Directory.CreateDirectory(evidenceDir);
        File.WriteAllText(Path.Combine(repositoryRoot, "README.md"), "test");

        try
        {
            var files = CrossLanguageSoakEvidenceArchive.RecordEvidence(evidenceDir, repositoryRoot);
            Assert.Equal(2, files.Count);
            Assert.Contains(files, path => path.Contains("pregame_guest_smoke", StringComparison.Ordinal));
            Assert.Contains(files, path => path.Contains("netcode_step12_invasion", StringComparison.Ordinal));
            Assert.All(files, path => Assert.StartsWith(evidenceDir, Path.GetDirectoryName(path)!));
        }
        finally
        {
            if (Directory.Exists(baseDir))
                Directory.Delete(baseDir, recursive: true);
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
        Assert.True(File.Exists(CrossLanguageSoakManifest.ResolveDefaultManifestPath()));
    }

    [Fact]
    public void RecordValidationSkippedEvidence_CopiesHarnessJsonFiles()
    {
        var baseDir = Path.Combine(Path.GetTempPath(), $"hcde-soak-skipped-{Guid.NewGuid():N}");
        var repositoryRoot = Path.Combine(baseDir, "repo");
        var evidenceDir = CrossLanguageSoakEvidenceArchive.ResolveDefaultEvidenceDirectory(repositoryRoot);
        Directory.CreateDirectory(evidenceDir);
        File.WriteAllText(Path.Combine(repositoryRoot, "README.md"), "test");

        try
        {
            var files = CrossLanguageSoakEvidenceArchive.RecordDefaultEvidence(repositoryRoot);
            Assert.Equal(2, files.Count);
            Assert.Contains(files, path => path.Contains("pregame_guest_smoke", StringComparison.Ordinal));
            Assert.Contains(files, path => path.Contains("netcode_step12_invasion", StringComparison.Ordinal));
            Assert.All(files, path => Assert.Contains("_Skipped.json", path));
            Assert.All(files, path => Assert.StartsWith(evidenceDir, Path.GetDirectoryName(path)!));
            Assert.True(File.Exists(CrossLanguageSoakManifest.ResolveDefaultManifestPath(repositoryRoot)));
        }
        finally
        {
            if (Directory.Exists(baseDir))
                Directory.Delete(baseDir, recursive: true);
        }
    }

    [Fact]
    public void RecordValidationPassedEvidence_WhenRequested()
    {
        if (Environment.GetEnvironmentVariable("HCDE_RECORD_VALIDATION_EVIDENCE") != "1")
            return;

        var serverPath = Environment.GetEnvironmentVariable("HCDE_HCDESERV_PATH");
        var iwadPath = Environment.GetEnvironmentVariable("HCDE_IWAD_PATH");
        if (string.IsNullOrWhiteSpace(serverPath) || string.IsNullOrWhiteSpace(iwadPath))
            return;

        var evidenceDir = CrossLanguageSoakEvidenceArchive.ResolveDefaultEvidenceDirectory();
        if (Directory.Exists(evidenceDir))
        {
            foreach (var file in Directory.GetFiles(evidenceDir, "*.json"))
                File.Delete(file);
        }

        var files = CrossLanguageSoakEvidenceArchive.RecordValidationEvidence();
        Assert.Equal(2, files.Count);
        Assert.All(files, path => Assert.DoesNotContain("_Skipped.json", path));
        Assert.All(files, path => Assert.Contains("_Passed.json", path));
        Assert.True(File.Exists(CrossLanguageSoakManifest.ResolveDefaultManifestPath()));
        var manifest = File.ReadAllText(CrossLanguageSoakManifest.ResolveDefaultManifestPath());
        Assert.Contains("\"Passed\"", manifest);
    }

    [Fact]
    public void ExportCommittedTemplates_WritesCiArtifactBundle()
    {
        if (Environment.GetEnvironmentVariable("HCDE_EXPORT_SOAK_TEMPLATES") != "1")
            return;

        var outputDir = Environment.GetEnvironmentVariable("HCDE_SOAK_TEMPLATE_EXPORT_DIR");
        Assert.False(string.IsNullOrWhiteSpace(outputDir));

        var files = CrossLanguageSoakEvidenceArchive.ExportCommittedTemplates(outputDir!);
        Assert.NotEmpty(files);
        Assert.True(File.Exists(Path.Combine(outputDir!, "manifest.json")));
        Assert.True(Directory.Exists(Path.Combine(outputDir!, "evidence")));
    }

    [Fact]
    public void ExportCommittedTemplates_CopiesManifestAndEvidence()
    {
        if (Environment.GetEnvironmentVariable("HCDE_EXPORT_SOAK_TEMPLATES") != "1")
            return;

        var baseDir = Path.Combine(Path.GetTempPath(), $"hcde-soak-export-{Guid.NewGuid():N}");
        var repositoryRoot = Path.Combine(baseDir, "repo");
        var evidenceDir = Path.Combine(repositoryRoot, "csharp", "validation", "soak", "evidence");
        Directory.CreateDirectory(evidenceDir);
        File.WriteAllText(Path.Combine(repositoryRoot, "README.md"), "test");
        File.WriteAllText(Path.Combine(evidenceDir, "pregame_guest_smoke_test_Passed.json"), "{}");
        File.WriteAllText(
            Path.Combine(repositoryRoot, "csharp", "validation", "soak", "manifest.json"),
            $$"""
            {
              "RecordedAtUtc": "{{DateTimeOffset.UtcNow:O}}",
              "Harnesses": [
                { "Harness": "pregame_guest_smoke", "Status": "Passed", "EvidenceFile": "pregame_guest_smoke_test_Passed.json" }
              ]
            }
            """);

        var outputDir = Path.Combine(baseDir, "artifact");
        try
        {
            var files = CrossLanguageSoakEvidenceArchive.ExportCommittedTemplates(outputDir, repositoryRoot);
            Assert.Contains(files, path => path.EndsWith("manifest.json"));
            Assert.Contains(files, path => path.Contains("evidence/pregame_guest_smoke_test_Passed.json"));
            Assert.True(File.Exists(Path.Combine(outputDir, "COMMIT_INSTRUCTIONS.md")));
        }
        finally
        {
            if (Directory.Exists(baseDir))
                Directory.Delete(baseDir, recursive: true);
        }
    }

    [Fact]
    public void ExportCommittedTemplates_RejectsStaleCommittedManifest()
    {
        var baseDir = Path.Combine(Path.GetTempPath(), $"hcde-soak-export-stale-{Guid.NewGuid():N}");
        var repositoryRoot = Path.Combine(baseDir, "repo");
        var evidenceDir = Path.Combine(repositoryRoot, "csharp", "validation", "soak", "evidence");
        Directory.CreateDirectory(evidenceDir);
        File.WriteAllText(Path.Combine(repositoryRoot, "README.md"), "test");
        File.WriteAllText(Path.Combine(evidenceDir, "pregame_guest_smoke_test_Passed.json"), "{}");
        File.WriteAllText(
            Path.Combine(repositoryRoot, "csharp", "validation", "soak", "manifest.json"),
            """
            {
              "RecordedAtUtc": "2020-01-01T00:00:00+00:00",
              "Harnesses": [
                { "Harness": "pregame_guest_smoke", "Status": "Passed", "EvidenceFile": "pregame_guest_smoke_test_Passed.json" }
              ]
            }
            """);

        try
        {
            var ex = Assert.Throws<InvalidOperationException>(
                () => CrossLanguageSoakEvidenceArchive.ExportCommittedTemplates(
                    Path.Combine(baseDir, "artifact"),
                    repositoryRoot));
            Assert.Contains("manifest stale", ex.Message);
        }
        finally
        {
            if (Directory.Exists(baseDir))
                Directory.Delete(baseDir, recursive: true);
        }
    }

    [Fact]
    public void ApplyExportedTemplates_WritesCommittedSoakTree()
    {
        if (Environment.GetEnvironmentVariable("HCDE_APPLY_SOAK_TEMPLATES") != "1")
            return;

        var exportDirectory = Environment.GetEnvironmentVariable("HCDE_SOAK_TEMPLATE_EXPORT_DIR");
        Assert.False(string.IsNullOrWhiteSpace(exportDirectory));

        var gate = CrossLanguageSoakEvidenceArchive.TryApplyExportedTemplatesFromEnvironment();
        Assert.NotEqual(CrossLanguageSoakGateStatus.Failed, gate.Status);
        Assert.True(File.Exists(CrossLanguageSoakManifest.ResolveDefaultManifestPath()));
        Assert.NotEmpty(Directory.GetFiles(
            CrossLanguageSoakEvidenceArchive.ResolveDefaultEvidenceDirectory(),
            "*.json"));
    }

    [Fact]
    public void ApplyExportedTemplates_RejectsStaleExportManifest()
    {
        var baseDir = Path.Combine(Path.GetTempPath(), $"hcde-soak-stale-manifest-{Guid.NewGuid():N}");
        var exportDir = Path.Combine(baseDir, "artifact");
        var exportEvidenceDir = Path.Combine(exportDir, "evidence");
        Directory.CreateDirectory(exportEvidenceDir);
        var evidenceFile = "pregame_guest_smoke_20260101_Passed.json";
        File.WriteAllText(Path.Combine(exportEvidenceDir, evidenceFile), "{}");
        File.WriteAllText(
            Path.Combine(exportDir, "manifest.json"),
            $$"""
            {
              "RecordedAtUtc": "2020-01-01T00:00:00+00:00",
              "Harnesses": [
                { "Harness": "pregame_guest_smoke", "Status": "Passed", "EvidenceFile": "{{evidenceFile}}" }
              ]
            }
            """);

        try
        {
            var freshness = CrossLanguageSoakGate.EvaluateExportBundleDualFreshness(
                exportDir,
                DateTimeOffset.UtcNow,
                maxManifestAgeDays: 8,
                maxEvidenceAgeDays: 8);
            Assert.Equal(CrossLanguageSoakGateStatus.Failed, freshness.Status);
            Assert.Contains("manifest stale", freshness.Reason);

            var ex = Assert.Throws<InvalidOperationException>(
                () => CrossLanguageSoakEvidenceArchive.ApplyExportedTemplates(exportDir, Path.Combine(baseDir, "repo")));
            Assert.Contains("manifest stale", ex.Message);
        }
        finally
        {
            if (Directory.Exists(baseDir))
                Directory.Delete(baseDir, recursive: true);
        }
    }

    [Fact]
    public void ApplyExportedTemplates_RejectsStaleBundleEvidence()
    {
        var baseDir = Path.Combine(Path.GetTempPath(), $"hcde-soak-stale-bundle-{Guid.NewGuid():N}");
        var exportDir = Path.Combine(baseDir, "artifact");
        var exportEvidenceDir = Path.Combine(exportDir, "evidence");
        Directory.CreateDirectory(exportEvidenceDir);
        var evidenceFile = "pregame_guest_smoke_20200101_Passed.json";
        File.WriteAllText(Path.Combine(exportEvidenceDir, evidenceFile), "{}");
        File.SetLastWriteTimeUtc(Path.Combine(exportEvidenceDir, evidenceFile), DateTime.UtcNow.AddDays(-30));
        File.WriteAllText(
            Path.Combine(exportDir, "manifest.json"),
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
            var freshness = CrossLanguageSoakGate.EvaluateExportBundleEvidenceFreshness(
                exportDir,
                DateTimeOffset.UtcNow,
                maxAgeDays: 8);
            Assert.Equal(CrossLanguageSoakGateStatus.Failed, freshness.Status);
            Assert.Contains("export bundle evidence stale", freshness.Reason);

            var ex = Assert.Throws<InvalidOperationException>(
                () => CrossLanguageSoakEvidenceArchive.ApplyExportedTemplates(exportDir, Path.Combine(baseDir, "repo")));
            Assert.Contains("export bundle evidence stale", ex.Message);
        }
        finally
        {
            if (Directory.Exists(baseDir))
                Directory.Delete(baseDir, recursive: true);
        }
    }

    [Fact]
    public void ApplyExportedTemplates_CopiesBundleIntoCommittedTree()
    {
        var baseDir = Path.Combine(Path.GetTempPath(), $"hcde-soak-apply-{Guid.NewGuid():N}");
        var repositoryRoot = Path.Combine(baseDir, "repo");
        var evidenceDir = Path.Combine(repositoryRoot, "csharp", "validation", "soak", "evidence");
        Directory.CreateDirectory(evidenceDir);
        File.WriteAllText(Path.Combine(repositoryRoot, "README.md"), "test");
        File.WriteAllText(Path.Combine(evidenceDir, "stale.json"), "{}");

        var exportDir = Path.Combine(baseDir, "artifact");
        var exportEvidenceDir = Path.Combine(exportDir, "evidence");
        Directory.CreateDirectory(exportEvidenceDir);
        File.WriteAllText(Path.Combine(exportEvidenceDir, "pregame_guest_smoke_test_Passed.json"), "{}");
        File.WriteAllText(
            Path.Combine(exportDir, "manifest.json"),
            $$"""
            {
              "RecordedAtUtc": "{{DateTimeOffset.UtcNow:O}}",
              "Harnesses": [
                {
                  "Harness": "pregame_guest_smoke",
                  "Status": "Passed",
                  "EvidenceFile": "pregame_guest_smoke_test_Passed.json"
                }
              ]
            }
            """);

        try
        {
            var copied = CrossLanguageSoakEvidenceArchive.ApplyExportedTemplates(exportDir, repositoryRoot);
            Assert.Contains(copied, path => path.EndsWith("manifest.json"));
            Assert.False(File.Exists(Path.Combine(evidenceDir, "stale.json")));
            Assert.True(File.Exists(Path.Combine(evidenceDir, "pregame_guest_smoke_test_Passed.json")));
        }
        finally
        {
            if (Directory.Exists(baseDir))
                Directory.Delete(baseDir, recursive: true);
        }
    }

    [Fact]
    public void TryApplyExportedTemplatesFromEnvironment_ReturnsNotRequiredWhenUnset()
    {
        if (Environment.GetEnvironmentVariable("HCDE_APPLY_SOAK_TEMPLATES") == "1")
            return;

        var result = CrossLanguageSoakEvidenceArchive.TryApplyExportedTemplatesFromEnvironment();
        Assert.Equal(CrossLanguageSoakGateStatus.NotRequired, result.Status);
    }

    [Fact]
    public void TryRecordPassedValidationEvidence_ReturnsNotRequiredWhenSecretsMissing()
    {
        if (CrossLanguageSoakGate.AreSoakSecretsConfigured())
            return;

        var result = CrossLanguageSoakEvidenceArchive.TryRecordPassedValidationEvidence();
        Assert.Equal(CrossLanguageSoakGateStatus.NotRequired, result.Status);
    }

    [Fact]
    public void TryRecordPassedValidationEvidence_PassesGateWhenBinariesPresent()
    {
        if (!CrossLanguageSoakGate.AreSoakSecretsConfigured())
            return;

        if (Environment.GetEnvironmentVariable("HCDE_RECORD_PASSED_VALIDATION_EVIDENCE") != "1")
            return;

        var result = CrossLanguageSoakEvidenceArchive.TryRecordPassedValidationEvidence();
        Assert.Equal(CrossLanguageSoakGateStatus.Passed, result.Status);
        Assert.True(File.Exists(CrossLanguageSoakManifest.ResolveDefaultManifestPath()));
        var manifest = File.ReadAllText(CrossLanguageSoakManifest.ResolveDefaultManifestPath());
        Assert.Contains("\"Passed\"", manifest);
    }

    [Fact]
    public void RefreshCommittedEvidence_CopiesManifestAndEvidence()
    {
        var baseDir = Path.Combine(Path.GetTempPath(), $"hcde-soak-refresh-{Guid.NewGuid():N}");
        var repositoryRoot = Path.Combine(baseDir, "repo");
        var evidenceDir = Path.Combine(repositoryRoot, "csharp", "validation", "soak", "evidence");
        Directory.CreateDirectory(evidenceDir);
        File.WriteAllText(Path.Combine(repositoryRoot, "README.md"), "test");
        File.WriteAllText(Path.Combine(evidenceDir, "pregame_guest_smoke_19990101_000000_Skipped.json"), "{}");

        try
        {
            var files = CrossLanguageSoakEvidenceArchive.RefreshCommittedEvidence(repositoryRoot);
            Assert.Equal(2, files.Count);
            Assert.False(File.Exists(Path.Combine(evidenceDir, "pregame_guest_smoke_19990101_000000_Skipped.json")));
            Assert.True(File.Exists(CrossLanguageSoakManifest.ResolveDefaultManifestPath(repositoryRoot)));
            Assert.All(files, path => Assert.StartsWith(evidenceDir, Path.GetDirectoryName(path)!));
        }
        finally
        {
            if (Directory.Exists(baseDir))
                Directory.Delete(baseDir, recursive: true);
        }
    }

    [Fact]
    public void RefreshCommittedEvidence_ReplacesStaleHarnessFiles()
    {
        if (Environment.GetEnvironmentVariable("HCDE_REFRESH_SOAK_TEMPLATES") != "1")
            return;

        var evidenceDir = CrossLanguageSoakEvidenceArchive.ResolveDefaultEvidenceDirectory();
        Directory.CreateDirectory(evidenceDir);
        var stalePath = Path.Combine(evidenceDir, "pregame_guest_smoke_19990101_000000_Skipped.json");
        File.WriteAllText(stalePath, "{}");

        var files = CrossLanguageSoakEvidenceArchive.RefreshCommittedEvidence();
        Assert.Equal(2, files.Count);
        Assert.False(File.Exists(stalePath));
        Assert.All(files, path => Assert.StartsWith(evidenceDir, Path.GetDirectoryName(path)!));
        Assert.True(File.Exists(CrossLanguageSoakManifest.ResolveDefaultManifestPath()));
    }
}
