using HCDE.MapLoader;
using HCDE.Playsim;

namespace HCDE.Client.Tests;

public class SoftwarePainBlendTests
{
    [Theory]
    [InlineData(0, false, 1, 0)]
    [InlineData(25, false, 1, 115)]
    [InlineData(25, true, 1, 71)]
    [InlineData(25, true, 0.5, 35)]
    [InlineData(25, true, 0, 0)]
    public void RecordedHardwareAppliesPainAfterWorldBlit(int damage, bool smooth, double fade, int alpha)
    {
        using var host = ClientHost.DemoRoom();
        var player = host.Simulation.Players.Single(); player.NoPain = true;
        if (damage > 0) ActorDamage.Apply(player, damage);
        host.UseHardware = true; host.GamePaused = true;
        host.SmoothPain = smooth; host.PainSmoothStrength = 1; host.BloodFadeScalar = fade;
        var saved = SimSavegame.Write(host.Simulation);
        host.Tick(default);
        host.Frame.Get(0, 0, out var backgroundRed, out var backgroundGreen, out var backgroundBlue);
        Assert.Equal(alpha, backgroundRed); Assert.Equal(0, backgroundGreen); Assert.Equal(0, backgroundBlue);
        host.Frame.Get(host.Frame.Width / 2, host.Frame.Height / 2, out var r, out var g, out var b);
        Assert.Equal((180 * (255 - alpha) + 255 * alpha) / 255, r);
        Assert.Equal(40 * (255 - alpha) / 255, g); Assert.Equal(g, b);
        Assert.Equal(damage, player.DamageCount);
        Assert.Equal(saved, SimSavegame.Write(host.Simulation));
        Assert.Equal(ClientHost.HardwareWall, Assert.Single(host.Hardware.Quads).Argb);
        var pixels = host.Frame.Pixels.ToArray(); host.Tick(default);
        Assert.Equal(pixels, host.Frame.Pixels);
    }

