using HCDE.MapLoader;
using HCDE.Playsim;

namespace HCDE.Client.Tests;

public class LauncherViewPreferencesTests
{
    [Fact]
    public void PathWithSpacesSurvivesArgumentRoundTrip()
    {
        var selection = new LauncherSelection { IwadPath = "doom.wad", ViewPreferencesPath = "settings folder/view.json" };
        var args = LauncherPlan.BuildArgs(selection);
        Assert.True(LauncherPlan.TryParse(args, out var parsed, out var error), error);
        Assert.Equal(selection.ViewPreferencesPath, parsed.ViewPreferencesPath);
    }

    [Theory]
    [InlineData()]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("--renderer", "hardware")]
    public void MissingOrBlankPathIsRejected(params string[] values)
    {
        Assert.False(LauncherPlan.TryParse(["--iwad", "doom.wad", "--view-preferences", .. values], out _, out var error));
        Assert.Equal("view-preferences-path", error);
    }

    [Fact]
    public void OmittedPathPreservesExistingArguments()
    {
        var args = LauncherPlan.BuildArgs(new LauncherSelection { IwadPath = "doom.wad" });
        Assert.DoesNotContain("--view-preferences", args);
        Assert.True(LauncherPlan.TryParse(args, out var selection, out _));
        Assert.Null(selection.ViewPreferencesPath);
        Assert.False(LauncherPlan.TryValidate(new() { IwadPath = "doom.wad", ViewPreferencesPath = " " }, out var error));
        Assert.Equal("view-preferences-path", error);
    }

    [Fact]
    public void SelectionCreatesConfiguredHostAndPersistsPreferences()
    {
        var path = Path.Combine(Path.GetTempPath(), "hcde-launch-view-" + Guid.NewGuid().ToString("N") + ".json");
        try
        {
            File.WriteAllBytes(path + ".wad", [80,87,65,68,0,0,0,0,12,0,0,0]);
            new ClientViewPreferences { SmoothPain = true, BloodFadeScalar = 0.5 }.SaveFile(path);
            var args = LauncherPlan.BuildArgs(new() { IwadPath = path + ".wad", ViewPreferencesPath = path,
                Width = 64, Height = 48, SoftwareRenderer = false });
            Assert.True(LauncherPlan.TryParse(args, out var selection, out var error), error);
            var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 128 }],
                Things = [new LevelThing { Type = 1 }] });
            using (var host = LauncherPlan.CreateHost(selection, sim))
            {
                Assert.Same(sim, host.Simulation); Assert.True(host.UseHardware);
                Assert.Equal(64, host.Frame.Width); Assert.Equal(48, host.Frame.Height);
                Assert.True(host.SmoothPain); Assert.Equal(0.5, host.BloodFadeScalar);
                host.BloodFadeScalar = 0.75;
            }
            Assert.Equal(0.75, ClientViewPreferences.LoadFile(path).BloodFadeScalar);
        }
        finally { if (File.Exists(path)) File.Delete(path); File.Delete(path + ".wad"); }
    }
}
