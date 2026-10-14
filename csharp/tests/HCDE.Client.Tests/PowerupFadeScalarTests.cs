using HCDE.Playsim;

namespace HCDE.Client.Tests;

public class PowerupFadeScalarTests
{
    [Theory]
    [InlineData(0, 40, 30, 20)]
    [InlineData(0.5, 66, 26, 17)]
    [InlineData(2, 146, 15, 10)]
    [InlineData(100, 255, 0, 0)]
    public void ScalarControlsTintAlpha(double scalar, byte r, byte g, byte b)
    {
        var frame = new SoftwareFrame(1, 1); frame.Clear(40, 30, 20);
        SoftwarePowerupBlend.Apply(frame, 0x3fff0000u, scalar);
        Assert.Equal(new[] { r, g, b }, frame.Pixels);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void InvalidScalarRejectsWithoutMutatingPixelsOrHost(double scalar)
    {
        var frame = new SoftwareFrame(1, 1); frame.Clear(40, 30, 20);
        Assert.Throws<ArgumentOutOfRangeException>(() => SoftwarePowerupBlend.Apply(frame, 0, scalar));
        Assert.Equal(new byte[] { 40, 30, 20 }, frame.Pixels);
        using var host = ClientHost.DemoRoom(); var before = host.CaptureViewPreferences();
        Assert.Throws<ArgumentOutOfRangeException>(() => host.ApplyViewPreferences(new() { BloodFadeScalar = 0.5, PowerupFadeScalar = scalar }));
        Assert.Equal(before, host.CaptureViewPreferences());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void BothViewPathsHonorLocalScalarWithoutChangingGameplay(bool hardware)
    {
        using var host = ClientHost.DemoRoom(); host.GamePaused = true; host.UseHardware = hardware;
        host.Simulation.Players.Single().GivePowerStrength(); var saved = SimSavegame.Write(host.Simulation);
        host.PowerupFadeScalar = 0; host.Tick(default); var baseline = host.Frame.Pixels.ToArray();
        host.ApplyViewPreferences(ClientViewPreferences.Read(new ClientViewPreferences { PowerupFadeScalar = 2 }.Write()));
        host.Tick(default); Assert.NotEqual(baseline, host.Frame.Pixels);
        Assert.Equal(saved, SimSavegame.Write(host.Simulation));
        Assert.Equal(2, host.CaptureViewPreferences().PowerupFadeScalar);
    }

    [Fact]
    public void OlderPreferencesDefaultToOneAndNewValueRoundTrips()
    {
        Assert.Equal(1, ClientViewPreferences.Read("{}").PowerupFadeScalar);
        var preferences = new ClientViewPreferences { PowerupFadeScalar = 0.25 };
        Assert.Equal(preferences, ClientViewPreferences.Read(preferences.Write()));
    }
}
