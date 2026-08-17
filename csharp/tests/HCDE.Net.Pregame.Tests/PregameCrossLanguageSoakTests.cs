using HCDE.Net.Core;
using HCDE.Net.Pregame;

namespace HCDE.Net.Pregame.Tests;

[Collection("CrossLanguageSoak")]
public class PregameCrossLanguageSoakTests
{
    [Fact]
    public void RunPregameGuestSmoke_SkipsWhenNotConfigured()
    {
        var serverPath = Environment.GetEnvironmentVariable("HCDE_HCDESERV_PATH");
        var iwadPath = Environment.GetEnvironmentVariable("HCDE_IWAD_PATH");
        if (!string.IsNullOrWhiteSpace(serverPath) && !string.IsNullOrWhiteSpace(iwadPath))
            return;

        var result = PregameCrossLanguageSoak.RunPregameGuestSmoke();
        Assert.Equal(CrossLanguageSoakStatus.Skipped, result.Status);
        Assert.NotNull(result.SkipReason);
    }

    [Fact]
    public void RunPregameGuestSmoke_PassesWhenConfigured()
    {
        var serverPath = Environment.GetEnvironmentVariable("HCDE_HCDESERV_PATH");
        var iwadPath = Environment.GetEnvironmentVariable("HCDE_IWAD_PATH");
        if (string.IsNullOrWhiteSpace(serverPath) || string.IsNullOrWhiteSpace(iwadPath))
            return;

        var result = PregameCrossLanguageSoak.RunPregameGuestSmoke();
        if (result.Status == CrossLanguageSoakStatus.Skipped)
            return;

        Assert.Equal(CrossLanguageSoakStatus.Passed, result.Status);
    }
}
