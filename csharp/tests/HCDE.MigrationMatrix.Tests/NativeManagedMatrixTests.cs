using System.Diagnostics;
using System.Text.Json;

namespace HCDE.MigrationMatrix.Tests;

public class NativeManagedMatrixTests
{
    [Fact]
    public void CommittedMatrixMatchesGenerator()
    {
        var repo = FindRepoRoot();
        var script = Path.Combine(repo, "csharp", "tools", "generate_native_managed_matrix.py");
        var jsonPath = Path.Combine(repo, "csharp", "docs", "data", "native_managed_matrix.json");
        var proc = Process.Start(new ProcessStartInfo
        {
            FileName = "python3",
            Arguments = $"\"{script}\"",
            WorkingDirectory = repo,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        }) ?? throw new InvalidOperationException("Failed to start matrix generator.");
        proc.WaitForExit();
        Assert.True(proc.ExitCode == 0, proc.StandardError.ReadToEnd());
        var generated = JsonDocument.Parse(File.ReadAllText(jsonPath));
        var committed = JsonDocument.Parse(File.ReadAllText(jsonPath));
        Assert.Equal(
            committed.RootElement.GetProperty("summaries").GetRawText(),
            generated.RootElement.GetProperty("summaries").GetRawText());
    }

    [Fact]
    public void MatrixContainsExpectedSections()
    {
        var repo = FindRepoRoot();
        var jsonPath = Path.Combine(repo, "csharp", "docs", "data", "native_managed_matrix.json");
        using var doc = JsonDocument.Parse(File.ReadAllText(jsonPath));
        Assert.True(doc.RootElement.GetProperty("locScope").GetProperty("nativeEngineLoc").GetInt32() > 400_000);
        Assert.True(doc.RootElement.GetProperty("acsPcode").GetArrayLength() > 300);
    }

    private static string FindRepoRoot() => FindRepoRootPublic();

    internal static string FindRepoRootPublic()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "csharp", "HCDE.sln"))) return dir.FullName;
            dir = dir.Parent;
        }
        throw new InvalidOperationException("Repository root not found.");
    }
}
