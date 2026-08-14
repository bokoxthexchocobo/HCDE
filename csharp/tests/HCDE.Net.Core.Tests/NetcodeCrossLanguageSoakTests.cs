namespace HCDE.Net.Core.Tests;

public class NetcodeCrossLanguageSoakTests
{
    [Fact]
    public void RunStep12InvasionSmoke_SkipsWhenNotConfigured()
    {
        var serverPath = Environment.GetEnvironmentVariable("HCDE_HCDESERV_PATH");
        var iwadPath = Environment.GetEnvironmentVariable("HCDE_IWAD_PATH");
        if (!string.IsNullOrWhiteSpace(serverPath) && !string.IsNullOrWhiteSpace(iwadPath))
            return;

        var result = NetcodeCrossLanguageSoak.RunStep12InvasionSmoke();
        Assert.Equal(CrossLanguageSoakStatus.Skipped, result.Status);
        Assert.NotNull(result.SkipReason);
    }

    [Fact]
    public void RunStep12InvasionSmoke_PassesWhenConfigured()
    {
        var serverPath = Environment.GetEnvironmentVariable("HCDE_HCDESERV_PATH");
        var iwadPath = Environment.GetEnvironmentVariable("HCDE_IWAD_PATH");
        if (string.IsNullOrWhiteSpace(serverPath) || string.IsNullOrWhiteSpace(iwadPath))
            return;

        var result = NetcodeCrossLanguageSoak.RunStep12InvasionSmoke();
        if (result.Status == CrossLanguageSoakStatus.Skipped)
            return;

        Assert.Equal(CrossLanguageSoakStatus.Passed, result.Status);
    }
}
