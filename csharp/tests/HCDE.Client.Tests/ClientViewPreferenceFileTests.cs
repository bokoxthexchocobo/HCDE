using System.Text.Json;
using HCDE.MapLoader;
using HCDE.Playsim;

namespace HCDE.Client.Tests;

public sealed class ClientViewPreferenceFileTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), "hcde-view-" + Guid.NewGuid().ToString("N"));
    public ClientViewPreferenceFileTests() => Directory.CreateDirectory(_directory);
    public void Dispose() => Directory.Delete(_directory, recursive: true);
    private string SettingsPath => Path.Combine(_directory, "view.json");
    private static AuthoritySimulation Simulation() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] });

    [Fact]
    public void HostLoadsDefaultsThenSavesAndRestoresOnNextStartup()
    {
        var sim = Simulation(); var before = SimSavegame.Write(sim);
        using (var host = new ClientHost(sim, viewPreferencesPath: SettingsPath))
        {
            Assert.Equal(new ClientViewPreferences(), host.CaptureViewPreferences());
            host.BloodFadeScalar = 0.25; host.SmoothPain = true; host.PainSmoothStrength = 0.9f;
            host.PoisonPaletteFlash = true;
        }
        using var next = new ClientHost(sim, viewPreferencesPath: SettingsPath);
        Assert.Equal(0.25, next.BloodFadeScalar); Assert.True(next.SmoothPain);
        Assert.Equal(0.9f, next.PainSmoothStrength);
        Assert.True(next.PoisonPaletteFlash);
        Assert.Equal(before, SimSavegame.Write(sim));
        Assert.Single(Directory.GetFiles(_directory));
    }

    [Fact]
    public void MalformedFileFailsStartupAndRemainsUntouched()
    {
        File.WriteAllText(SettingsPath, "invalid");
        Assert.Throws<JsonException>(() => new ClientHost(Simulation(), viewPreferencesPath: SettingsPath));
        Assert.Equal("invalid", File.ReadAllText(SettingsPath));
    }

    [Fact]
    public void InvalidSettingsCannotReplaceExistingArchive()
    {
        new ClientViewPreferences { BloodFadeScalar = 0.5 }.SaveFile(SettingsPath);
        var before = File.ReadAllText(SettingsPath);
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ClientViewPreferences { BloodFadeScalar = -1 }.SaveFile(SettingsPath));
        Assert.Equal(before, File.ReadAllText(SettingsPath));
        Assert.Single(Directory.GetFiles(_directory));
    }

    [Fact]
    public void FailedReplacementCleansTemporaryFileAndPropagatesError()
    {
        var target = Path.Combine(_directory, "existing-directory"); Directory.CreateDirectory(target);
        var error = Record.Exception(() => new ClientViewPreferences().SaveFile(target));
        Assert.True(error is IOException or UnauthorizedAccessException);
        Assert.Empty(Directory.GetFiles(_directory)); Assert.True(Directory.Exists(target));
    }

    [Fact]
    public void MissingParentDirectoryIsAnErrorRatherThanDefaultSettings()
    {
        var path = Path.Combine(_directory, "missing", "view.json");
        Assert.Throws<DirectoryNotFoundException>(() => ClientViewPreferences.LoadFile(path));
    }

    [Fact]
    public void FailedShutdownSaveStillDisposesHost()
    {
        var host = new ClientHost(Simulation(), viewPreferencesPath: SettingsPath);
        Directory.CreateDirectory(SettingsPath);
        var error = Record.Exception(host.Dispose);
        Assert.True(error is IOException or UnauthorizedAccessException);
        host.Dispose();
        Assert.Throws<ObjectDisposedException>(() => host.Tick(default));
        Assert.Empty(Directory.GetFiles(_directory));
    }
}
