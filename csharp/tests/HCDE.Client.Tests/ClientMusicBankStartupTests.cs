namespace HCDE.Client.Tests;

public class ClientMusicBankStartupTests
{
    [Fact]
    public void LauncherMusicOptionForwardsConfigurationForOrdinaryWadWithoutCatalog()
    {
        var root = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"))).FullName;
        try
        {
            var wad = Path.Combine(root, "game.wad");
            File.WriteAllBytes(wad, [80, 87, 65, 68, 0, 0, 0, 0, 12, 0, 0, 0]);
            var bankDirectory = Directory.CreateDirectory(Path.Combine(root, "banks")).FullName;
            var bank = Path.Combine(bankDirectory, "bank.sf2");
            File.WriteAllText(bank, "RIFF0000sfbkLIST");
            var config = Path.Combine(root, "paths.ini");
            File.WriteAllText(config, "[SoundfontSearch.Directories]\nPath=" + bankDirectory);
            Assert.True(LauncherPlan.TryParse(["--iwad", wad, "--search-config", config, "--music-banks"], out var selection, out _));
            Assert.True(LauncherPlan.TryParse(LauncherPlan.BuildArgs(selection), out var copy, out _));
            Assert.True(copy.LoadMusicBanks);
            using var demo = ClientHost.DemoRoom();
            using var host = LauncherPlan.CreateHost(copy, demo.Simulation);
            Assert.Equal(bank, Assert.Single(host.MusicBanks).Path);
            using var disabled = LauncherPlan.CreateHost(new LauncherSelection { IwadPath = wad, SearchPathConfigPath = config }, demo.Simulation);
            Assert.Empty(disabled.MusicBanks);
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public void HostStartupDiscoversConfiguredBankAndOpensReaderThroughCallbacks()
    {
        var root = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"))).FullName;
        try
        {
            var bank = Path.Combine(root, "bank.sf2");
            File.WriteAllText(bank, "RIFF0000sfbkLIST");
            var config = VmSearchPathConfiguration.Parse("[SoundfontSearch.Directories]\nPath=" + root);
            using var demo = ClientHost.DemoRoom();
            using var host = new ClientHost(demo.Simulation, musicConfiguration: config);
            Assert.Equal(bank, Assert.Single(host.MusicBanks).Path);
            Assert.Equal(bank, host.MusicCallbacks.PathToBank("bank", VmMusicBankFormat.Sf2));
            using var handle = host.MusicCallbacks.OpenBank("bank", VmMusicBankFormat.Sf2)!;
            Assert.Contains(bank, System.Text.Encoding.UTF8.GetString(handle.OpenFile(null).Data!));
            using var bytes = handle.OpenByteReader(bank)!;
            Assert.Equal(16, bytes.Read(new byte[16]));
            Assert.Empty(demo.MusicBanks);
            host.Dispose();
            Assert.Throws<ObjectDisposedException>(() => handle.OpenFile(null));
            Assert.Throws<ObjectDisposedException>(() => host.MusicCallbacks.OpenBank("bank", VmMusicBankFormat.Sf2));
            Assert.Equal(0, bytes.Seek(0, SeekOrigin.Begin));
        }
        finally { Directory.Delete(root, recursive: true); }
    }
}
