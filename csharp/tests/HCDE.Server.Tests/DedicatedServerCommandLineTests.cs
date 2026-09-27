namespace HCDE.Server.Tests;

public class DedicatedServerCommandLineTests
{
    [Fact]
    public void TryParse_ConfiguresInvasionModeWaveLimitAndTimers()
    {
        var iwad = CreateTempIwad();
        try
        {
            Assert.True(DedicatedServerCommandLine.TryParse(
                ["--iwad", iwad, "--gamemode", "4", "--invasion-waves", "5", "--invasion-countdown", "2", "--invasion-intermission", "3"],
                out var options, out var error), error);
            Assert.Equal("Invasion", options.GameModeName);
            Assert.Equal(5, options.InvasionWaves);
            Assert.Equal(70, options.InvasionCountdownTics);
            Assert.Equal(105, options.InvasionIntermissionTics);
        }
        finally { File.Delete(iwad); }
    }

    [Theory]
    [InlineData("--invasion-waves", "0")]
    [InlineData("--invasion-waves", "65536")]
    [InlineData("--invasion-countdown", "-1")]
    [InlineData("--invasion-intermission", "3601")]
    [InlineData("--invasion-countdown", "many")]
    public void TryParse_RejectsInvalidInvasionSettings(string option, string value)
    {
        Assert.False(DedicatedServerCommandLine.TryParse([option, value], out _, out var error));
        Assert.StartsWith(option, error);
    }

    [Fact]
    public void TryParse_EnablesMasterAdvertiseWithDefaults()
    {
        var iwad = CreateTempIwad();
        try
        {
            Assert.True(DedicatedServerCommandLine.TryParse(
                ["--iwad", iwad, "--master"],
                out var options,
                out var error),
                error);

            Assert.True(options.EnableMasterAdvertise);
            Assert.Equal("hcde.servebeer.com", options.MasterHost);
            Assert.Equal(15000, options.MasterPort);
        }
        finally
        {
            File.Delete(iwad);
        }
    }

    [Fact]
    public void TryParse_ParsesMasterHostAndPort()
    {
        var iwad = CreateTempIwad();
        try
        {
            Assert.True(DedicatedServerCommandLine.TryParse(
                ["--iwad", iwad, "--master", "127.0.0.1:15001"],
                out var options,
                out _));

            Assert.True(options.EnableMasterAdvertise);
            Assert.Equal("127.0.0.1", options.MasterHost);
            Assert.Equal(15001, options.MasterPort);
        }
        finally
        {
            File.Delete(iwad);
        }
    }

    [Fact]
    public void TryParse_AppliesPublicQuerySnapshotFields()
    {
        var iwad = CreateTempIwad();
        try
        {
            Assert.True(DedicatedServerCommandLine.TryParse(
                [
                    "--iwad", iwad,
                    "--server-name", "Iter33 Host",
                    "--skill", "4",
                    "--deathmatch",
                    "--teamplay",
                    "--gamemode", "2",
                    "--gamemode-name", "Invasion",
                    "--no-query",
                ],
                out var options,
                out _));

            Assert.Equal("Iter33 Host", options.ServerName);
            Assert.Equal((byte)4, options.Skill);
            Assert.True(options.Deathmatch);
            Assert.True(options.Teamplay);
            Assert.Equal((byte)2, options.GameMode);
            Assert.Equal("Invasion", options.GameModeName);
            Assert.False(options.EnableServerQuery);
        }
        finally
        {
            File.Delete(iwad);
        }
    }

    [Fact]
    public void TryParse_ReadsRconPasswordAndPort()
    {
        var iwad = CreateTempIwad();
        try
        {
            Assert.True(DedicatedServerCommandLine.TryParse(
                ["--iwad", iwad, "--rcon-password", "secret", "--rcon-port", "10667"],
                out var options,
                out var error),
                error);
            Assert.Equal("secret", options.RconPassword);
            Assert.Equal(10667, options.RconPort);
        }
        finally
        {
            File.Delete(iwad);
        }
    }

    private static string CreateTempIwad()
    {
        var path = Path.Combine(Path.GetTempPath(), $"hcde-iwad-{Guid.NewGuid():N}.wad");
        File.WriteAllBytes(path, [0x49, 0x57, 0x41, 0x44]);
        return path;
    }
}
