using HCDE.Net.Core;

namespace HCDE.Net.Pregame;

public static class CrossLanguageSoakEvidenceArchive
{
    private static readonly string[] KnownHarnesses =
    [
        "pregame_guest_smoke",
        "netcode_step12_invasion",
    ];

    public static IReadOnlyList<string> RecordEvidence(string evidenceDirectory, string? repositoryRoot = null)
    {
        Directory.CreateDirectory(evidenceDirectory);
        var priorEvidenceDir = Environment.GetEnvironmentVariable("HCDE_SOAK_EVIDENCE_DIR");
        Environment.SetEnvironmentVariable("HCDE_SOAK_EVIDENCE_DIR", evidenceDirectory);
        try
        {
            var results = CrossLanguageSoakSuite.RunAll(repositoryRoot);
            var files = Directory.GetFiles(evidenceDirectory, "*.json").OrderBy(path => path).ToArray();
            var manifestPath = Path.Combine(Directory.GetParent(evidenceDirectory)!.FullName, "manifest.json");
            CrossLanguageSoakManifest.Write(manifestPath, results, files);
            return files;
        }
        finally
        {
            Environment.SetEnvironmentVariable("HCDE_SOAK_EVIDENCE_DIR", priorEvidenceDir);
        }
    }

    public static string ResolveDefaultEvidenceDirectory(string? repositoryRoot = null)
    {
        repositoryRoot ??= FindRepositoryRoot();
        return Path.Combine(repositoryRoot, "csharp", "validation", "soak", "evidence");
    }

    public static IReadOnlyList<string> RecordDefaultEvidence(string? repositoryRoot = null)
    {
        return RecordEvidence(ResolveDefaultEvidenceDirectory(repositoryRoot), repositoryRoot);
    }

    public static IReadOnlyList<string> RecordValidationEvidence(string? repositoryRoot = null)
    {
        return RecordDefaultEvidence(repositoryRoot);
    }

    public static IReadOnlyList<string> RecordValidationPassedEvidence(string? repositoryRoot = null)
    {
        if (Environment.GetEnvironmentVariable("HCDE_RECORD_VALIDATION_EVIDENCE") != "1")
            return Array.Empty<string>();

        return RecordValidationEvidence(repositoryRoot);
    }

    public static CrossLanguageSoakGateResult TryRecordValidationPassedEvidence(string? repositoryRoot = null)
    {
        if (Environment.GetEnvironmentVariable("HCDE_RECORD_VALIDATION_EVIDENCE") != "1")
        {
            return new CrossLanguageSoakGateResult(
                CrossLanguageSoakGateStatus.NotRequired,
                "record validation evidence not requested");
        }

        var files = RecordValidationEvidence(repositoryRoot);
        if (files.Count == 0)
        {
            return new CrossLanguageSoakGateResult(
                CrossLanguageSoakGateStatus.Failed,
                "validation evidence recording produced no files");
        }

        if (!File.Exists(CrossLanguageSoakManifest.ResolveDefaultManifestPath(repositoryRoot)))
        {
            return new CrossLanguageSoakGateResult(
                CrossLanguageSoakGateStatus.Failed,
                "validation evidence manifest missing after recording");
        }

        if (CrossLanguageSoakGate.AreSoakSecretsConfigured())
        {
            if (files.Any(path => !path.Contains("_Passed.json", StringComparison.Ordinal)))
            {
                return new CrossLanguageSoakGateResult(
                    CrossLanguageSoakGateStatus.Failed,
                    "validation evidence recording did not produce passed harness files");
            }

            return CrossLanguageSoakGate.Evaluate(repositoryRoot, requireConfiguredSecrets: true);
        }

        if (files.Any(path => !path.Contains("_Skipped.json", StringComparison.Ordinal)))
        {
            return new CrossLanguageSoakGateResult(
                CrossLanguageSoakGateStatus.Failed,
                "validation evidence recording did not produce skipped harness files");
        }

        return new CrossLanguageSoakGateResult(
            CrossLanguageSoakGateStatus.Passed,
            "validation passed evidence recorded");
    }