    [Fact]
    public void RecordedHardwareWithoutPlayerStillDrawsWorld()
    {
        using var host = new ClientHost(AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 512 }] }), 4, 4);
        host.UseHardware = true; host.GamePaused = true; host.Tick(default);
        host.Frame.Get(2, 2, out var r, out var g, out var b);
        Assert.Equal(180, r); Assert.Equal(40, g); Assert.Equal(40, b);
    }

    [Theory]
    [InlineData(25, true, 1f, 1, 71)]
    [InlineData(25, true, 1f, 0.5, 35)]
    [InlineData(25, true, 0f, 1, 115)]
    [InlineData(25, false, 1f, 1, 115)]
    [InlineData(0, true, 1f, 1, 0)]
    [InlineData(100, true, 1f, 0.5, 113)]
    [InlineData(75, true, 1f, 1, 175)]
    public void SmoothingMatchesNativeRatioAndTruncation(int damage, bool enabled, float strength, double fade, int expected)
    {
        var frame = new SoftwareFrame(1, 1); frame.Clear(0, 0, 0);
        SoftwarePainBlend.Apply(frame, damage, fadeScalar: fade, smoothPain: enabled, smoothStrength: strength);
        Assert.Equal(new byte[] { (byte)expected, 0, 0 }, frame.Pixels);
    }

    [Fact]
    public void SmoothingRatioUsesRawCounterForTranslucentPainColor()
    {
        var frame = new SoftwareFrame(1, 1); frame.Clear(0, 0, 0);
        SoftwarePainBlend.Apply(frame, 25, 0x8000ff00u, smoothPain: true, smoothStrength: 1);
        Assert.Equal(new byte[] { 0, 44, 0 }, frame.Pixels);
    }

    [Fact]
    public void SmoothingPreferencesClampAndRenderingPreservesSave()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 512 }], Things = [new LevelThing { Type = 1 }] });
        var player = sim.Players.Single(); player.NoPain = true; ActorDamage.Apply(player, 25);
        using var host = new ClientHost(sim, 2, 2);
        Assert.False(host.SmoothPain); Assert.Equal(0.6f, host.PainSmoothStrength);
        host.PainSmoothStrength = -1; Assert.Equal(0, host.PainSmoothStrength);
        host.PainSmoothStrength = 2; Assert.Equal(1, host.PainSmoothStrength);
        Assert.Throws<ArgumentOutOfRangeException>(() => host.PainSmoothStrength = float.NaN);
        Assert.Equal(1, host.PainSmoothStrength);
        var saved = SimSavegame.Write(sim);
        SoftwareView.Draw(sim, host.Frame, smoothPain: true, smoothStrength: host.PainSmoothStrength);
        host.Frame.Get(0, 0, out var red, out _, out _);
        Assert.Equal((40 * (255 - 71) + 255 * 71) / 255, red);
        Assert.Equal(saved, SimSavegame.Write(sim));
    }

    [Theory]
    [InlineData(20, 0, 0)]
    [InlineData(20, 0.5, 50)]
    [InlineData(100, 0.5, 113)]
    [InlineData(20, 2, 175)]
    public void FadeScalarAppliesAfterCurveBeforeCap(int count, double scalar, int expected)
    {
        var frame = new SoftwareFrame(1, 1); frame.Clear(0, 0, 0);
        SoftwarePainBlend.Apply(frame, count, fadeScalar: scalar);
        Assert.Equal(new byte[] { (byte)expected, 0, 0 }, frame.Pixels);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void InvalidFadeScalarIsRejectedWithoutChangingPixels(double scalar)
    {
        var frame = new SoftwareFrame(1, 1); frame.Clear(40, 30, 20);
        Assert.Throws<ArgumentOutOfRangeException>(() => SoftwarePainBlend.Apply(frame, 20, fadeScalar: scalar));
        Assert.Equal(new byte[] { 40, 30, 20 }, frame.Pixels);
    }

    [Fact]
    public void ViewCanDisablePainFadeWithoutChangingDamageCounter()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 512 }], Things = [new LevelThing { Type = 1 }] });
        var player = sim.Players.Single(); player.NoPain = true; ActorDamage.Apply(player, 20);
        var frame = new SoftwareFrame(2, 2);
        SoftwareView.Draw(sim, frame, bloodFadeScalar: 0);
        frame.Get(0, 0, out var r, out var g, out var b);
        Assert.Equal(SoftwareView.CeilingR, r);
        Assert.Equal(SoftwareView.CeilingG, g);
        Assert.Equal(SoftwareView.CeilingB, b);
        Assert.Equal(20, player.DamageCount);
        using var host = new ClientHost(sim);
        Assert.Equal(1, host.BloodFadeScalar);
        host.BloodFadeScalar = 0;
        Assert.Throws<ArgumentOutOfRangeException>(() => host.BloodFadeScalar = double.NaN);
        Assert.Equal(0, host.BloodFadeScalar);
    }

    [Theory]
    [InlineData(0x0000ff00u, 0)]
    [InlineData(0xff00ff00u, 100)]
    [InlineData(0x8000ff00u, 62)]
    public void ColorOpacityScalesCounterBeforeNativeCurve(uint color, int alpha)
    {
        var frame = new SoftwareFrame(1, 1); frame.Clear(0, 0, 0);
        SoftwarePainBlend.Apply(frame, 20, color);
        Assert.Equal(new byte[] { 0, (byte)alpha, 0 }, frame.Pixels);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 8)]
    [InlineData(20, 100)]
    [InlineData(100, 175)]
    public void BlendUsesNativeCurveAndMaximum(int damage, int alpha)
    {
        var frame = new SoftwareFrame(1, 1); frame.Clear(40, 30, 20);
        SoftwarePainBlend.Apply(frame, damage);
        Assert.Equal(new byte[] { (byte)((40 * (255 - alpha) + 255 * alpha) / 255),
            (byte)(30 * (255 - alpha) / 255), (byte)(20 * (255 - alpha) / 255) }, frame.Pixels);
    }

    [Fact]
    public void ViewAppliesPainWithoutChangingAuthoritativeCounter()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 512 }], Things = [new LevelThing { Type = 1 }] });
        var player = sim.Players.Single(); player.NoPain = true; ActorDamage.Apply(player, 20);
        var saved = SimSavegame.Write(sim); var frame = new SoftwareFrame(2, 2);
        SoftwareView.Draw(sim, frame); Assert.Equal(20, player.DamageCount);
        Assert.Equal(saved, SimSavegame.Write(sim));
        frame.Get(0, 0, out var r, out var g, out var b);
        Assert.True(r > SoftwareView.CeilingR); Assert.True(g < SoftwareView.CeilingG); Assert.True(b < SoftwareView.CeilingB);
    }
}
