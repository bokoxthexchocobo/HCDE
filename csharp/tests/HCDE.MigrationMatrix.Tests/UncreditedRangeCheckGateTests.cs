using System.Diagnostics;

namespace HCDE.MigrationMatrix.Tests;

public class UncreditedRangeCheckGateTests
{
    static string RepoRoot => NativeManagedMatrixTests.FindRepoRootPublic();

    static (int ExitCode, string Output) RunUncredited(params string[] args)
    {
        var script = Path.Combine(RepoRoot, "csharp", "tools", "uncredited_range_check.py");
        var proc = Process.Start(new ProcessStartInfo
        {
            FileName = "python3",
            Arguments = $"\"{script}\" {string.Join(" ", args)}",
            WorkingDirectory = RepoRoot,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        }) ?? throw new InvalidOperationException("Failed to start uncredited_range_check.");
        var stdout = proc.StandardOutput.ReadToEnd();
        var stderr = proc.StandardError.ReadToEnd();
        proc.WaitForExit();
        return (proc.ExitCode, stdout + stderr);
    }

    [Fact]
    public void Chunk883_EachNativeRangeRowIsUncredited()
    {
        var (code, output) = RunUncredited("--chunk", "883");
        Assert.Equal(0, code);
        Assert.Contains("[UNCredited] DBot::Reachable", output);
        Assert.Contains("45-124", output);
        Assert.DoesNotContain("[OVERLAP] DBot::Reachable", output);
    }

    [Fact]
    public void OverlappingBandStillFails()
    {
        var (code, output) = RunUncredited(
            "--file", "src/playsim/bots/b_func.cpp",
            "--start", "134", "--end", "144");
        Assert.Equal(1, code);
        Assert.Contains("[OVERLAP]", output);
        Assert.Contains("bot-func-sight-mate-enemy-chunk879", output);
    }

    [Fact]
    public void BandEnvelopeFlagsGapCreditBetween883Bands()
    {
        var (code, output) = RunUncredited("--chunk", "883", "--band");
        Assert.Equal(1, code);
        Assert.Contains("[OVERLAP]", output);
        Assert.Contains("bot-func-sight-mate-enemy-chunk879", output);
    }

    [Fact]
    public void LandGateUsesPerNativeRangeRowsNotEnvelope()
    {
        var land = File.ReadAllText(Path.Combine(RepoRoot, "csharp", "tools", "land_chunk_fast.sh"));
        var slice = land.Split("uncredited_range_check.py", 2)[1];
        Assert.DoesNotContain("--band", slice[..Math.Min(400, slice.Length)]);
    }
}
