using System.Diagnostics;
using HCDE.Net.Core;

namespace HCDE.Net.Pregame;

public static class PregameCrossLanguageSoak
{
    public static CrossLanguageSoakResult RunPregameGuestSmoke(string? repositoryRoot = null)
    {
        var serverPath = Environment.GetEnvironmentVariable("HCDE_HCDESERV_PATH");
        var iwadPath = Environment.GetEnvironmentVariable("HCDE_IWAD_PATH");
        if (string.IsNullOrWhiteSpace(serverPath) || string.IsNullOrWhiteSpace(iwadPath))
        {
            return CrossLanguageSoakEvidence.Finalize(
                "pregame_guest_smoke",
                new CrossLanguageSoakResult(
                    CrossLanguageSoakStatus.Skipped,
                    output: string.Empty,
                    skipReason: "Set HCDE_HCDESERV_PATH and HCDE_IWAD_PATH to run the cross-language pregame soak."));
        }

        if (!File.Exists(serverPath) || !File.Exists(iwadPath))
        {
            return CrossLanguageSoakEvidence.Finalize(
                "pregame_guest_smoke",
                new CrossLanguageSoakResult(
                    CrossLanguageSoakStatus.Skipped,
                    output: string.Empty,
                    skipReason: "Configured HCDE_HCDESERV_PATH or HCDE_IWAD_PATH does not exist on disk."));
        }

        repositoryRoot ??= FindRepositoryRoot();
        var scriptPath = Path.Combine(repositoryRoot, "csharp", "validation", "pregame", "pregame_guest_smoke.py");
        if (!File.Exists(scriptPath))
        {
            return CrossLanguageSoakEvidence.Finalize(
                "pregame_guest_smoke",
                new CrossLanguageSoakResult(
                    CrossLanguageSoakStatus.Failed,
                    $"pregame smoke script not found: {scriptPath}"));
        }

        var arguments = new List<string>
        {
            scriptPath,
            "--server",
            serverPath,
            "--iwad",
            iwadPath,
        };

        var wadCrcs = Environment.GetEnvironmentVariable("HCDE_IWAD_CRC");
        if (!string.IsNullOrWhiteSpace(wadCrcs))
        {
            foreach (var crc in wadCrcs.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                arguments.AddRange(["--wad-crc", crc]);
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = "python3",
            WorkingDirectory = repositoryRoot,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        foreach (var argument in arguments)
            startInfo.ArgumentList.Add(argument);

        using var process = Process.Start(startInfo);
        if (process is null)
        {
            return CrossLanguageSoakEvidence.Finalize(
                "pregame_guest_smoke",
                new CrossLanguageSoakResult(
                    CrossLanguageSoakStatus.Failed,
                    "Failed to start python3 for pregame cross-language soak."));
        }

        var stdout = process.StandardOutput.ReadToEnd();
        var stderr = process.StandardError.ReadToEnd();
        process.WaitForExit();
        var output = string.IsNullOrWhiteSpace(stderr) ? stdout : $"{stdout}\n{stderr}";
        var result = process.ExitCode == 0
            ? new CrossLanguageSoakResult(CrossLanguageSoakStatus.Passed, output)
            : new CrossLanguageSoakResult(CrossLanguageSoakStatus.Failed, output);
        return CrossLanguageSoakEvidence.Finalize("pregame_guest_smoke", result);
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