    public static CrossLanguageSoakGateResult TryRecordValidationSkippedEvidence(string? repositoryRoot = null)
    {
        if (Environment.GetEnvironmentVariable("HCDE_RECORD_VALIDATION_EVIDENCE") != "1")
        {
            return new CrossLanguageSoakGateResult(
                CrossLanguageSoakGateStatus.NotRequired,
                "record validation evidence not requested");
        }

        var files = RecordValidationEvidence(repositoryRoot);
        if (files.Count == 0)
        {
            return new CrossLanguageSoakGateResult(
                CrossLanguageSoakGateStatus.Failed,
                "validation evidence recording produced no files");
        }

        if (files.Any(path => !path.Contains("_Skipped.json", StringComparison.Ordinal)))
        {
            return new CrossLanguageSoakGateResult(
                CrossLanguageSoakGateStatus.Failed,
                "validation evidence recording did not produce skipped harness files");
        }

        if (!File.Exists(CrossLanguageSoakManifest.ResolveDefaultManifestPath(repositoryRoot)))
        {
            return new CrossLanguageSoakGateResult(
                CrossLanguageSoakGateStatus.Failed,
                "validation evidence manifest missing after recording");
        }

        return new CrossLanguageSoakGateResult(
            CrossLanguageSoakGateStatus.Passed,
            "validation skipped evidence recorded");
    }

    public static IReadOnlyList<string> RefreshCommittedEvidence(string? repositoryRoot = null)
    {
        repositoryRoot ??= FindRepositoryRoot();
        var evidenceDirectory = ResolveDefaultEvidenceDirectory(repositoryRoot);
        Directory.CreateDirectory(evidenceDirectory);
        PruneHarnessEvidenceFiles(evidenceDirectory);
        return RecordEvidence(evidenceDirectory, repositoryRoot);
    }

    public static CrossLanguageSoakGateResult TryRecordPassedValidationEvidence(string? repositoryRoot = null)
    {
        if (!CrossLanguageSoakGate.AreSoakSecretsConfigured())
        {
            return new CrossLanguageSoakGateResult(
                CrossLanguageSoakGateStatus.NotRequired,
                "soak secrets not configured");
        }

        RefreshCommittedEvidence(repositoryRoot);
        return CrossLanguageSoakGate.Evaluate(repositoryRoot, requireConfiguredSecrets: true);
    }

    public static IReadOnlyList<string> ExportCommittedTemplates(string outputDirectory, string? repositoryRoot = null)
    {
        repositoryRoot ??= FindRepositoryRoot();
        var committedFreshness = CrossLanguageSoakGate.EvaluateCommittedDualFreshness(
            repositoryRoot,
            DateTimeOffset.UtcNow,
            CrossLanguageSoakGate.ResolveMaxManifestAgeDays(),
            CrossLanguageSoakGate.ResolveMaxEvidenceAgeDays());
        if (committedFreshness.Status == CrossLanguageSoakGateStatus.Failed)
            throw new InvalidOperationException(committedFreshness.Reason);

        Directory.CreateDirectory(outputDirectory);
        var evidenceDirectory = ResolveDefaultEvidenceDirectory(repositoryRoot);
        var outputEvidenceDirectory = Path.Combine(outputDirectory, "evidence");
        Directory.CreateDirectory(outputEvidenceDirectory);

        var copied = new List<string>();
        if (Directory.Exists(evidenceDirectory))
        {
            foreach (var file in Directory.GetFiles(evidenceDirectory, "*.json"))
            {
                var destination = Path.Combine(outputEvidenceDirectory, Path.GetFileName(file));
                File.Copy(file, destination, overwrite: true);
                copied.Add(destination);
            }
        }

        var manifestPath = CrossLanguageSoakManifest.ResolveDefaultManifestPath(repositoryRoot);
        if (File.Exists(manifestPath))
        {
            var destinationManifest = Path.Combine(outputDirectory, "manifest.json");
            File.Copy(manifestPath, destinationManifest, overwrite: true);
            copied.Add(destinationManifest);
        }

        var instructionsPath = Path.Combine(outputDirectory, "COMMIT_INSTRUCTIONS.md");
        File.WriteAllText(
            instructionsPath,
            """
            # Soak template commit bundle

            Copy this artifact into the repository after a green cross-language soak run:

            - `evidence/*.json` -> `csharp/validation/soak/evidence/`
            - `manifest.json` -> `csharp/validation/soak/manifest.json`
            """);
        copied.Add(instructionsPath);
        return copied;
    }

    public static CrossLanguageSoakGateResult TryExportCommittedTemplatesFromEnvironment(string? repositoryRoot = null)
    {
        if (Environment.GetEnvironmentVariable("HCDE_EXPORT_SOAK_TEMPLATES") != "1")
        {
            return new CrossLanguageSoakGateResult(
                CrossLanguageSoakGateStatus.NotRequired,
                "export soak templates not requested");
        }

        var outputDirectory = Environment.GetEnvironmentVariable("HCDE_SOAK_TEMPLATE_EXPORT_DIR");
        if (string.IsNullOrWhiteSpace(outputDirectory))
        {
            return new CrossLanguageSoakGateResult(
                CrossLanguageSoakGateStatus.Failed,
                "HCDE_SOAK_TEMPLATE_EXPORT_DIR not set");
        }

        try
        {
            ExportCommittedTemplates(outputDirectory, repositoryRoot);
            return new CrossLanguageSoakGateResult(CrossLanguageSoakGateStatus.Passed);
        }
        catch (InvalidOperationException ex)
        {
            return new CrossLanguageSoakGateResult(CrossLanguageSoakGateStatus.Failed, ex.Message);
        }
    }

