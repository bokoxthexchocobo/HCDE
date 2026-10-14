using HCDE.MapLoader;
using HCDE.Playsim;

namespace HCDE.Client.Tests;

public class PickupFadeScalarTests
{
    [Theory]
    [InlineData(0, 0)]
    [InlineData(0.01, 0)]
    [InlineData(0.5, 24)]
    [InlineData(1, 48)]
    [InlineData(2, 96)]
    public void ScalingTruncatesBeforeAlphaCalculation(double scalar, int expected)
        => Assert.Equal(expected / 255f, SoftwarePlayerBlend.GetPickupAlpha(6, scalar));

    [Theory]
    [InlineData(128, 128f / 255)]
    [InlineData(129, 0.5f)]
    public void NativeHalfOpacityBranchStartsAbove128(int scaled, float expected)
        => Assert.Equal(expected, SoftwarePlayerBlend.GetPickupAlpha(1, scaled / 8.0));

    [Fact]
    public void HugeFiniteScaleSaturatesWithoutIntegerOverflow()
        => Assert.Equal(0.5f, SoftwarePlayerBlend.GetPickupAlpha(6, double.MaxValue));

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void BothViewPathsHonorArchivedLocalSetting(bool hardware)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1 }, new LevelThing { Type = PickupCatalog.HealthBonus }]
        });
        using var host = new ClientHost(sim); host.UseHardware = hardware; host.Tick(default); host.GamePaused = true;
        var tinted = host.Frame.Pixels.ToArray(); var saved = SimSavegame.Write(sim, true);
        host.ApplyViewPreferences(ClientViewPreferences.Read(new ClientViewPreferences { PickupFadeScalar = 0 }.Write()));
        host.Tick(default); Assert.NotEqual(tinted, host.Frame.Pixels);
        Assert.Equal(saved, SimSavegame.Write(sim, true)); Assert.Equal(6, sim.Players.Single().BonusCount);
        Assert.Equal(0, host.CaptureViewPreferences().PickupFadeScalar);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void InvalidScaleDoesNotPartiallyApplyPreferences(double scalar)
    {
        using var host = ClientHost.DemoRoom(); var before = host.CaptureViewPreferences();
        Assert.Throws<ArgumentOutOfRangeException>(() => host.ApplyViewPreferences(new() { BloodFadeScalar = 0, PickupFadeScalar = scalar }));
        Assert.Equal(before, host.CaptureViewPreferences());
        Assert.Throws<ArgumentOutOfRangeException>(() => SoftwarePlayerBlend.GetPickupAlpha(6, scalar));
    }

    [Fact]
    public void OlderPreferencesUseDefaultIntensity()
        => Assert.Equal(1, ClientViewPreferences.Read("{}").PickupFadeScalar);
}
