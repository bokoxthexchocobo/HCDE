namespace HCDE.Server.Tests;

public class DedicatedServerCommandLineTests
{
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

    private static string CreateTempIwad()
    {
        var path = Path.Combine(Path.GetTempPath(), $"hcde-iwad-{Guid.NewGuid():N}.wad");
        File.WriteAllBytes(path, [0x49, 0x57, 0x41, 0x44]);
        return path;
    }
}