    public static IReadOnlyList<string> ApplyExportedTemplates(string exportDirectory, string? repositoryRoot = null)
    {
        repositoryRoot ??= FindRepositoryRoot();
        if (!Directory.Exists(exportDirectory))
            throw new DirectoryNotFoundException(exportDirectory);

        var bundleFreshness = CrossLanguageSoakGate.EvaluateExportBundleDualFreshness(
            exportDirectory,
            DateTimeOffset.UtcNow,
            CrossLanguageSoakGate.ResolveMaxManifestAgeDays(),
            CrossLanguageSoakGate.ResolveMaxEvidenceAgeDays());
        if (bundleFreshness.Status == CrossLanguageSoakGateStatus.Failed)
            throw new InvalidOperationException(bundleFreshness.Reason);

        var evidenceDirectory = ResolveDefaultEvidenceDirectory(repositoryRoot);
        Directory.CreateDirectory(evidenceDirectory);
        var copied = new List<string>();

        foreach (var file in Directory.GetFiles(evidenceDirectory, "*.json"))
            File.Delete(file);

        var exportEvidenceDirectory = Path.Combine(exportDirectory, "evidence");
        if (Directory.Exists(exportEvidenceDirectory))
        {
            foreach (var file in Directory.GetFiles(exportEvidenceDirectory, "*.json"))
            {
                var destination = Path.Combine(evidenceDirectory, Path.GetFileName(file));
                File.Copy(file, destination, overwrite: true);
                copied.Add(destination);
            }
        }

        var exportManifest = Path.Combine(exportDirectory, "manifest.json");
        if (File.Exists(exportManifest))
        {
            var destinationManifest = CrossLanguageSoakManifest.ResolveDefaultManifestPath(repositoryRoot);
            Directory.CreateDirectory(Path.GetDirectoryName(destinationManifest)!);
            File.Copy(exportManifest, destinationManifest, overwrite: true);
            copied.Add(destinationManifest);
        }

        return copied;
    }

    public static CrossLanguageSoakGateResult TryApplyExportedTemplatesFromEnvironment(string? repositoryRoot = null)
    {
        if (Environment.GetEnvironmentVariable("HCDE_APPLY_SOAK_TEMPLATES") != "1")
        {
            return new CrossLanguageSoakGateResult(
                CrossLanguageSoakGateStatus.NotRequired,
                "apply soak templates not requested");
        }

        var exportDirectory = Environment.GetEnvironmentVariable("HCDE_SOAK_TEMPLATE_EXPORT_DIR");
        if (string.IsNullOrWhiteSpace(exportDirectory))
        {
            return new CrossLanguageSoakGateResult(
                CrossLanguageSoakGateStatus.Failed,
                "HCDE_SOAK_TEMPLATE_EXPORT_DIR not set");
        }

        var bundleFreshness = CrossLanguageSoakGate.EvaluateExportBundleDualFreshness(
            exportDirectory,
            DateTimeOffset.UtcNow,
            CrossLanguageSoakGate.ResolveMaxManifestAgeDays(),
            CrossLanguageSoakGate.ResolveMaxEvidenceAgeDays());
        if (bundleFreshness.Status == CrossLanguageSoakGateStatus.Failed)
            return bundleFreshness;

        ApplyExportedTemplates(exportDirectory, repositoryRoot);
        if (!CrossLanguageSoakGate.AreSoakSecretsConfigured())
        {
            return new CrossLanguageSoakGateResult(
                CrossLanguageSoakGateStatus.NotRequired,
                "soak secrets not configured");
        }

        return CrossLanguageSoakGate.Evaluate(repositoryRoot, requireConfiguredSecrets: true);
    }

    public static void PruneHarnessEvidenceFiles(string evidenceDirectory)
    {
        if (!Directory.Exists(evidenceDirectory))
            return;

        foreach (var harness in KnownHarnesses)
        {
            foreach (var file in Directory.GetFiles(evidenceDirectory, $"{harness}_*.json"))
                File.Delete(file);
        }
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
