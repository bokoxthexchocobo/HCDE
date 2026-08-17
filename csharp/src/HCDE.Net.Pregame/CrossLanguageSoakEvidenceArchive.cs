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

    public static IReadOnlyList<string> RefreshCommittedEvidence(string? repositoryRoot = null)
    {
        repositoryRoot ??= FindRepositoryRoot();
        var evidenceDirectory = ResolveDefaultEvidenceDirectory(repositoryRoot);
        Directory.CreateDirectory(evidenceDirectory);
        PruneHarnessEvidenceFiles(evidenceDirectory);
        return RecordEvidence(evidenceDirectory, repositoryRoot);
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
