namespace HCDE.Net.Core.Tests;

public class NetcodeCrossLanguageTests
{
    [Fact]
    public void SkipsUnlessNetcodeSoakConfigured()
    {
        var serverPath = Environment.GetEnvironmentVariable("HCDE_HCDESERV_PATH");
        var iwadPath = Environment.GetEnvironmentVariable("HCDE_IWAD_PATH");
        if (string.IsNullOrWhiteSpace(serverPath) || string.IsNullOrWhiteSpace(iwadPath))
            return;

        Assert.True(File.Exists(serverPath));
        Assert.True(File.Exists(iwadPath));
    }
}
