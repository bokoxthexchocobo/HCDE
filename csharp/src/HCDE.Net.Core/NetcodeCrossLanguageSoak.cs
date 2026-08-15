using System.Diagnostics;

namespace HCDE.Net.Core;

public static class NetcodeCrossLanguageSoak
{
    public static CrossLanguageSoakResult RunStep12InvasionSmoke(string? repositoryRoot = null)
    {
        var serverPath = Environment.GetEnvironmentVariable("HCDE_HCDESERV_PATH");
        var iwadPath = Environment.GetEnvironmentVariable("HCDE_IWAD_PATH");
        if (string.IsNullOrWhiteSpace(serverPath) || string.IsNullOrWhiteSpace(iwadPath))
        {
            return CrossLanguageSoakEvidence.Finalize(
                "netcode_step12_invasion",
                new CrossLanguageSoakResult(
                    CrossLanguageSoakStatus.Skipped,
                    output: string.Empty,
                    skipReason: "Set HCDE_HCDESERV_PATH and HCDE_IWAD_PATH to run the cross-language netcode soak."));
        }

        if (!File.Exists(serverPath) || !File.Exists(iwadPath))
        {
            return CrossLanguageSoakEvidence.Finalize(
                "netcode_step12_invasion",
                new CrossLanguageSoakResult(
                    CrossLanguageSoakStatus.Skipped,
                    output: string.Empty,
                    skipReason: "Configured HCDE_HCDESERV_PATH or HCDE_IWAD_PATH does not exist on disk."));
        }

        repositoryRoot ??= FindRepositoryRoot();
        var scriptPath = Path.Combine(repositoryRoot, "tests", "netcode_step12", "netcode_step12_stress.py");
        if (!File.Exists(scriptPath))
        {
            return CrossLanguageSoakEvidence.Finalize(
                "netcode_step12_invasion",
                new CrossLanguageSoakResult(
                    CrossLanguageSoakStatus.Failed,
                    $"netcode step12 script not found: {scriptPath}"));
        }

        var arguments = new List<string>
        {
            scriptPath,
            "--server",
            serverPath,
            "--iwad",
            iwadPath,
            "--cases",
            "invasion",
            "--duration",
            "20",
            "--wave-pulses",
            "2",
        };

        var clientPath = Environment.GetEnvironmentVariable("HCDE_HCDE_CLIENT_PATH");
        if (!string.IsNullOrWhiteSpace(clientPath) && File.Exists(clientPath))
        {
            arguments.AddRange(["--client", clientPath, "--client-count", "1"]);
        }

        return RunPython(repositoryRoot, arguments);
    }

    private static CrossLanguageSoakResult RunPython(string repositoryRoot, IReadOnlyList<string> arguments)
    {
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
                "netcode_step12_invasion",
                new CrossLanguageSoakResult(
                    CrossLanguageSoakStatus.Failed,
                    "Failed to start python3 for netcode cross-language soak."));
        }

        var stdout = process.StandardOutput.ReadToEnd();
        var stderr = process.StandardError.ReadToEnd();
        process.WaitForExit();
        var output = string.IsNullOrWhiteSpace(stderr) ? stdout : $"{stdout}\n{stderr}";
        var result = process.ExitCode == 0
            ? new CrossLanguageSoakResult(CrossLanguageSoakStatus.Passed, output)
            : new CrossLanguageSoakResult(CrossLanguageSoakStatus.Failed, output);
        return CrossLanguageSoakEvidence.Finalize("netcode_step12_invasion", result);
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
